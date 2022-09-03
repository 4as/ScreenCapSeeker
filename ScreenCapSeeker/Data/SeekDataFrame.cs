using System;
using System.Collections.Generic;
using System.Drawing;

namespace ScreenCapSeeker {

	public class SeekDataFrame {
		private readonly SortedList<uint, SeekDataBitmap> listFactors = new SortedList<uint, SeekDataBitmap>();
		private SeekDataBitmap seekBitmap;
		private byte nMaxFactor;

		public SeekDataFrame() { }

		public byte MaxFactor => nMaxFactor;
		public ushort Width => seekBitmap.Width;
		public ushort Height => seekBitmap.Height;

		public SeekDataBitmap Bitmap {
			get { return seekBitmap; }
			set {
				seekBitmap = value;

				listFactors.Clear();
				listFactors.Add( GetKey( seekBitmap.Width, seekBitmap.Height ), seekBitmap );

				double log2 = Math.Log( 2 );
				byte max_w = (byte)Math.Ceiling( Math.Log( seekBitmap.Width ) / log2 );
				byte max_h = (byte)Math.Ceiling( Math.Log( seekBitmap.Height ) / log2 );
				nMaxFactor = max_w > max_h ? max_h : max_w;
			}
		}

		public bool Has(byte factor) {
			return factor <= nMaxFactor;
		}

		public SeekDataBitmap Get(byte factor) {
			return Get( GetSize( factor ) );
		}

		public SeekDataBitmap Get(Size size) {
			return Get( (ushort)size.Width, (ushort)size.Height );
		}

		public SeekDataBitmap Get(ushort width, ushort height) {
			Size size = new Size( width, height );
			uint key = GetKey( size );
			if( listFactors.ContainsKey( key ) ) {
				listFactors.TryGetValue( key, out var value );
				return value;
			}

			SeekDataBitmap result = seekBitmap.Crush( width, height );
			listFactors.Add( key, result );
			return result;
		}

		public Size GetSize(byte factor) {
			if( factor > nMaxFactor ) {
				factor = nMaxFactor;
			}

			ushort w, h;
			if( factor == 0 || factor == nMaxFactor ) {
				w = seekBitmap.Width;
				h = seekBitmap.Height;
			}
			else {
				double divi = Math.Pow( 2, factor );
				w = divi > seekBitmap.Width ? seekBitmap.Width : (ushort)divi;
				h = divi > seekBitmap.Height ? seekBitmap.Height : (ushort)divi;
			}
			return new Size( w, h );
		}

		public ulong Compare(byte factor, SeekDataFrame other_frame) {
			if( !Has( factor ) || !other_frame.Has( factor ) ) return ulong.MaxValue;

			Size size = GetSize( factor );
			if( size.Width > other_frame.Width || size.Height > other_frame.Height ) {
				size = other_frame.GetSize( factor );
			}

			SeekDataBitmap source = Get( size );
			SeekDataBitmap target = other_frame.Get( size );

			return source.Compare( target );
		}

		private uint GetKey(Size size) {
			return GetKey( (ushort)size.Width, (ushort)size.Height );
		}
		private uint GetKey(ushort width, ushort height) {
			return (uint)((width << 16) + (height));
		}
	}
}
