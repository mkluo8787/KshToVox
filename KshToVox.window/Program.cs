using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Threading;

using SongList;

namespace KshToVox.window
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form());	
		}

		static SongList.SongList songList = new SongList.SongList();
		static string statusText = "";
        static string statusRText = "";
        static bool changes = false;
        static bool loading = false;
        static int selectedSongId = 0;

        public static void LoadSongList(Action callbackUpdate)
        {
            if (loading) return;

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

			folderBrowserDialog1.Description = "Select the KFC path.";
			folderBrowserDialog1.ShowNewFolderButton = false;

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				LoadSongList(folderBrowserDialog1.SelectedPath, callbackUpdate);
			}		
		}

		public static void LoadSongList(string path, Action callbackUpdate)
		{
            if (loading) return;
            if (changes)
                if (!Ask("Discard unsaved changes?")) return;

            SetStatusR("Loading...");

            Thread thread = new Thread(() => LoadSongList_Thread(path, callbackUpdate));
            thread.Start();
        }

        private static void LoadSongList_Thread(string path, Action callbackUpdate)
        {
            loading = true;
            
            songList.Load(path);

            SetStatusR("");
            SetStatus("KFC song list loaded.");

            changes = false;
            loading = false;

            callbackUpdate();
        }

        public static void SaveSongList()
		{
            if (loading) return;
            changes = false;
            songList.Save();
            SetStatus("KFC song list saved.");
        }

		public static void ImportSong(Action callbackUpdate)
		{
            if (loading) return;
 
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

			folderBrowserDialog1.Description = "Select the KFC path.";
			folderBrowserDialog1.ShowNewFolderButton = false;

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
                ImportSong(folderBrowserDialog1.SelectedPath, callbackUpdate);
			}
		}

		public static void ImportSong(string path, Action callbackUpdate)
		{
            if (loading) return;

            if (!songList.Loaded())
			{
				SetStatus("KFC song list has not loaded!");
				return;
			}

            SetStatusR("Loading...");

            Thread thread = new Thread(() => ImportSong_Thread(path, callbackUpdate));
            thread.Start();
        }

        private static void ImportSong_Thread(string path, Action callbackUpdate)
        {
            loading = true;

            int newSelectedSongId = songList.AddKshSong(path, selectedSongId);
            selectedSongId = newSelectedSongId;            

            SetStatusR("");
            SetStatus("New K-Shoot song loaded.");
            changes = true;
            loading = false;
            
            callbackUpdate();
        }

        public static void DeleteSong()
		{
            if (loading) return;

            int id = selectedSongId;
			if (id == -1)
			{
				SetStatus("No song selected!");
				return;
			}
            changes = songList.DeleteId(id);
		}

        public static bool CheckUnsavedB4Closing()
        {
            if (changes) return !Ask("Discard unsaved changes?");
            else return false;
        }

        public static List<KeyValuePair<int, Song>> GetSongList() {	return songList.List();	}
        public static KeyValuePair<int, Song> GetSongListId(int id) { return new KeyValuePair<int, Song>(id, songList.Song(id)); }

        public static int GetSelectedIndex() { return selectedSongId; }

		public static Dictionary<string, string> GetLabels()
		{
			int id = selectedSongId;

			Dictionary<string, string> labels = new Dictionary<string, string>();

			string[] tags = {"title_name", "artist_name"};
			foreach (string tag in tags)
				if (id == -1) labels[tag] = "";
				else labels[tag] = songList.Song(id).Data(tag);

			return labels;
		}

        public static string GetTitle()
        {
            if (changes) return "KshToVox (Unsaved changes)";
            else return "KshToVox";
        }

        public static void UpdateSeletedSongId(int id) { selectedSongId = id; }

        public static string GetStatus() { return statusText; }
        public static string GetStatusR() { return statusRText; }

        public static bool Loaded() { return songList.Loaded(); }

        private static void SetStatus(string text) { statusText = text; }
        private static void SetStatusR(string text) { statusRText = text; }

        private static bool Ask(string text)
        {
            return MessageBox.Show(text, "KshToVox",
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }
	}
}
