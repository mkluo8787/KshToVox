﻿using System;
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

			foreach (KeyValuePair<string, int> difInfo in difficulty)
			{
				if (difInfo.Value == 0) continue;

				string suffix;
				if		(difInfo.Key == "novice")	suffix = "_1n";
				else if (difInfo.Key == "advanced")	suffix = "_2a";
				else if (difInfo.Key == "exhaust")	suffix = "_3e";
				else if (difInfo.Key == "infinite")	suffix = "_4i";
				else throw new Exception("Difficulty name " + difInfo.Key + " invalid!");

				string chartPath = kfcPath + "\\data\\others\\vox\\" + BaseName() + suffix + ".vox";
				if (!File.Exists(chartPath)) throw new FileNotFoundException();
				FileStream cstream = new FileStream(chartPath, FileMode.Open);

				charts[difInfo.Key] = new Chart(cstream);

				cstream.Close();
			}

			// .2dx to wav

			string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + ".2dx";
			if (!File.Exists(soundPath)) throw new FileNotFoundException();
			FileStream sstream = new FileStream(soundPath, FileMode.Open);

			sstream.Position = 0x64;
			sstream.CopyTo(music);

			sstream.Close();
			/*
			string outSoundPath = kfcPath + "\\data\\sound\\" + base_name + ".wav";
			FileStream osstream = new FileStream(outSoundPath, FileMode.Create);
			music.WriteTo(osstream);

			osstream.Close();
			*/
		}

		public string Data(string tag) { return data[tag]; }

		public int Difficulty(string tag) { return difficulty[tag]; }

		public string BaseName()
		{
			return	data["version"].PadLeft(3, '0') + "_" +
					data["label"].PadLeft(4, '0') + "_" +
					data["ascii"];
		}

		public MemoryStream GetWav() { return music; }

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
		private MemoryStream music = new MemoryStream();
	}
}