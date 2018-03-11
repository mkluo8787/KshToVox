using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

using NAudio.Wave;

using Utility;

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
                    string kshPath,
                    int specificVer = 0)
        {
            // ogg mp3 wav

            if (!Directory.Exists(kshPath)) throw new DirectoryNotFoundException();

            string[] kshFiles = Directory.GetFiles(kshPath, "*.ksh");

            if (kshFiles.Length == 0) throw new Exception("No .ksh found in this folder!");

            // Parsing the first file for song infos.
            FileStream fs = new FileStream(kshFiles[0], FileMode.Open);
            (Dictionary<string, string> kshInfoParse, List<string> _) = ParseKshInfo(fs);
            fs.Close();

            data["label"] = label;
            data["title_name"] = kshInfoParse["title"];
            data["title_yomigana"] = "";
            data["artist_name"] = kshInfoParse["artist"];
            data["artist_yomigana"] = "";
            string[] asciiInfo = { data["title_name"], data["artist_name"] };
            data["ascii"] = MakeAscii(asciiInfo);

            string[] bpms = kshInfoParse["t"].Split('-');
            if (bpms.Length == 1)
            {
                data["bpm_max"] = string.Format("{0:0}", (Convert.ToDouble(bpms[0]) * 100.0));
                data["bpm_min"] = string.Format("{0:0}", (Convert.ToDouble(bpms[0]) * 100.0));
            }
            else if (bpms.Length == 2)
            {
                data["bpm_max"] = string.Format("{0:0}", (Convert.ToDouble(bpms[1]) * 100.0));
                data["bpm_min"] = string.Format("{0:0}", (Convert.ToDouble(bpms[0]) * 100.0));
            }
            else
                throw new Exception("Ksh bpm format error!");

            data["distribution_date"] = "22222222";
            data["volume"] = "100";
            data["bg_no"] = "0";
            data["genre"] = "16";
            data["is_fixed"] = "1";
            if (specificVer == 0)
                data["version"] = "3";
            else
                data["version"] = specificVer.ToString();
            data["demo_pri"] = "-2";
            data["inf_ver"] = "3";

            // Indicates that this is a custom simfile
            data["custom"] = "1";

            // Caching imformation
            data["kshFolder"] = kshPath;
            data["lastModFolder"] = Directory.GetLastWriteTime(kshPath).ToString();

            // Parsing for Charts
            foreach (string kshFile in kshFiles)
			{
                // Detect if some object is in 1st Measure (Shift with +1 measure)
                FileStream fs1 = new FileStream(kshFile, FileMode.Open);
                (Dictionary<string, string> kshCheckParse, List<string> chartCheck) = ParseKshInfo(fs1);
                fs1.Close();

                Chart checkVoxChart = new Chart(chartCheck, kshCheckParse["t"], false);

                bool shift = checkVoxChart.SomethingIsInFirstMeasure();

                double offset = checkVoxChart.FirstMesureLength();

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

                // Parsing for wav

                string soundPath = kshPath + "\\" + kshParse["m"].Split(';').ElementAt<string>(0);

                if (!soundCaches.ContainsKey(soundPath))
                {
                    string ext = Path.GetExtension(soundPath);
                    if (!((ext == ".mp3") || (ext == ".ogg") || (ext == ".wav")))
                        throw new Exception("Music file format " + ext + " invalid!");

                    string outSoundPath = Util.cachePath + BaseName() + Suffix(SongList.DIFS[id]) + ".wav";
                    string preSoundPath = Util.cachePath + BaseName() + Suffix(SongList.DIFS[id]) + "_p.wav";

                    string oriSoundPath = soundPath;

                    // SOX BUG: the mp3 to wav conversion seems to ignore the implicit offset in mp3.
                    // so here we do a conversion first with 
                    if (ext == ".mp3")
                    {
                        using (Mp3FileReader mp3 = new Mp3FileReader(soundPath))
                        {
                            using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                            {
                                soundPath = soundPath.Substring(0, soundPath.Length - 4) + ".wav";
                                WaveFileWriter.CreateWaveFile(soundPath, pcm);
                            }
                        }
                    }

                    if (shift)
                        ConvertAndTrimToWav(soundPath, outSoundPath, int.Parse(kshParse["o"]) - Convert.ToInt32(offset * 1000), -1);
                    else
                        ConvertAndTrimToWav(soundPath, outSoundPath, int.Parse(kshParse["o"]), -1);

                    if (soundCaches.Count == 0)
                        soundCaches.Add(oriSoundPath, new Tuple<string, string>(outSoundPath, ""));
                    else
                        soundCaches.Add(oriSoundPath, new Tuple<string, string>(outSoundPath, Suffix(SongList.DIFS[id])));

                    // Preview sound

                    ConvertAndTrimToWav(soundPath, preSoundPath, int.Parse(kshParse["po"]), int.Parse(kshParse["plength"]));

                    // Fix ms-adpcm BlockSize mismatch, and fade in/out effect for the sample music

                    Fade(preSoundPath);
                    WavBlockSizeFix(preSoundPath);                    

                    if (preSoundCaches.Count == 0)
                        preSoundCaches.Add(oriSoundPath, new Tuple<string, string>(preSoundPath, ""));
                    else
                        preSoundCaches.Add(oriSoundPath, new Tuple<string, string>(preSoundPath, Suffix(SongList.DIFS[id])));
                }

                // Parsing for Image

                string imagePath = kshPath + "\\" + kshParse["jacket"];

                // ONLY supporting one jacket now!

                image = new KshImage(imagePath);

                /*
                if (!images.ContainsKey(imagePath))
                {
                    string ext = Path.GetExtension(imagePath);
                    if (!((ext == ".bmp") || (ext == ".jpg") || (ext == ".png")))
                        throw new Exception("Image file format " + ext + " invalid!");

                    if (images.Count == 0)
                    {
                        string imageName = "jk_" + BaseNameNum();
                        images.Add(imagePath, new Tuple<Image, string>(new Image(imagePath, imageName), "_1"));
                    }                    
                    else
                        images.Add(imagePath, new Tuple<Image, string>(new Image(imagePath), "_" + id.ToString()));
                    
                }
                */

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

                // .2dx to wav (looking for difficulty-specific sound file)

                string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + Suffix(chartInfo.Key) + ".2dx";
                string outSoundPath = Util.cachePath + BaseName() + Suffix(chartInfo.Key) + ".wav";
                if (File.Exists(soundPath))
                {
                    FileStream sstream = new FileStream(soundPath, FileMode.Open);
                    FileStream osstream = new FileStream(outSoundPath, FileMode.Create);

                    sstream.Position = 0x64;
                    sstream.CopyTo(osstream);

                    sstream.Close();
                    osstream.Close();

                    soundCaches.Add(soundPath, new Tuple<string, string>(outSoundPath, ""));
                }
            }

            // .2dx to wav (looking for common sound file)

            string soundPath2 = kfcPath + "\\data\\sound\\" + BaseName() + ".2dx";
            string outSoundPath2 = Util.cachePath + BaseName() + ".wav";
            if (!File.Exists(soundPath2)) throw new Exception("");
            FileStream sstream2 = new FileStream(soundPath2, FileMode.Open);
            FileStream osstream2 = new FileStream(outSoundPath2, FileMode.Create);

            sstream2.Position = 0x64;
            sstream2.CopyTo(osstream2);

            sstream2.Close();
            osstream2.Close();

            soundCaches.Add(soundPath2, new Tuple<string, string>(outSoundPath2, ""));

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

            foreach (KeyValuePair<string, Tuple<string, string>> sc in soundCaches)
            {
                string soundPath = kfcPath + "\\data\\sound\\" + BaseName() + sc.Value.Item2 + ".2dx";
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

                FileStream fs = new FileStream(sc.Value.Item1, FileMode.Open);
                fs.CopyTo(osstream);
                fs.Close();

                long endPos = osstream.Position;
                bw.BaseStream.Position = 0x54;
                bw.Write((int)(endPos - 0x64));

                osstream.Close();
            }

            foreach (KeyValuePair<string, Tuple<string, string>> sc in preSoundCaches)
            {
                string soundPath = kfcPath + "\\data\\sound\\preview\\" + BaseNamePre() + sc.Value.Item2 + ".2dx";
                FileStream osstream = new FileStream(soundPath, FileMode.Create);

                BinaryWriter bw = new BinaryWriter(osstream);

                bw.BaseStream.Position = 0x10;
                bw.Write(0x0000004C);
                bw.Write(0x00000001);

                bw.BaseStream.Position = 0x48;
                bw.Write(0x0000004C);
                bw.Write(0x39584432);
                bw.Write(0x00000018);
                bw.Write(0x00000000); // Will be replaced with sound length later
                bw.Write(0xFFFF3231);
                bw.Write(0x00010040);
                bw.Write(0x00000000);

                FileStream fs = new FileStream(sc.Value.Item1, FileMode.Open);
                fs.CopyTo(osstream);
                fs.Close();

                long endPos = osstream.Position;
                bw.BaseStream.Position = 0x54;
                bw.Write((int)(endPos - 0x64));

                osstream.Close();
            }
        }

        public Task ImageToTex(string tgaName, string texPath, int pixel)
        {
            Util.ConsoleWrite("Replacing " + image.Name() + " into " + tgaName + "...");

            // Image to tga (in cache)
            string tgaPath = Util.cachePath + tgaName;

            image.ToTga(tgaPath, pixel);

            // tga to tex
            Task task = Task.Run(() => Util.TgaToTex_Thread(tgaPath, texPath));

            return task;
        }

        private static void ConvertAndTrimToWav(string src, string dest, int trimMs, int duraMs)
		{
            double trimSec = System.Convert.ToDouble(trimMs) * 0.001;
            //double duraSec = System.Convert.ToDouble(duraMs) * 0.001;

            double duraSec = 10.30;

            string soxExe = Util.toolsPath + "sox\\sox.exe";

            if (duraMs < 0)
            {
                if (trimMs > 0)
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\" trim " + trimSec.ToString() + " -0.0");
                else if (trimMs < 0)
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\" pad " + (-trimSec).ToString() + " 0.0");
                else
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\"");
            }
            else
            {
                if (trimMs > 0)
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\" trim " + trimSec.ToString() + " " + duraSec.ToString());
                else if (trimMs < 0)
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\" pad " + (-trimSec).ToString() + " 0.0 trim 0 " + duraSec.ToString());
                else
                    Util.Execute(soxExe, "-G -q \"" + src + "\" -r 44100 -e ms-adpcm \"" + dest + "\" trim 0 " + duraSec.ToString());
            }
        }

        private static void Fade(string src)
        {
            string dest = src;
            FileInfo currentFile = new FileInfo(src);
            string source = currentFile.Directory.FullName + "\\" + Util.RandomString(20) + currentFile.Extension;
            currentFile.MoveTo(source);

            string soxExe = Util.toolsPath + "sox\\sox.exe";

            Util.Execute(soxExe, "-G -q \"" + source + "\" \"" + dest + "\" fade q 2 0 2");
        }

        private static void WavBlockSizeFix(string src)
        {
            string dest = src;
            FileInfo currentFile = new FileInfo(src);
            string source = currentFile.Directory.FullName + "\\" + Util.RandomString(20) + currentFile.Extension;
            currentFile.MoveTo(source);

            string convertExe = Util.toolsPath + "2dxConvert\\2dxWavConvert.exe";

            Util.Execute(convertExe, "\"" + source + "\" \"" + dest + "\" preview");
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

        public string BaseNamePre()
        {
            return data["version"].PadLeft(3, '0') + "_" +
                    data["label"].PadLeft(4, '0') + "_" +
                    "pre";
        }

        public string BaseNameNum()
        {
            return data["version"].PadLeft(3, '0') + "_" +
                    data["label"].PadLeft(4, '0');
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

        // Override
        public override string ToString()
		{
            return Data("ascii");
		}

		// Song Attributes

		Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> chartData = new Dictionary<string, Dictionary<string, string>>();

        // Image (Jacket)

        //Dictionary<string, Tuple<Image, string>> images = new Dictionary<string, Tuple<Image, string>>();
        KshImage image;

        //string songCachePath;

        Dictionary<string, Tuple<string, string>> soundCaches = new Dictionary<string, Tuple<string, string>>();
        Dictionary<string, Tuple<string, string>> preSoundCaches = new Dictionary<string, Tuple<string, string>>();
        bool loaded = false;
        bool dummy = false;

        // Charts
        private Dictionary<string, Chart> charts = new Dictionary<string, Chart>();
	}
}
