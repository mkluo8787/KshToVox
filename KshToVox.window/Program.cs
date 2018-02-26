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
		static Form form;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			form = new Form();
			Application.Run(form);	
		}

		static SongList.SongList	songList = new SongList.SongList();
		static string statusText = "";

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
			Song newSong = new Song(path);
			songList.AddSong(newSong);
		}

		public static void DeleteSong()
		{
			int id = form.GetSongListTextBoxSongId();
			if (id == -1)
			{
				SetStatus("No song selected!");
				return;
			}
			songList.DeleteId(id);
		}

		public static List<KeyValuePair<int, Song>> GetSongList() {	return songList.List();	}

		public static string GetSelectedTitle()
		{
			int id = form.GetSongListTextBoxSongId();
			Console.WriteLine(id);
			if (id == -1) return "";
			return songList.Song(id).Title();
		}

		public static string GetStatus() { return statusText; }

		public static bool Loaded() { return songList.Loaded(); }

		public static void SetStatus(string text) { statusText = text; }
	}
}
