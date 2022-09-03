namespace ScreenCapSeeker {
	public readonly struct SeekInfo {
		public readonly double duration;
		public readonly long numFrames;

		public SeekInfo(double duration, long numFrames) {
			this.duration = duration;
			this.numFrames = numFrames;
		}

		public override string ToString() {
			return "" + typeof( SeekInfo ).Name + "[duration: " + duration + ", frames: " + numFrames + "]";
		}
	}
}
