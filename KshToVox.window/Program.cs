using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

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

		static SongList.SongList	songList = new SongList.SongList();
		static string statusText = "";
		static int selectedSongId = -1;

		public static void LoadSongList() {

			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

			folderBrowserDialog1.Description = "Select the KFC path.";
			folderBrowserDialog1.ShowNewFolderButton = false;

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				LoadSongList(folderBrowserDialog1.SelectedPath);
			}		
		}

		public static void LoadSongList(string path)
		{
			SetStatus("KFC song list loaded.");
			songList.Load(path);
		}

		public static void SaveSongList()
		{
			songList.Save();
		}

		public static void ImportSong()
		{
			if (!songList.Loaded())
			{
				SetStatus("KFC song list has not loaded!");
				return;
			}
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

			folderBrowserDialog1.Description = "Select the KFC path.";
			folderBrowserDialog1.ShowNewFolderButton = false;

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				ImportSong(folderBrowserDialog1.SelectedPath);
			}
		}

		public static void ImportSong(string path)
		{
			if (!songList.Loaded())
			{
				SetStatus("KFC song list has not loaded!");
				return;
			}
			SetStatus("New K-Shoot song loaded with id = " + songList.AddKshSong(path));
		}

		public static void DeleteSong()
		{
			int id = selectedSongId;
			if (id == -1)
			{
				SetStatus("No song selected!");
				return;
			}
			songList.DeleteId(id);
		}

		public static List<KeyValuePair<int, Song>> GetSongList() {	return songList.List();	}

		public static int GetSelectedIndex() { return selectedSongId; }

		public static Dictionary<string, string> GetLabels()
		{
			int id = selectedSongId;

			Dictionary<string, string> labels = new Dictionary<string, string>();

			string[] tags = {"title", "artist"};
			foreach (string tag in tags)
				if (id == -1) labels[tag] = "";
				else labels[tag] = songList.Song(id).Data(tag);

			return labels;
		}

		internal static void UpdateSeletedSongId(int id) {	selectedSongId = id; }

		public static string GetStatus() { return statusText; }

		public static bool Loaded() { return songList.Loaded(); }

		public static void SetStatus(string text) { statusText = text; }
	}
}
