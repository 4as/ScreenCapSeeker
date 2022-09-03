using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	public class App : Form {

		private readonly TextBox textInfo = new TextBox();
		private readonly TextBox textProgress = new TextBox();
		private readonly SeekBrowse seekBrowse = new SeekBrowse();
		private readonly SeekPreview seekPreview = new SeekPreview();
		private readonly SeekSelector seekSelector = new SeekSelector();
		private readonly Button buttonCancel = new Button();

		private Seeker seekSeeker;

		public App() : base() {
			Text = "Screencap Seeker";
			MinimumSize = new System.Drawing.Size( 300, 200 );

			textInfo.Multiline = true;
			textInfo.Enabled = false;
			textInfo.ReadOnly = true;
			textInfo.BorderStyle = BorderStyle.None;
			textInfo.TextAlign = HorizontalAlignment.Center;
			textInfo.Font = new Font( textInfo.Font.FontFamily, 24, FontStyle.Bold );

			textProgress.Multiline = false;
			textProgress.Enabled = false;
			textProgress.ReadOnly = true;
			textProgress.Text = "Pick two media files to begin the matching process";
			textProgress.BorderStyle = BorderStyle.None;
			textProgress.Height = 30;
			Controls.Add( textProgress );

			buttonCancel.Text = "CANCEL";
			buttonCancel.Click += OnCancel;

			seekPreview.Location = new System.Drawing.Point( 0, 0 );

			seekSelector.EventAccepted += OnStartWithClip;

			seekBrowse.EventStart += OnStart;
			Controls.Add( seekBrowse );

			Resize += OnResized;
			Size = new Size( 600, 400 );

			Load += OnLoaded;
		}

		private string FormatBestMatch(long frame) {
			double percent = (double)frame / seekSeeker.TotalFrames;
			double ticks = seekSeeker.Info.duration * percent;
			TimeSpan time = TimeSpan.FromMilliseconds( ticks * 1000 );
			return time.ToString() + ", frame: " + seekSeeker.Progress.Frame + "/" + seekSeeker.TotalFrames;
		}

		private void End(string summary) {
			seekSeeker.Stop();
			Controls.Remove( seekBrowse );
			Controls.Remove( buttonCancel );
			Controls.Remove( textProgress );
			Controls.Add( textInfo );
			Controls.SetChildIndex( textInfo, 0 );
			summary = summary.Replace( "\n", System.Environment.NewLine );
			textInfo.Text = summary;
		}

		private void OnLoaded(object sender, EventArgs e) {
			seekSeeker = new Seeker();
			if( !seekSeeker.IsValid ) {
				End( "Failed to initialize.\nMake sure 'ffmpeg' directory is not empty and contains proper FFMPEG binaries.\nCheck out the 'readme' for usage details." );
			}
		}

		private void OnStart() {
			Controls.Remove( seekBrowse );
			Controls.Add( seekSelector );
			Controls.SetChildIndex( seekSelector, 0 );
			if( seekBrowse.IsMatchingMovie ) {
				seekSelector.Open( seekBrowse.PathMatch );
			}
			else {
				OnStartWithClip();
			}
		}

		private void OnStartWithClip() {
			Controls.Remove( seekSelector );
			Controls.Add( buttonCancel );
			Controls.Add( seekPreview );
			Controls.Add( textProgress );

			SeekDataBitmap bitmap;
			if( seekBrowse.IsMatchingMovie ) {
				bitmap = seekSelector.Frame;
				seekSelector.Close();
			}
			else {
				var bmp = new Bitmap( Image.FromFile( seekBrowse.PathMatch ) );
				bitmap = SeekDataBitmap.FromImage( bmp );
				bmp.Dispose();
			}

			textProgress.Text = "Opening, please wait...";
			seekSeeker.Open( seekBrowse.PathMain, bitmap );

			seekSeeker.EventProgress += OnProgress;
			seekSeeker.EventCompleted += onCompleted;
			seekSeeker.Start();
		}

		private void OnCancel(object sender, EventArgs e) {
			End( "Processing canceled.\nBest match found: " + FormatBestMatch( seekSeeker.Progress.Frame ) );
		}

		private void OnProgress(int percent, SeekProgress progress) {
			seekPreview.SetProgress( progress );
			textProgress.Text = "" + percent + "% " + seekSeeker.CurrentFrame + "/" + seekSeeker.TotalFrames + " (Current best match on: " + FormatBestMatch( progress.Frame ) + ")";
		}

		private void onCompleted(RunWorkerCompletedEventArgs e) {
			if( e.Error != null ) {
				End( "Processing failed.\nError: " + e.Error.Message );
			}
			else if( e.Cancelled ) {
				End( "Processing canceled.\nBest match found on: " + FormatBestMatch( seekSeeker.Progress.Frame ) );
			}
			else {
				End( "COMPLETED.\nBest match found on: " + FormatBestMatch( seekSeeker.Progress.Frame ) );
				seekPreview.SetProgress( seekSeeker.Progress, null );
			}
		}



		private void OnResized(object sender, EventArgs e) {
			int w = ClientSize.Width;
			int h = ClientSize.Height;
			textInfo.Size = new Size( w, h/2 );
			seekPreview.Size = new Size( w, h - textProgress.Height );
			seekBrowse.Width = w;
			seekBrowse.Location = new Point( 0, (h / 2) - (seekBrowse.Height / 2) );
			textProgress.Width = w;
			textProgress.Location = new Point( 0, h - textProgress.Height );
			buttonCancel.Location = new Point( w / 2 - buttonCancel.Width / 2, h / 2 - buttonCancel.Height / 2 );
			seekSelector.Size = new Size( w, h );
		}
	}
}
