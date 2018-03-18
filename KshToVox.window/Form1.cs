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

using Utility;
using SongList;

using SharpUpdate;
using System.Reflection;

namespace KshToVox.window
{
	public partial class Form1 : Form, ISharpUpdatable
    {
        private SharpUpdater updater;

        public Form1()
		{
            Controller.Init();            

            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            Controller.RecordSelectedIndex(-1);
            UpdateView();

            updater = new SharpUpdater(this);
        }

        #region VIEW

        private void PreUpdateView()
        {
            if (dataGridView1.SelectedRows.Count == 1)
                //Program.RecordSelectedIndex(dataGridView1.SelectedRows[0].Index);
                Controller.RecordSelectedIndex((int)dataGridView1.SelectedRows[0].Cells[0].Value);
        }

        private void UpdateViewInvoke()
        {
            this.Invoke((MethodInvoker)delegate
            {
                UpdateView();
            });
        }

        private void UpdateView()
        { 
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = Controller.GetSongsInfo();

            dataGridView1.Columns[0].Width = 30;
            dataGridView1.Columns[1].Width = 45;
            dataGridView1.Columns[2].Width = 150;

            dataGridView2.DataSource = null;
            dataGridView2.DataSource = Controller.GetPendingSongsInfo();

            dataGridView2.Columns[0].Width = 135;

            UpdateViewStatic();
        }

		private void UpdateViewStatic()
		{
            int id = Controller.GetSelectedIndex();
            dataGridView1.ClearSelection();
            if (id >= 0)
            {
                pictureBox1.Image = Controller.GetImage();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    if ((int)row.Cells[0].Value == id)
                        row.Selected = true;
            }
            else
                dataGridView1.ClearSelection();

            Text = Controller.GetTitle();

            toolStripStatusLabel1.Text = Controller.GetStatus();
            toolStripStatusLabel3.Text = Controller.GetStatusR();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
            Controller.SaveSongList();
            UpdateViewStatic();
		}

		private void button2_Click(object sender, EventArgs e)
		{
            PreUpdateView();
            Controller.ToggleDeleteSong();
			UpdateView();
		}

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = Controller.CheckUnsavedB4Closing();
        }

        private void importkshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Controller.ImportSong();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PreUpdateView();
            Controller.Update(UpdateViewInvoke);
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            PreUpdateView();
            UpdateViewStatic();
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            UpdateViewStatic();
        }

        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            PreUpdateView();
            UpdateViewStatic();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PreUpdateView();
            Controller.ClearPending();
            UpdateView();
        }

        private void dataGridView2_DragDrop(object sender, DragEventArgs e)
        {
            string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
            //if (!((folders.Length == 1) && (Directory.Exists(folders[0])))) return;

            PreUpdateView();
            Controller.ToggleImportSongs(folders);
            UpdateView();
        }

        private void dataGridView2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updater.DoUpdate();
        }

        #endregion

        #region Controller

        static class Controller
        {
            public static void Init()
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
                    songList.LoadFromDB();
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
                    ToggleImportSongs(paths);
                }
            }

            public static void ToggleImportSongs(string[] paths)
            {
                if (loading) return;

                if (!songList.Loaded())
                {
                    SetStatus("KFC song list has not loaded!");
                    return;
                }

                foreach (string path in paths)
                {
                    if (!Directory.Exists(path))
                        continue;
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
                        Directory.Delete(newPath, true);

                    Util.CopyDirectory(path, newPath);
                    newPaths.Add(newPath);
                }

                callbackUpdate();

                songList.LoadKshSongs(newPaths.ToArray());


                SetStatusR("");
                SetStatus("New K-Shoot songs loaded.");
                changes = true;
                loading = false;

                callbackUpdate();
            }


            public static void ToggleDeleteSong()
            {
                if (loading) return;

                //int id = SelectingId();
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
                if (loading) return;

                if (deletePending.Contains(selectedSongId))
                    selectedSongId = -1;

                if (deletePending.Count > 0)
                {
                    foreach (int id in deletePending)
                        songList.Delete(id);
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

            public static void ClearPending()
            {
                if (loading) return;

                deletePending.Clear();
                newPending.Clear();
            }

            public static bool CheckUnsavedB4Closing()
            {
                if (Pending() || changes) return !Ask("Discard unsaved changes?");
                else return false;
            }

            public static DataTable GetSongsInfo()
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("Id", Type.GetType("System.Int32")));
                table.Columns.Add(new DataColumn("State"));
                table.Columns.Add(new DataColumn("Song"));

                foreach (KeyValuePair<int, Song> song in songList.List())
                    if (!song.Value.IsDummy())
                    {
                        DataRow row = table.NewRow();
                        row["Id"] = song.Key;
                        row["Song"] = song.Value.Data("title_name");
                        if (deletePending.Contains(song.Key))
                            row["State"] = "Delete";

                        table.Rows.Add(row);
                    }

                return table;
            }

            public static DataTable GetPendingSongsInfo()
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("New Song"));

                foreach (string path in newPending)
                {
                    DataRow row = table.NewRow();
                    row["New Song"] = new DirectoryInfo(path).Name;
                    table.Rows.Add(row);
                }

                return table;
            }

            public static KeyValuePair<int, Song> GetSongListId(int id) { return new KeyValuePair<int, Song>(id, songList.Song(id)); }

            public static void RecordSelectedIndex(int id)
            {
                //DataTable table = GetSongsInfo();
                //selectedSongId = (int)table.Rows[i]["Id"];
                selectedSongId = id;
            }

            public static int GetSelectedIndex()
            {
                return selectedSongId;
            }

            public static Dictionary<string, string> GetLabels()
            {
                int id = selectedSongId;

                Dictionary<string, string> labels = new Dictionary<string, string>();

                string[] tags = { "title_name", "artist_name" };
                foreach (string tag in tags)
                    if (id == -1) labels[tag] = "";
                    else labels[tag] = songList.Song(id).Data(tag);

                return labels;
            }

            public static Image GetImage()
            {
                int id = selectedSongId;

                return songList.Song(id).Image();
            }

            public static string GetTitle()
            {
                if (Pending() || changes) return "KshToVox (Unsaved changes)";
                else return "KshToVox";
            }

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

        #endregion

        #region SharpUpdate
        public string ApplicationName
        {
            get { return "KshToVox.window"; }
        }

        public string ApplicationID
        {
            get { return "KshToVox.window"; }
        }

        public Assembly ApplicationAssembly
        {
            get { return Assembly.GetExecutingAssembly(); }
        }

        public Icon ApplicationIcon
        {
            get { return this.Icon; }
        }

        public Uri UpdateXmlLocation
        {
            get { return new Uri("https://github.com/MKLUO/KshToVox/raw/master/KshToVox_Update.xml"); }
        }

        public Form Context
        {
            get { return this; }
        }
        #endregion

    }
}
