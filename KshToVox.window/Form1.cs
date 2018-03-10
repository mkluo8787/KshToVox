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
            foreach (var item in Program.GetSongList())
                SongListTextBox.Items.Add(item);
        }

		private void UpdateView()
		{
            this.Invoke((MethodInvoker)delegate
            {
                for (int i = 0; i < SongListTextBox.Items.Count; ++i)
                {
                    SongListTextBox.Items[i] = Program.GetSongListId(i + 1);
                }
                
                SongListTextBox.SelectedIndex = Program.GetSelectedIndex() - 1;

                UpdateViewStatic();
            });
		}

		private void UpdateViewStatic()
		{
            Text = Program.GetTitle();

			Dictionary<string, string> labels = Program.GetLabels();
			label_title.Text = labels["title_name"];
			label_artist.Text = labels["artist_name"];

			toolStripStatusLabel1.Text = Program.GetStatus();
            toolStripStatusLabel3.Text = Program.GetStatusR();

            button2.Enabled = Program.Loaded();
            //button3.Enabled = Program.Loaded();
            button3.Enabled = false;

        }

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.LoadSongList(UpdateView);
			UpdateView();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.SaveSongList();
			UpdateView();
		}

		private void splitContainer1_Panel1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void splitContainer1_Panel1_DragDrop(object sender, DragEventArgs e)
		{
			string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

			Program.LoadSongList(folders[0], UpdateView);
			UpdateView();
		}

		private void splitContainer2_Panel2_DragEnter(object sender, DragEventArgs e)
		{
			//if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void splitContainer2_Panel2_DragDrop(object sender, DragEventArgs e)
		{
            /*
			string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
			if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

			Program.ImportSong(folders[0], UpdateView);
			UpdateView();
            */
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Program.DeleteSong();
			UpdateView();
		}

		private void SongListTextBox_SelectedValueChanged(object sender, EventArgs e)
		{
            /*	
            if (SongListTextBox.SelectedItem == null)
                Program.UpdateSeletedSongId(-1);
            else
            {
                KeyValuePair<int, Song> songId = (KeyValuePair<int, Song>)SongListTextBox.SelectedItem;
                Program.UpdateSeletedSongId(songId.Key);

            }

            UpdateViewStatic();
            */
        }

        private void SongListTextBox_MouseClick(object sender, MouseEventArgs e)
        {	
            if (SongListTextBox.SelectedItem != null)
            { 
                KeyValuePair<int, Song> songId = (KeyValuePair<int, Song>)SongListTextBox.SelectedItem;
                Program.UpdateSeletedSongId(songId.Key);

            }
            UpdateViewStatic();
        }

        private void SongListTextBox_MouseLeave(object sender, EventArgs e)
        {
            if (SongListTextBox.SelectedItem != null)
            {
                KeyValuePair<int, Song> songId = (KeyValuePair<int, Song>)SongListTextBox.SelectedItem;
                Program.UpdateSeletedSongId(songId.Key);

            }
            UpdateViewStatic();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = Program.CheckUnsavedB4Closing();
        }
    }
}
