using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	public class SeekPicture : PictureBox {
		public InterpolationMode interpolation = InterpolationMode.Default;
		public PixelOffsetMode offsetMode = PixelOffsetMode.Half;

		public SeekPicture() {
			BackColor = Color.Black;
		}

		protected override void OnPaint(PaintEventArgs paintEventArgs) {
			paintEventArgs.Graphics.InterpolationMode = interpolation;
			paintEventArgs.Graphics.PixelOffsetMode = offsetMode;
			try {
				base.OnPaint( paintEventArgs );
			}
			catch( Exception ) {

			}
		}
	}
}
