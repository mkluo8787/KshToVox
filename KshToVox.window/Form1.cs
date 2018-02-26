using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KshToVox.window
{
	public partial class Form : System.Windows.Forms.Form
	{
		public Form()
		{
			InitializeComponent();
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog Dialog1 = new FolderBrowserDialog();

			if (Dialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				
			}
		}
	}
}
