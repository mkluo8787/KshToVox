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
            dataGridView1.DataSource = Program.GetSongsInfo();

            dataGridView1.Columns[0].Width = 30;
            dataGridView1.Columns[1].Width = 45;
            dataGridView1.Columns[2].Width = 155;
        }

        private void PreUpdateView()
        {
            if (dataGridView1.SelectedRows.Count == 1)
                Program.RecordSelectedIndex(dataGridView1.SelectedRows[0].Index);
        }

        private void UpdateView()
		{
            this.Invoke((MethodInvoker)delegate
            {
                //int newSelId = Program.GetSelectedIndex();
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = Program.GetSongsInfo();

                dataGridView1.Columns[0].Width = 30;
                dataGridView1.Columns[1].Width = 45;
                dataGridView1.Columns[2].Width = 155;

                int id = Program.GetSelectedIndex();

                dataGridView1.ClearSelection();

                if (id >= 0)
                    dataGridView1.Rows[id].Selected = true;

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
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.SaveSongList();
            UpdateViewStatic();
		}

        /*
		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.LoadSongList(UpdateView);
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
*/
        private void splitContainer2_Panel2_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void splitContainer2_Panel2_DragDrop(object sender, DragEventArgs e)
		{
            
			string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
			//if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

            PreUpdateView();
            Program.ToggleImportSongs(folders);
			UpdateViewStatic();
		}

		private void button2_Click(object sender, EventArgs e)
		{
            PreUpdateView();
            Program.ToggleDeleteSong();
			UpdateView();
		}

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = Program.CheckUnsavedB4Closing();
        }

        private void importkshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.ImportSong();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PreUpdateView();
            Program.Update(UpdateView);
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            PreUpdateView();
            UpdateViewStatic();
        }

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            PreUpdateView();
            UpdateViewStatic();
        }
    }
}
