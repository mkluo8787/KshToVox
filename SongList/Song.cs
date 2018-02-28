using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

namespace SongList
{
	public class Song
	{
		// Methods

		// From K-Shoot
		public Song(string label,
					string kshPath)
		{
			// ogg mp3 wav

			if (!Directory.Exists(kshPath)) throw new DirectoryNotFoundException();

			string[] kshFiles = Directory.GetFiles(kshPath, "*.ksh");

			if (kshFiles.Length == 0) throw new Exception("No .ksh found in this folder!");

			// Parsing the first file for song infos.
			FileStream fs = new FileStream(kshFiles[0], FileMode.Open);
			(Dictionary<string, string> kshCheckParse, List<string> _) = ParseKshInfo(fs);
			fs.Close();

			data["label"] = label;
			data["title"] = kshCheckParse["title"];
			data["artist"] = kshCheckParse["artist"];
			string[] asciiInfo = { data["title"], data["artist"] };
			data["ascii"] = MakeAscii(asciiInfo);
			data["version"] = "3";
			data["inf_ver"] = "3";

			// Parsing for Charts
			foreach (string kshFile in kshFiles)
			{
				FileStream fs2 = new FileStream(kshFile, FileMode.Open);
				(Dictionary<string, string> kshParse, List<string> chart) = ParseKshInfo(fs2);
				fs2.Close();

				if		(kshParse["difficulty"] == "light")
				{
					charts		[SongList.DIFS[0]] = new Chart(chart);
					difficulty	[SongList.DIFS[0]] = int.Parse(kshParse["level"]);
				}
				else if	(kshParse["difficulty"] == "challenge")
				{
					charts		[SongList.DIFS[1]] = new Chart(chart);
					difficulty	[SongList.DIFS[1]] = int.Parse(kshParse["level"]);
				}
				else if (kshParse["difficulty"] == "extended")
				{
					charts		[SongList.DIFS[2]] = new Chart(chart);
					difficulty	[SongList.DIFS[2]] = int.Parse(kshParse["level"]);
				}
				else if (kshParse["difficulty"] == "infinite")
				{
					charts		[SongList.DIFS[3]] = new Chart(chart);
					difficulty	[SongList.DIFS[3]] = int.Parse(kshParse["level"]);
				}
			}

			// Parsing for wav

			string soundPath = kshPath + "\\" + kshCheckParse["m"].Split(';').ElementAt<string>(0);

			string ext = Path.GetExtension(soundPath);
			if (!((ext == ".mp3") || (ext == ".ogg") || (ext == ".wav")))
				throw new Exception("Music file format " + ext + " invalid!");

			string outSoundPath = SongList.cachePath + BaseName() + ".wav";
			ConvertAndTrimToWav(soundPath, outSoundPath, int.Parse(kshCheckParse["o"]));

            songCachePath = outSoundPath + RandomString(10);
            if (!File.Exists(outSoundPath)) throw new FileNotFoundException();
            System.IO.File.Move(outSoundPath, songCachePath);
        }

		// From KFC
		public Song(Dictionary<string, string>	data_,
					Dictionary<string, int>		difficulty_,
					string kfcPath)
		{
			data = data_;
			difficulty = difficulty_;

			foreach (KeyValuePair<string, int> difInfo in difficulty)
			{
				if (difInfo.Value == 0) continue;

				string chartPath = kfcPath + "\\data\\others\\vox\\" + BaseName() + Suffix(difInfo.Key) + ".vox";
				if (!File.Exists(chartPath)) throw new FileNotFoundException();
				FileStream cstream = new FileStream(chartPath, FileMode.Open);

				charts[difInfo.Key] = new Chart(cstream);

				cstream.Close();
            }

            // .2dx to wav

            string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + ".2dx";
            string outSoundPath = SongList.cachePath + BaseName() + ".wav" + RandomString(10);
            if (!File.Exists(soundPath)) throw new FileNotFoundException();
            FileStream sstream = new FileStream(soundPath, FileMode.Open);
            FileStream osstream = new FileStream(outSoundPath, FileMode.Create);

            sstream.Position = 0x64;
            sstream.CopyTo(osstream);

            sstream.Close();
            osstream.Close();

            songCachePath = outSoundPath;
        }

        // Write to Vox
        public void Save(string kfcPath)
        {
            foreach (KeyValuePair<string, int> difInfo in difficulty)
            {
                if (difInfo.Value == 0) continue;

                string chartPath = kfcPath + "\\data\\others\\vox\\" + BaseName() + Suffix(difInfo.Key) + ".vox";
                FileStream cstream = new FileStream(chartPath, FileMode.Create);

                MemoryStream mstream = charts[difInfo.Key].ToVox();
                mstream.Position = 0;
                mstream.CopyTo(cstream);

                mstream.Close();
                cstream.Close();
            }
        }

		// Music (wav ms-adpcm)
		public FileStream GetWav()
		{
			if (!File.Exists(songCachePath)) throw new FileNotFoundException();
			return new FileStream(songCachePath, FileMode.Open);
		}

		private static void ConvertAndTrimToWav(string src, string dest, int trimMs)
		{
            double trimSec = System.Convert.ToDouble(trimMs) * 0.001;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "sox.exe";
            if (trimMs > 0)
			    startInfo.Arguments = "-G -q \"" + src + "\" -e ms-adpcm \"" + dest + "\" trim " + trimSec.ToString() + " -0.0";
            else if (trimMs < 0)
                startInfo.Arguments = "-G -q \"" + src + "\" -e ms-adpcm \"" + dest + "\" pad " + (-trimSec).ToString() + " 0.0";
            else
                startInfo.Arguments = "-G -q \"" + src + "\" -e ms-adpcm \"" + dest + "\"";
            process.StartInfo = startInfo;
            Console.WriteLine(startInfo.Arguments);
			process.Start();
            process.WaitForExit();
        }

		// Utils
		public string Data(string tag) { return data[tag]; }

		public int Difficulty(string tag)
		{
			if (!difficulty.ContainsKey(tag))
				return 0;
			else
				return difficulty[tag];
		}

		public string BaseName()
		{
			return	data["version"].PadLeft(3, '0') + "_" +
					data["label"].PadLeft(4, '0') + "_" +
					data["ascii"];
		}

		private static string MakeAscii(string[] tokens)
		{
			string ascii = "";
			char[] headTailTrim = { '_', ' ' };
			Regex rgx = new Regex("[^a-zA-Z0-9 -]");

			foreach (string token in tokens)
			{
				string tokenRgx = rgx.Replace(token, "");
				ascii += tokenRgx.Replace(' ', '_').Replace('-', '_').ToLower().Trim(headTailTrim) + "_";
			}
			ascii = ascii.Remove(ascii.Length - 1);

			return ascii;
		}

		private static string Suffix(string dif)
		{
			if (dif == "novice") return "_1n";
			else if (dif == "advanced") return "_2a";
			else if (dif == "exhaust") return "_3e";
			else if (dif == "infinite") return "_4i";
			else throw new Exception("Suffix Invalid!");
		}

		private static (Dictionary<string, string>, List<string>) ParseKshInfo(FileStream fs)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			List<string> chart = new List<string>();

			StreamReader sr = new StreamReader(fs);

			while (sr.Peek() >= 0)
			{
				string line = sr.ReadLine();

				// The start of chart
				if (line == "--")
				{ 
					while (sr.Peek() >= 0)
					{
						string chartLine = sr.ReadLine();
						if (chartLine.Contains("#define_fx"))
							break;
						chart.Add(chartLine);
					}
					break;
				}

				string[] tokens = line.Split('=');
				if (tokens.Length != 2) throw new Exception("Invalid format in .ksh file!");
				data[tokens[0]] = tokens[1];
			}

			return (data, chart);
		}

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Override
        public override string ToString()
		{
			return data["ascii"];
		}

		// Song Attributes

		Dictionary<string, string> data = new Dictionary<string, string>();
		Dictionary<string, int> difficulty = new Dictionary<string, int>();

        string songCachePath;

		// Charts
		private Dictionary<string, Chart> charts = new Dictionary<string, Chart>();
	}
}
