using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

using SongList;

namespace KshToVox.window
{
	public partial class Form : System.Windows.Forms.Form
	{
		public Form()
		{
			InitializeComponent();
		}

		private void UpdateView()
		{
			SongListTextBox.DataSource = null;
			SongListTextBox.DataSource = Program.GetSongList();

			label_title.Text = Program.GetSelectedTitle();

			toolStripStatusLabel1.Text = Program.GetStatus();

			button2.Enabled = Program.Loaded();
			button3.Enabled = Program.Loaded();
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.LoadSongList();
			UpdateView();
		}

		public int GetSongListTextBoxSongId()
		{
			if (SongListTextBox.SelectedItem == null) return -1;
			KeyValuePair<int, Song> songId = (KeyValuePair<int, Song>)SongListTextBox.SelectedItem;

			return songId.Key;
		}

		private void splitContainer1_Panel1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void splitContainer1_Panel1_DragDrop(object sender, DragEventArgs e)
		{
			string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

			Program.LoadSongList(folders[0]);
			UpdateView();
		}

		private void splitContainer2_Panel2_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void splitContainer2_Panel2_DragDrop(object sender, DragEventArgs e)
		{
			string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

			Program.ImportSong(folders[0]);
			UpdateView();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Program.DeleteSong();
			UpdateView();
		}
	}
}
