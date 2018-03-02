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

        // dummy
        public Song()
        {
            dummy = true;
        }

        public bool IsDummy() { return dummy; }

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
			(Dictionary<string, string> kshCheckParse, List<string> chartCheck) = ParseKshInfo(fs);
			fs.Close();

            data["label"] = label;
            data["title_name"] = kshCheckParse["title"];
            data["title_yomigana"] = "";
            data["artist_name"] = kshCheckParse["artist"];
            data["artist_yomigana"] = "";
            string[] asciiInfo = { data["title_name"], data["artist_name"] };
            data["ascii"] = MakeAscii(asciiInfo);
            data["bpm_max"] = "99999";
            data["bpm_min"] = "99999";
            data["distribution_date"] = "22222222";
            data["volume"] = "100";
            data["bg_no"] = "0";
            data["genre"] = "16";
            data["is_fixed"] = "1";
            data["version"] = "3";
            data["demo_pri"] = "-2";
            data["inf_ver"] = "3";

            // Indicates that this is a custom simfile
            data["custom"] = "1";

            // Detect if some object is in 1st Measure (Shift with +1 measure)
            Chart checkVoxChart = new Chart(chartCheck, kshCheckParse["t"], false);
            bool shift;
            if (checkVoxChart.SomethingIsInFirstMeasure())
                shift = true;
            else
                shift = false;

            double offset = checkVoxChart.FirstMesureLength();


            // Parsing for Charts
            foreach (string kshFile in kshFiles)
			{
				FileStream fs2 = new FileStream(kshFile, FileMode.Open);
				(Dictionary<string, string> kshParse, List<string> chart) = ParseKshInfo(fs2);
				fs2.Close();

                int id = 0;
                if      (kshParse["difficulty"] == "light")     id = 0;
                else if (kshParse["difficulty"] == "challenge") id = 1;
                else if (kshParse["difficulty"] == "extended")  id = 2;
                else if (kshParse["difficulty"] == "infinite")  id = 3;
                else new Exception("Invalid dif num in .ksh!");

                charts		[SongList.DIFS[id]] = new Chart(chart, kshParse["t"], shift);

                chartData   [SongList.DIFS[id]] = new Dictionary<string, string>();
                chartData   [SongList.DIFS[id]]["difnum"]       = kshParse["level"];
                chartData   [SongList.DIFS[id]]["illustrator"]  = kshParse["illustrator"];
                chartData   [SongList.DIFS[id]]["effected_by"]  = kshParse["effect"];
                chartData   [SongList.DIFS[id]]["price"]        = "-1";
                chartData   [SongList.DIFS[id]]["limited"]      = "3";
			}

            // Fill in empty data for chartdata
            foreach (string dif in SongList.DIFS)
            {
                if (!chartData.ContainsKey(dif))
                {
                    chartData[dif] = new Dictionary<string, string>();
                    chartData[dif]["difnum"]        = "0";
                    chartData[dif]["illustrator"]   = "";
                    chartData[dif]["effected_by"]   = "";
                    chartData[dif]["price"]         = "-1";
                    chartData[dif]["limited"]       = "3";
                }
            }

            // Parsing for wav

            string soundPath = kshPath + "\\" + kshCheckParse["m"].Split(';').ElementAt<string>(0);

			string ext = Path.GetExtension(soundPath);
			if (!((ext == ".mp3") || (ext == ".ogg") || (ext == ".wav")))
				throw new Exception("Music file format " + ext + " invalid!");

			string outSoundPath = SongList.cachePath + BaseName() + ".wav";
            if (shift)
			    ConvertAndTrimToWav(soundPath, outSoundPath, int.Parse(kshCheckParse["o"]) - Convert.ToInt32(offset * 1000));
            else
                ConvertAndTrimToWav(soundPath, outSoundPath, int.Parse(kshCheckParse["o"]));

            songCachePath = outSoundPath;
            loaded = true;
        }

		// From KFC
		public Song(Dictionary<string, string>	data_,
                    Dictionary<string, Dictionary<string, string>> chartData_,
					string kfcPath,
                    bool load)
		{
            data = data_;
            chartData = chartData_;
            

            if (!load) return; 
            // load:    If false, skips the entire .vox & .2dx parsing procedure.

            // Make Charts

            foreach (KeyValuePair<string, Dictionary<string, string>> chartInfo in chartData)
			{
				if (chartInfo.Value["difnum"] == "0") continue;

				string chartPath = kfcPath + "\\data\\others\\vox\\" + BaseName() + Suffix(chartInfo.Key) + ".vox";
                if (!File.Exists(chartPath))
                {
                    chartPath = SongList.GetCachePath() + BaseName() + Suffix(chartInfo.Key) + ".vox";
                    if (!File.Exists(chartPath))
                        throw new FileNotFoundException();
                }
				FileStream cstream = new FileStream(chartPath, FileMode.Open);

				charts[chartInfo.Key] = new Chart(cstream);

				cstream.Close();
            }

            // .2dx to wav

            string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + ".2dx";
            string outSoundPath = SongList.cachePath + BaseName() + ".wav";
            if (!File.Exists(soundPath)) throw new FileNotFoundException();
            FileStream sstream = new FileStream(soundPath, FileMode.Open);
            FileStream osstream = new FileStream(outSoundPath, FileMode.Create);

            sstream.Position = 0x64;
            sstream.CopyTo(osstream);

            sstream.Close();
            osstream.Close();

            songCachePath = outSoundPath;
            loaded = true;
        }

        // Write to Vox and 2dx
        public void Save(string kfcPath)
        {
            if (!loaded) return;

            foreach (KeyValuePair<string, Dictionary<string, string>> chartInfo in chartData)
            {
                if (chartInfo.Value["difnum"] == "0") continue;

                string chartPath = kfcPath + "\\data\\others\\vox\\" + BaseName() + Suffix(chartInfo.Key) + ".vox";
                FileStream cstream = new FileStream(chartPath, FileMode.Create);

                MemoryStream mstream = charts[chartInfo.Key].ToVox();
                mstream.Position = 0;
                mstream.CopyTo(cstream);

                mstream.Close();
                cstream.Close();
            }

            string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + ".2dx";
            FileStream osstream = new FileStream(soundPath, FileMode.Create);

            BinaryWriter bw = new BinaryWriter(osstream);

            bw.BaseStream.Position = 0x48;
            bw.Write(0x0000004C);
            bw.Write(0x39584432);
            bw.Write(0x00000018);
            bw.Write(0x00000000); // Will be replaced with sound length later
            bw.Write(0xFFFF3231);
            bw.Write(0x00010040);
            bw.Write(0x00000000);

            FileStream fs = GetWav();
            fs.CopyTo(osstream);
            fs.Close();

            long endPos = osstream.Position;
            bw.BaseStream.Position = 0x54;
            bw.Write((int)(endPos - 0x64));

            osstream.Close();
        }

		// Music (wav ms-adpcm)
		private FileStream GetWav()
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
		public string Data(string tag)
        {
            if (data.ContainsKey(tag))
                return data[tag];
            else
                return "";
        }

        public Dictionary<string, string> Dict() { return data; }

        public Dictionary<string, Dictionary<string, string>> ChartDict() { return chartData; }

        public Dictionary<string, string> ChartInfo(string tag)
		{
			if (!chartData.ContainsKey(tag))
				throw new Exception("Unknown Difficulty!");
			return chartData[tag];
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
            return Data("ascii");
		}

		// Song Attributes

		Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> chartData = new Dictionary<string, Dictionary<string, string>>();

        string songCachePath;
        bool loaded = false;
        bool dummy = false;

        // Charts
        private Dictionary<string, Chart> charts = new Dictionary<string, Chart>();
	}
}
