using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;

namespace ScreenCapSeeker {
	public class SeekFFMPEG {

		private VideoStreamDecoder decoder;
		private VideoFrameConverter converter;
		private IReadOnlyDictionary<string, string> info;

		private long nFrame = 0;
		private long nTotalFrames = 0;
		private double nDuration = 0;
		private double nRate = 0.0;

		public SeekFFMPEG() { }

		public long CurrentFrame => nFrame;
		public long TotalFrames => nTotalFrames;
		public IReadOnlyDictionary<string, string> Metadata => info;
		public double Duration => nDuration;
		public double FrameRate => nRate;

		public void Open(string url) {
			if( !SeekFFMPEGLibrary.isInit ) throw new InvalidOperationException( "Can't open url: " + url + ", SeekFFMPEGLibrary has not been initialized." );

			decoder = new VideoStreamDecoder( url, SeekFFMPEGLibrary.deviceType );
			info = decoder.GetContextInfo();

			var sourceSize = decoder.FrameSize;
			var sourcePixelFormat = SeekFFMPEGLibrary.pixelFormat;
			var destinationSize = sourceSize;
			var destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_BGR24;
			converter = new VideoFrameConverter( sourceSize, sourcePixelFormat, destinationSize, destinationPixelFormat );

			nFrame = 0;
			nDuration = decoder.Duration;
			nRate = (double)decoder.Stream.r_frame_rate.num / (double)decoder.Stream.r_frame_rate.den;
			nTotalFrames = (long)(nDuration * nRate);
		}

		public SeekDataBitmap ReadFrame() {
			if( decoder == null ) throw new InvalidOperationException( "Can't read frame, no video has been opened yet." );

			bool has_frame = decoder.TryDecodeNextFrame( out var frame );
			if( !has_frame ) {
				return null;
			}

			nFrame++;

			return SeekDataBitmap.FromFrame( converter.Convert( frame ) );
		}

		public void SeekTo(long frame) {
			if( decoder == null ) throw new InvalidOperationException( "Can't seek to a frame: " + frame + ", no video has been opened yet." );

			bool success = decoder.TrySeekFrame( frame );
			if( success ) {
				nFrame = frame;
			}
		}

		public void Dispose() {
			if( decoder == null ) return;
			decoder.Dispose();
			decoder = null;
			nFrame = 0;
		}
	}
}
