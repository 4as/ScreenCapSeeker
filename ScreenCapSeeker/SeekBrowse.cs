using System;
using System.IO;
using System.Windows.Forms;

namespace ScreenCapSeeker {
	public class SeekBrowse : Panel {
		private const string TEXT_PATH_MAIN = "Choose a source video";
		private const string TEXT_PATH_MATCH = "Choose a clip/image to match";
		private static readonly string[] EXTENSIONS_IMAGE = new string[] { ".bmp", ".jpg", ".jpeg", ".gif" };
		private static readonly string[] EXTENSIONS_MOVIE = new string[] { ".avi", ".mp4", ".mkv", ".mpeg", ".wmv", ".mov" };

		private const int SIZE_DEFAULT_HEIGHT = 25;
		private const int MARGIN = 4;

		private readonly TextBox textPathMain = new TextBox();
		private readonly Button butBrowseMain = new Button();
		private readonly TextBox textPathMatch = new TextBox();
		private readonly Button butBrowseMatch = new Button();
		private readonly Button butStart = new Button();

		private string sSelectedMain;
		private string sSelectedMatch;
		private bool bMovie;

		public SeekBrowse() {
			MinimumSize = new System.Drawing.Size( 300, 80 );

			textPathMain.Multiline = false;
			textPathMain.Enabled = false;
			textPathMain.ReadOnly = true;
			textPathMain.Text = TEXT_PATH_MAIN;
			textPathMain.BorderStyle = BorderStyle.FixedSingle;
			textPathMain.Width = 200;
			textPathMain.Height = SIZE_DEFAULT_HEIGHT;
			Controls.Add( textPathMain );

			butBrowseMain.Text = "...";
			butBrowseMain.Width = 60;
			butBrowseMain.Height = SIZE_DEFAULT_HEIGHT;
			butBrowseMain.Click += OnBrowseMain;
			Controls.Add( butBrowseMain );

			textPathMatch.Multiline = false;
			textPathMatch.Enabled = false;
			textPathMatch.ReadOnly = true;
			textPathMatch.Text = TEXT_PATH_MATCH;
			textPathMatch.BorderStyle = BorderStyle.FixedSingle;
			textPathMatch.Width = 200;
			textPathMatch.Height = SIZE_DEFAULT_HEIGHT;
			Controls.Add( textPathMatch );

			butBrowseMatch.Text = "...";
			butBrowseMatch.Width = 60;
			butBrowseMatch.Height = SIZE_DEFAULT_HEIGHT;
			butBrowseMatch.Click += OnBrowseMatch;
			Controls.Add( butBrowseMatch );

			butStart.Text = "START";
			butStart.Click += OnStart;
			butStart.Width = 100;
			butStart.Height = 25;
			butStart.Enabled = false;
			Controls.Add( butStart );

			Resize += OnResized;
			OnResized( null, null );
		}

		public string PathMain => sSelectedMain;
		public string PathMatch => sSelectedMatch;
		public bool IsMatchingMovie => bMovie;

		private string Browse(string file_filter) {
			using( OpenFileDialog openFileDialog = new OpenFileDialog() ) {
				//openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
				openFileDialog.Filter = "Supported Files (" + file_filter + ")|" + file_filter;
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = false;
				//openFileDialog.RestoreDirectory = true;

				if( openFileDialog.ShowDialog() == DialogResult.OK ) {
					return openFileDialog.FileName;
				}
			}
			return null;
		}

		private void UpdateStart() {
			butStart.Enabled = !(string.IsNullOrEmpty( sSelectedMain ) || string.IsNullOrEmpty( sSelectedMatch ));
		}

		private string Compress(string text, int max_length = 42) {
			if( text.Length <= max_length || text.Length < 12 ) return text;
			int size = (max_length / 2) - 5;
			return text.Substring( 0, size ) + "[...]" + text.Substring( text.Length - size );
		}

		public event Action EventStart;

		private void OnBrowseMain(object sender, EventArgs e) {
			if( e is MouseEventArgs mouse_ev && mouse_ev.Button != MouseButtons.Left ) return;
			sSelectedMain = Browse( "*.AVI;*.MP4;*.MKV;*.MPEG;*.WMV;*.MOV" );
			if( sSelectedMain == sSelectedMatch ) {
				textPathMatch.Text = TEXT_PATH_MATCH;
				sSelectedMatch = null;
			}
			textPathMain.Text = Compress( sSelectedMain ?? TEXT_PATH_MAIN );
			UpdateStart();
		}

		private void OnBrowseMatch(object sender, EventArgs e) {
			sSelectedMatch = Browse( "*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG;*.AVI;*.MP4;*.MKV;*.MPEG;*.WMV;*.MOV" );
			if( sSelectedMatch == sSelectedMain ) {
				textPathMain.Text = TEXT_PATH_MAIN;
				sSelectedMain = null;
			}
			if( string.IsNullOrEmpty( sSelectedMatch ) ) {
				textPathMatch.Text = TEXT_PATH_MATCH;
			}
			else {
				textPathMatch.Text = Compress( sSelectedMatch );
				bMovie = Array.IndexOf( EXTENSIONS_MOVIE, Path.GetExtension( sSelectedMatch ) ) != -1;
			}
			UpdateStart();
		}

		private void OnStart(object sender, EventArgs e) {
			EventStart?.Invoke();
		}

		private void OnResized(object sender, EventArgs e) {
			int center = Width / 2;
			int size = textPathMain.Width + butBrowseMain.Width;
			textPathMain.Location = new System.Drawing.Point( center - (size / 2), 2 );
			butBrowseMain.Location = new System.Drawing.Point( textPathMain.Location.X + textPathMain.Width + MARGIN, textPathMain.Location.Y - 2 );
			textPathMatch.Location = new System.Drawing.Point( center - (size / 2), textPathMain.Location.Y + textPathMain.Height + MARGIN );
			butBrowseMatch.Location = new System.Drawing.Point( textPathMatch.Location.X + textPathMatch.Width + MARGIN, textPathMatch.Location.Y - 2 );
			butStart.Location = new System.Drawing.Point( center - (butStart.Width / 2), textPathMatch.Location.Y + textPathMatch.Height + MARGIN );
		}
	}
}
