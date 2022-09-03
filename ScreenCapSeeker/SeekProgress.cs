using System;
using System.Drawing;

namespace ScreenCapSeeker {
	public class SeekProgress : IDisposable {
		private long nFrame;
		private Bitmap bmpMain;
		private Bitmap bmpMatch;
		private Bitmap bmpWorkMain;
		private Bitmap bmpWorkMatch;

		public long Frame => nFrame;

		public Bitmap Main {
			get { return bmpMain; }
		}

		public Bitmap Match {
			get { return bmpMatch; }
		}

		public Bitmap WorkMain {
			get { return bmpWorkMain; }
		}

		public Bitmap WorkMatch {
			get { return bmpWorkMatch; }
		}

		public void SetSnapshot(long frame, SeekDataFrame main, SeekDataFrame clip, byte factor) {
			nFrame = frame;

			bmpMain?.Dispose();
			bmpMain = main.Bitmap.ToBitmap();

			bmpMatch?.Dispose();
			bmpMatch = main.Bitmap.ToBitmap();

			bmpWorkMain?.Dispose();
			bmpWorkMain = main.Get( factor ).ToBitmap();

			bmpWorkMatch?.Dispose();
			bmpWorkMatch = clip.Get( factor ).ToBitmap();
		}

		public void SetPreview(SeekDataFrame main) {
			bmpMain?.Dispose();
			bmpMain = main.Bitmap.ToBitmap();
		}

		public void Dispose() {
			bmpMain?.Dispose();
			bmpMatch?.Dispose();
			bmpWorkMain?.Dispose();
			bmpWorkMatch?.Dispose();
		}
	}
}
