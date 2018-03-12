using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Threading;

using Utility;
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
            Util.SetKfcPath(@"E:\CHIKAN\ks_to_SDVX\Minimal SDVX HH for FX testing");

            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            Init();

            // Starts the form
            Application.Run(new Form());
        }

        static void Init()
        {
            // Check if kfc dll exists.
            if (!File.Exists(Util.kfcPath + "soundvoltex.dll"))
            {
                //Console.WriteLine("soundvoltex.dll not found! Please choose a valid KFC path.");
                //Console.ReadKey();
                return;
            }

            // Check if folders exist.
            if (!Directory.Exists(Util.kfcPath + "KshSongs\\"))
                Directory.CreateDirectory(Util.kfcPath + "KshSongs\\");

            if (!Directory.Exists(Util.kfcPath + "data\\others\\vox\\"))
                Directory.CreateDirectory(Util.kfcPath + "data\\others\\vox\\");

            Util.ClearCache();

            // DB backup (for later restore)
            Util.DbBackup();

            songList = new SongList.SongList();

            try
            {
                songList.LoadFromKshSongs(false, false);
            }
            catch (Exception e)
            {
                Util.DbRestore();
                return;
            }
        }

		static SongList.SongList songList;

        static List<int> deletePending = new List<int>();
        static List<string> newPending = new List<string>();

        static string statusText = "Welcome to KshToVox!";
        static string statusRText = "";
        static bool changes = false;
        static bool loading = false;
        static int selectedSongId = 0;

        /*
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
        */

        public static void SaveSongList()
		{
            if (loading) return;
            changes = false;
            songList.Save();
            SetStatus("KFC song list saved.");
        }
        
		public static void ImportSong()
		{
            if (loading) return;
 
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

			folderBrowserDialog1.Description = "Select the KSH song path.";
			folderBrowserDialog1.ShowNewFolderButton = false;

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
                string[] paths = new string[1];
                paths[0] = folderBrowserDialog1.SelectedPath;
                ImportSongs(paths);
			}
		}

        public static void ImportSongs(string[] paths)
        {
            if (loading) return;

            if (!songList.Loaded())
            {
                SetStatus("KFC song list has not loaded!");
                return;
            }

            foreach (string path in paths)
            {
                if (!newPending.Contains(path))
                    newPending.Add(path);
            }
        }
        
        
        static void ImportSongs_Thread(string[] paths, Action callbackUpdate)
        {
            loading = true;

            SetStatusR("Loading...");

            List<string> newPaths = new List<string>();

            foreach (string path in paths)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                string newPath = Util.kfcPath + "KshSongs\\" + di.Name;

                if (Directory.Exists(newPath))
                    continue;

                Util.CopyDirectory(path, newPath);
                newPaths.Add(newPath);
            }

            try
            {
                songList.LoadKshSongs(newPaths.ToArray());
            }
            catch (Exception e)
            {
                Util.DbRestore();
                return;
            }

            SetStatusR("");
            SetStatus("New K-Shoot songs loaded.");
            changes = true;
            loading = false;

            callbackUpdate();
        }
        

        public static void ToggleDeleteSong()
		{
            if (loading) return;

            int id = selectedSongId;
			if (songList.Song(id).IsDummy())
			{
				SetStatus("No song selected!");
				return;
			}

            if (deletePending.Contains(id))
                deletePending.Remove(id);
            else
                deletePending.Add(id);
		}

        public static void Update(Action callbackUpdate)
        {
            if (deletePending.Count > 0)
            {
                foreach (int id in deletePending)
                    songList.DeleteId(id);
                changes = true;
            }

            if (newPending.Count > 0)
            {
                string[] newPendingS = newPending.ToArray();
                Task task = new Task(() => ImportSongs_Thread(newPendingS, callbackUpdate));
                task.Start();
            }

            deletePending.Clear();
            newPending.Clear();

            callbackUpdate();
        }

        public static bool CheckUnsavedB4Closing()
        {
            if (Pending() || changes) return !Ask("Discard unsaved changes?");
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
            if (Pending() || changes) return "KshToVox (Unsaved changes)";
            else return "KshToVox";
        }

        public static void UpdateSeletedSongId(int id) { selectedSongId = id; }

        public static string GetStatus() { return statusText; }
        public static string GetStatusR() { return statusRText; }

        public static bool Loaded() { return songList.Loaded(); }

        public static bool Pending()
        {
            return !((deletePending.Count == 0) && (newPending.Count == 0));
        }

        static void SetStatus(string text) { statusText = text; }
        static void SetStatusR(string text) { statusRText = text; }

        static bool Ask(string text)
        {
            return MessageBox.Show(text, "KshToVox",
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        static void Warn(string text)
        {
            MessageBox.Show(text, "KshToVox",
                MessageBoxButtons.OK);
        }
    }
}
