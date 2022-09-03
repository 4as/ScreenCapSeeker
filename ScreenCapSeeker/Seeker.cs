using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ScreenCapSeeker {
	public class Seeker {
		private readonly BackgroundWorker workerBackground = new BackgroundWorker();

		private readonly Dictionary<byte, ulong> dictBounds = new Dictionary<byte, ulong>();
		private readonly Dictionary<byte, ulong> dictResults = new Dictionary<byte, ulong>();

		private readonly SeekFFMPEG mediaMain = new SeekFFMPEG();
		private readonly SeekProgress seekProgress = new SeekProgress();

		private readonly SeekDataFrame frameClip = new SeekDataFrame();
		private readonly SeekDataFrame frameMain = new SeekDataFrame();

		private SeekInfo seekInfo;
		private byte nIterations = 1;

		private long nFrame;

		public Seeker() {
			SeekFFMPEGLibrary.Setup();
		}

		public bool IsValid => SeekFFMPEGLibrary.IsInitialized;
		public SeekInfo Info => seekInfo;
		public SeekProgress Progress => seekProgress;
		public long CurrentFrame => nFrame;
		public long TotalFrames => mediaMain.TotalFrames;

		public void Open(string source, SeekDataBitmap match) {
			frameClip.Bitmap = match;
			mediaMain.Open( source );
			seekInfo = new SeekInfo( mediaMain.Duration, mediaMain.TotalFrames );

			frameMain.Bitmap = mediaMain.ReadFrame();
			nIterations = frameMain.MaxFactor;
			for( byte i = 0; i < nIterations; i++ ) {
				dictBounds[i] = ulong.MaxValue;
			}
			nFrame = 1;
		}

		public void Start() {
			workerBackground.DoWork += Process;
			workerBackground.RunWorkerCompleted += OnCompleted;
			workerBackground.ProgressChanged += OnReport;
			workerBackground.WorkerSupportsCancellation = true;
			workerBackground.WorkerReportsProgress = true;
			workerBackground.RunWorkerAsync();
		}

		public void Stop() {
			if( workerBackground.IsBusy ) {
				workerBackground.CancelAsync();
			}
		}

		private void Process(object sender, DoWorkEventArgs e) {
			BackgroundWorker worker = (BackgroundWorker)sender;

			while( mediaMain.CurrentFrame <= mediaMain.TotalFrames ) {
				if( worker.CancellationPending ) {
					e.Cancel = true;
					return;
				}

				var frame = mediaMain.ReadFrame();
				nFrame = mediaMain.CurrentFrame;

				if( frame != null ) {
					frameMain.Bitmap = frame;

					dictResults.Clear();

					byte factor = 2;
					while( factor < nIterations ) {
						ulong distance = dictBounds[factor];
						ulong diff = frameMain.Compare( factor, frameClip );
						if( diff > distance ) {
							break;
						}
						else {
							dictResults[factor] = diff;
							factor++;
						}
					}

					if( factor == nIterations ) {
						foreach( var entry in dictResults ) {
							dictBounds[entry.Key] = entry.Value;
						}
						seekProgress.SetSnapshot( mediaMain.CurrentFrame, frameMain, frameClip, nIterations );
					}

					if( nFrame % 100 == 1 ) {
						double percent = (double)mediaMain.CurrentFrame / (double)mediaMain.TotalFrames;
						int step = (int)(percent * 100);
						seekProgress.SetPreview( frameMain );
						worker.ReportProgress( step, seekProgress );
					}
				}
				else {
					seekProgress.SetPreview( frameMain );
					worker.ReportProgress( 100, seekProgress );
					break;
				}
			}
		}

		public event Action<int, SeekProgress> EventProgress;
		public event Action<RunWorkerCompletedEventArgs> EventCompleted;

		private void OnReport(object sender, ProgressChangedEventArgs e) {
			SeekProgress progress = (SeekProgress)e.UserState;
			EventProgress?.Invoke( e.ProgressPercentage, progress );
		}

		private void OnCompleted(object sender, RunWorkerCompletedEventArgs e) {
			mediaMain.Dispose();

			workerBackground.DoWork -= Process;
			workerBackground.RunWorkerCompleted -= OnCompleted;
			workerBackground.ProgressChanged -= OnReport;
			EventCompleted?.Invoke( e );
		}

	}
}
