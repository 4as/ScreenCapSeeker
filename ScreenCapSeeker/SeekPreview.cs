using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	public class SeekPreview : Panel {

		private readonly SeekPicture seekPreviewMain = new SeekPicture();
		private readonly SeekPicture seekPreviewClip = new SeekPicture();
		private readonly SeekPicture seekWorkMain = new SeekPicture();
		private readonly SeekPicture seekWorkClip = new SeekPicture();
		private readonly SeekPicture seekWorkDifference = new SeekPicture();

		public SeekPreview() {
			seekPreviewMain.BorderStyle = BorderStyle.None;
			seekPreviewMain.SizeMode = PictureBoxSizeMode.Zoom;
			Controls.Add( seekPreviewMain );

			seekPreviewClip.BorderStyle = BorderStyle.None;
			seekPreviewClip.SizeMode = PictureBoxSizeMode.Zoom;
			Controls.Add( seekPreviewClip );

			seekWorkMain.BorderStyle = BorderStyle.FixedSingle;
			seekWorkMain.SizeMode = PictureBoxSizeMode.Zoom;
			seekWorkMain.interpolation = InterpolationMode.NearestNeighbor;
			Controls.Add( seekWorkMain );

			seekWorkClip.BorderStyle = BorderStyle.FixedSingle;
			seekWorkClip.SizeMode = PictureBoxSizeMode.Zoom;
			seekWorkClip.interpolation = InterpolationMode.NearestNeighbor;
			seekWorkClip.Click += OnSwitch;
			Controls.Add( seekWorkClip );

			seekWorkDifference.BorderStyle = BorderStyle.FixedSingle;
			seekWorkDifference.SizeMode = PictureBoxSizeMode.Zoom;
			seekWorkDifference.interpolation = InterpolationMode.NearestNeighbor;
			seekWorkDifference.Visible = false;
			seekWorkDifference.Click += OnSwitch;
			Controls.Add( seekWorkDifference );

			Resize += OnResized;
			OnResized( null, null );
		}

		private void OnSwitch(object sender, EventArgs e) {
			if( seekWorkDifference.Image == null && seekWorkClip.Visible ) return;
			seekWorkClip.Visible = !seekWorkClip.Visible;
			seekWorkDifference.Visible = !seekWorkClip.Visible;
		}

		public void SetProgress(SeekProgress progress, Bitmap difference = null) {
			seekPreviewMain.Image = progress.Main;
			seekPreviewClip.Image = progress.Match;
			seekWorkMain.Image = progress.WorkMain;
			seekWorkClip.Image = progress.WorkMatch;

			seekWorkClip.Visible = (difference == null);
			seekWorkDifference.Image = difference;
			seekWorkDifference.Visible = (difference != null);
		}

		private void OnResized(object sender, EventArgs e) {
			var size = new Size( Width / 2, Height / 2 );
			seekPreviewMain.Size = size;

			seekPreviewClip.Location = new Point( size.Width, 0 );
			seekPreviewClip.Size = size;

			seekWorkMain.Location = new Point( 0, size.Height );
			seekWorkMain.Size = size;

			seekWorkClip.Location = new Point( size.Width, size.Height );
			seekWorkClip.Size = size;

			seekWorkDifference.Location = seekWorkClip.Location;
			seekWorkDifference.Size = seekWorkClip.Size;
		}
	}
}
