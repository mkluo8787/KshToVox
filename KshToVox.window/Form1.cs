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
			Console.WriteLine("View Update!");
			SongListTextBox.DataSource = null;
			SongListTextBox.DataSource = Program.GetSongListString();
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Stream myStream = null;
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
			openFileDialog1.FilterIndex = 2;
			openFileDialog1.RestoreDirectory = true;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				if ((myStream = openFileDialog1.OpenFile()) != null)
				{
					Program.LoadSongList(myStream);	
				}
			}
			UpdateView();
		}
	}
}
