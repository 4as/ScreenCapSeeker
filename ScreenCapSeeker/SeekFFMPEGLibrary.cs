using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScreenCapSeeker {
	public static class SeekFFMPEGLibrary {

		static internal AVPixelFormat pixelFormat;
		static internal AVHWDeviceType deviceType;
		static internal bool isInit = false;

		public static bool IsInitialized => isInit;

		public static void Setup() {
			if( RegisterFFmpegBinaries() ) {
				ConfigureHWDecoder( out deviceType );
				pixelFormat = GetHWPixelFormat( deviceType );
				isInit = true;
			}
		}

		public static unsafe void SetupLogging(Action<string> callback) {
			ffmpeg.av_log_set_level( ffmpeg.AV_LOG_VERBOSE );

			// do not convert to local function
			av_log_set_callback_callback logCallback = (p0, level, format, vl) => {
				if( level > ffmpeg.av_log_get_level() ) return;

				var lineSize = 1024;
				var lineBuffer = stackalloc byte[lineSize];
				var printPrefix = 1;
				ffmpeg.av_log_format_line( p0, level, format, vl, lineBuffer, lineSize, &printPrefix );
				var line = Marshal.PtrToStringAnsi( (IntPtr)lineBuffer );
				callback( line );
			};

			ffmpeg.av_log_set_callback( logCallback );
		}

		private static void ConfigureHWDecoder(out AVHWDeviceType HWtype) {
			HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
			var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();
			var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
			var number = 0;
			while( (type = ffmpeg.av_hwdevice_iterate_types( type )) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE ) {
				++number;
				availableHWDecoders.Add( number, type );
			}
			if( availableHWDecoders.Count == 0 ) {
				HWtype = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
				return;
			}
			int decoderNumber = availableHWDecoders.SingleOrDefault( t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2 ).Key;
			if( decoderNumber == 0 )
				decoderNumber = availableHWDecoders.First().Key;
			availableHWDecoders.TryGetValue( decoderNumber, out HWtype );
		}

		private static AVPixelFormat GetHWPixelFormat(AVHWDeviceType hWDevice) {
			switch( hWDevice ) {
				case AVHWDeviceType.AV_HWDEVICE_TYPE_NONE:
					return AVPixelFormat.AV_PIX_FMT_NONE;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU:
					return AVPixelFormat.AV_PIX_FMT_VDPAU;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA:
					return AVPixelFormat.AV_PIX_FMT_CUDA;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI:
					return AVPixelFormat.AV_PIX_FMT_VAAPI;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2:
					return AVPixelFormat.AV_PIX_FMT_NV12;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_QSV:
					return AVPixelFormat.AV_PIX_FMT_QSV;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX:
					return AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA:
					return AVPixelFormat.AV_PIX_FMT_NV12;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_DRM:
					return AVPixelFormat.AV_PIX_FMT_DRM_PRIME;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL:
					return AVPixelFormat.AV_PIX_FMT_OPENCL;
				case AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC:
					return AVPixelFormat.AV_PIX_FMT_MEDIACODEC;
				default:
					return AVPixelFormat.AV_PIX_FMT_NONE;
			}
		}

		internal static bool RegisterFFmpegBinaries() {
			if( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) ) {
				var current = Environment.CurrentDirectory;
				var probe = Path.Combine( "FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86" );

				while( current != null ) {
					var ffmpegBinaryPath = Path.Combine( current, probe );

					if( Directory.Exists( ffmpegBinaryPath ) ) {
						ffmpeg.RootPath = ffmpegBinaryPath;
						return true;
					}

					current = Directory.GetParent( current )?.FullName;
				}

				return false;
			}
			else if( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) ) {
				ffmpeg.RootPath = "/lib/x86_64-linux-gnu/";
				return true;
			}
			else {
				return false;
			}
		}
	}
}
