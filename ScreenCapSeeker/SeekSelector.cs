using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	public class SeekSelector : Panel {
		private readonly SeekFFMPEG mediaClip = new SeekFFMPEG();
		private readonly SeekPicture seekPicture = new SeekPicture();
		private readonly TrackBar uiScroll = new TrackBar();
		private readonly Button buttonAccept = new Button();
		private SeekDataBitmap dataFrame;
		private Bitmap bitmapFrame;
		public SeekSelector() {
			seekPicture.BorderStyle = BorderStyle.FixedSingle;
			seekPicture.SizeMode = PictureBoxSizeMode.Zoom;
			seekPicture.interpolation = InterpolationMode.NearestNeighbor;
			Controls.Add( seekPicture );

			uiScroll.ValueChanged += OnSeek;
			Controls.Add( uiScroll );

			buttonAccept.Text = "OK";
			buttonAccept.Click += OnAccept;
			Controls.Add( buttonAccept );

			Resize += OnResized;
			OnResized( null, null );
		}

		public SeekDataBitmap Frame => dataFrame;

		public void Open(string path) {
			mediaClip.Open( path );
			uiScroll.Minimum = 0;
			uiScroll.Maximum = (int)(mediaClip.Duration);
			uiScroll.Value = 0;
			uiScroll.SmallChange = 1;
			if( uiScroll.Maximum < 10 ) {
				uiScroll.LargeChange = 1;
			}
			else if( uiScroll.Maximum < 300 ) {
				uiScroll.LargeChange = 5;
			}
			else {
				uiScroll.LargeChange = 10;
			}


			UpdatePreview();
		}

		public void Close() {
			mediaClip.Dispose();
		}

		public event Action EventAccepted;

		private void UpdatePreview() {
			if( bitmapFrame != null ) bitmapFrame.Dispose();
			dataFrame = mediaClip.ReadFrame();
			bitmapFrame = dataFrame.ToBitmap();
			seekPicture.Image = bitmapFrame;
		}

		private void OnResized(object sender, EventArgs e) {
			seekPicture.Size = new Size( Width, Height - 30 );

			uiScroll.Location = new Point( 0, Height - 30 );
			uiScroll.Size = new Size( Width - 60, 30 );

			buttonAccept.Location = new Point( Width - 60, Height - 30 );
			buttonAccept.Size = new Size( 60, 30 );
		}

		private void OnSeek(object sender, EventArgs e) {
			double v = (double)uiScroll.Value / (double)((int)(mediaClip.Duration));
			int frame = (int)(v * mediaClip.TotalFrames);
			mediaClip.SeekTo( frame );
			UpdatePreview();
		}

		private void OnAccept(object sender, EventArgs e) {
			EventAccepted?.Invoke();
		}
	}
}
