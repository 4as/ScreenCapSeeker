using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenCapSeeker {
	public sealed unsafe class VideoStreamDecoder : IDisposable {
		private readonly AVCodecContext* _pCodecContext;
		private readonly AVFormatContext* _pFormatContext;
		private readonly int _streamIndex;
		private readonly AVFrame* _pFrame;
		private readonly AVFrame* _receivedFrame;
		private readonly AVPacket* _pPacket;
		private long _currentFrame;

		public VideoStreamDecoder(string url, AVHWDeviceType HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE) {
			_pFormatContext = ffmpeg.avformat_alloc_context();
			_receivedFrame = ffmpeg.av_frame_alloc();
			var pFormatContext = _pFormatContext;
			ffmpeg.avformat_open_input( &pFormatContext, url, null, null ).ThrowExceptionIfError();
			ffmpeg.avformat_find_stream_info( _pFormatContext, null ).ThrowExceptionIfError();
			AVCodec* codec = null;
			_streamIndex = ffmpeg.av_find_best_stream( _pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0 ).ThrowExceptionIfError();
			_pCodecContext = ffmpeg.avcodec_alloc_context3( codec );
			if( HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE ) {
				ffmpeg.av_hwdevice_ctx_create( &_pCodecContext->hw_device_ctx, HWDeviceType, null, null, 0 ).ThrowExceptionIfError();
			}
			Stream = *_pFormatContext->streams[_streamIndex];
			ffmpeg.avcodec_parameters_to_context( _pCodecContext, Stream.codecpar ).ThrowExceptionIfError();
			ffmpeg.avcodec_open2( _pCodecContext, codec, null ).ThrowExceptionIfError();

			CodecName = ffmpeg.avcodec_get_name( codec->id );
			FrameSize = new Size( _pCodecContext->width, _pCodecContext->height );
			PixelFormat = _pCodecContext->pix_fmt;
			Context = *_pFormatContext;

			_pPacket = ffmpeg.av_packet_alloc();
			_pFrame = ffmpeg.av_frame_alloc();
		}

		public string CodecName { get; }
		public Size FrameSize { get; }
		public AVPixelFormat PixelFormat { get; }
		public AVFormatContext Context { get; }
		public AVStream Stream { get; }

		public double Duration => (double)Context.duration / (double)ffmpeg.AV_TIME_BASE;
		public long CurrentFrame => _currentFrame;
		public double FrameRate => (double)Stream.r_frame_rate.num / (double)Stream.r_frame_rate.den;
		public long TotalFrames => (long)(Duration * FrameRate);

		public void Dispose() {
			ffmpeg.av_frame_unref( _pFrame );
			ffmpeg.av_free( _pFrame );

			ffmpeg.av_packet_unref( _pPacket );
			ffmpeg.av_free( _pPacket );

			ffmpeg.avcodec_close( _pCodecContext );
			var pFormatContext = _pFormatContext;
			ffmpeg.avformat_close_input( &pFormatContext );
		}

		public bool TryDecodeNextFrame(out AVFrame frame) {
			ffmpeg.av_frame_unref( _pFrame );
			ffmpeg.av_frame_unref( _receivedFrame );
			int error;
			do {
				try {
					do {
						error = ffmpeg.av_read_frame( _pFormatContext, _pPacket );
						if( error == ffmpeg.AVERROR_EOF ) {
							frame = *_pFrame;
							return false;
						}

						error.ThrowExceptionIfError();
					} while( _pPacket->stream_index != _streamIndex );

					ffmpeg.avcodec_send_packet( _pCodecContext, _pPacket ).ThrowExceptionIfError();
				}
				finally {
					ffmpeg.av_packet_unref( _pPacket );
				}

				error = ffmpeg.avcodec_receive_frame( _pCodecContext, _pFrame );
			} while( error == ffmpeg.AVERROR( ffmpeg.EAGAIN ) );
			error.ThrowExceptionIfError();
			if( _pCodecContext->hw_device_ctx != null ) {
				ffmpeg.av_hwframe_transfer_data( _receivedFrame, _pFrame, 0 ).ThrowExceptionIfError();
				frame = *_receivedFrame;
			}
			else {
				frame = *_pFrame;
			}
			_currentFrame++;
			return true;
		}

		public bool TrySeekFrame(long target_frame) {
			if( target_frame >= TotalFrames ) return false;
			int flags = target_frame < _currentFrame ? ffmpeg.AVSEEK_FLAG_BACKWARD : 0;
			double time = ((double)target_frame / FrameRate) * ffmpeg.AV_TIME_BASE;
			long t = ffmpeg.av_rescale_q( (long)time, ffmpeg.av_get_time_base_q(), _pFormatContext->streams[_streamIndex]->time_base );
			int error = ffmpeg.av_seek_frame( _pFormatContext, _streamIndex, t, flags );
			error.ThrowExceptionIfError();
			_currentFrame = target_frame;
			return true;
		}

		public IReadOnlyDictionary<string, string> GetContextInfo() {
			AVDictionaryEntry* tag = null;
			var result = new Dictionary<string, string>();
			while( (tag = ffmpeg.av_dict_get( _pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX )) != null ) {
				var key = Marshal.PtrToStringAnsi( (IntPtr)tag->key );
				var value = Marshal.PtrToStringAnsi( (IntPtr)tag->value );
				result.Add( key, value );
			}

			return result;
		}
	}
}
