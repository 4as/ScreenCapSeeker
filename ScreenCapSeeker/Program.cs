using System;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	static class Program {

		[STAThread]
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new App() );
		}
	}
}
