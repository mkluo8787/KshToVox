using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SongList
{
	public class Song
	{
		// Methods

		// From K-Shoot
		public Song(string kshPath) { }

		// From KFC
		public Song(Dictionary<string, string> data_,
					Dictionary<string, int> difficulty_,
					string kfcPath)
		{
			data = data_;
			difficulty = difficulty_;

			string base_name =	data["version"].PadLeft(3, '0')	+ "_" +
								data["label"].PadLeft(4, '0')	+ "_" +
								data["ascii"];

			foreach (KeyValuePair<string, int> difInfo in difficulty)
			{
				if (difInfo.Value == 0) continue;

				string suffix;
				if		(difInfo.Key == "novice")	suffix = "_1n";
				else if (difInfo.Key == "advanced")	suffix = "_2a";
				else if (difInfo.Key == "exhaust")	suffix = "_3e";
				else if (difInfo.Key == "infinite")	suffix = "_4i";
				else throw new Exception("Difficulty name " + difInfo.Key + " invalid!");

				string chartPath = kfcPath + "\\data\\others\\vox\\" + base_name + suffix + ".vox";
				if (!File.Exists(chartPath)) throw new FileNotFoundException();
				FileStream stream = new FileStream(chartPath, FileMode.Open);

				charts[difInfo.Key] = new Chart(stream);

				stream.Close();
			}
		}

		public string Ascii() { return data["ascii"]; }
		public string Title() { return data["title"]; }

		public override string ToString()
		{
			return data["ascii"];
		}

		// Song Attributes

		Dictionary<string, string> data = new Dictionary<string, string>();
		Dictionary<string, int> difficulty = new Dictionary<string, int>();

		// Charts
		private Dictionary<string, Chart> charts = new Dictionary<string, Chart>();

		// Music (wav ms-adpcm)
		private Stream music;
	}
}
