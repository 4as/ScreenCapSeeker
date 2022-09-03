using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScreenCapSeeker {

	public class SeekDataBitmap {
		private const byte BYTES_PER_PIXEL = 3;

		private readonly byte[] arrBytes;
		private readonly ushort nWidth;
		private readonly ushort nHeight;
		private readonly int nStride;
		private readonly int nSize;

		public SeekDataBitmap(byte[] frame_data, ushort frame_width, ushort frame_height, int frame_stride) {
			arrBytes = frame_data;
			nWidth = frame_width;
			nHeight = frame_height;
			nStride = frame_stride;
			nSize = nStride * nHeight;
		}

		public ushort Width => nWidth;
		public ushort Height => nHeight;
		public byte[] Data => arrBytes;

		public Bitmap ToBitmap() {
			Bitmap bmp = new Bitmap( nWidth, nHeight, PixelFormat.Format24bppRgb );
			BitmapData bd = bmp.LockBits( new Rectangle( 0, 0, nWidth, nHeight ), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb );
			Marshal.Copy( arrBytes, 0, bd.Scan0, nSize );
			bmp.UnlockBits( bd );
			return bmp;
		}

		public ulong Compare(SeekDataBitmap other_bitmap) {
			ulong difference = 0;
			int idx = 0;
			while( idx < nSize ) {
				byte pixel_source = arrBytes[idx];
				byte pixel_target = other_bitmap.arrBytes[idx];
				difference += (byte)Math.Abs( pixel_source - pixel_target );
				idx++;
			}

			return difference;
		}

		public SeekDataBitmap Crush(ushort new_width, ushort new_height) {
			double block_width = (double)nWidth / new_width;
			double block_height = (double)nHeight / new_height;
			int stride = new_width * BYTES_PER_PIXEL;
			byte[] result = new byte[stride * new_height];


			IEnumerable<int> itr = System.Linq.Enumerable.Range( 0, new_width * new_height );
			Parallel.ForEach( itr, i => {
				ushort x = (ushort)(i % new_width);
				ushort y = (ushort)(i / new_width);
				Span<byte> pixel = result.AsSpan( (y * stride) + (x * BYTES_PER_PIXEL), BYTES_PER_PIXEL );
				AverageArea( (ushort)(x * block_width), (ushort)(y * block_height), (ushort)block_width, (ushort)block_height, pixel );
			} );

			return new SeekDataBitmap( result, new_width, new_height, stride );
		}

		private void AverageArea(ushort start_x, ushort start_y, ushort block_width, ushort block_height, in Span<byte> output) {
			uint[] sum = new uint[BYTES_PER_PIXEL];
			ushort len = start_y + block_height >= nHeight ? (ushort)(nHeight - start_y) : block_height;
			for( ushort row = 0; row < len; row++ ) {
				AverageRow( start_x, (ushort)(start_y + row), block_width, in sum );
			}

			for( byte bit = 0; bit < BYTES_PER_PIXEL; bit++ ) {
				output[bit] = (byte)(sum[bit] / len);
			}
		}

		private void AverageRow(ushort start_x, ushort start_y, ushort scan_size, in uint[] output) {
			int start = (start_y * nStride) + (start_x * BYTES_PER_PIXEL);
			int end_x = start_x + scan_size;
			ushort len = end_x >= nWidth ? (ushort)(nWidth - start_x) : scan_size;
			int steps = len * BYTES_PER_PIXEL;

			uint[] sum = new uint[BYTES_PER_PIXEL];
			for( int i = 0; i < steps; i += BYTES_PER_PIXEL ) {
				for( int bit = 0; bit < BYTES_PER_PIXEL; bit++ ) {
					sum[bit] += arrBytes[start + i + bit];
				}
			}

			for( int bit = 0; bit < BYTES_PER_PIXEL; bit++ ) {
				output[bit] += (sum[bit] / len);
			}
		}


		public static SeekDataBitmap FromFrame(FFmpeg.AutoGen.AVFrame frame) {
			int stride = frame.linesize[0];
			byte[] bytes = new byte[stride * frame.height];
			unsafe {
				Marshal.Copy( (IntPtr)frame.data[0], bytes, 0, bytes.Length );
			}
			return new SeekDataBitmap( bytes, (ushort)frame.width, (ushort)frame.height, stride );
		}

		public static SeekDataBitmap FromImage(Bitmap bitmap) {
			BitmapData bd = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb );
			int stride = bd.Stride;
			byte[] bytes = new byte[stride * bd.Height];
			unsafe {
				Marshal.Copy( bd.Scan0, bytes, 0, bytes.Length );
			}
			bitmap.UnlockBits( bd );
			return new SeekDataBitmap( bytes, (ushort)bitmap.Width, (ushort)bitmap.Height, stride );
		}
	}
}
