using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SongList
{
	class Chart
	{
		class TimePos
		{
			private int measure;
			private int beat;
			private int time;
            public TimePos(int measure, int beat, int time)
            {
                this.measure = measure;
                this.beat = beat;
                this.time = time;
            }
            public TimePos(string s)
            {
                string[] tokens = s.Split(',');
                measure = int.Parse(tokens[0]);
                beat    = int.Parse(tokens[1]);
                time    = int.Parse(tokens[2]);
            }
            public void Print()
            {
                Console.WriteLine("({0}, {1}, {2})", this.measure, this.beat, this.time);
            }
            // for Vol
            public void fixSlam(TimePos prev)
            {
                if (this.measure == prev.measure && this.beat == prev.beat
                                                 && this.time == prev.time + 6)
                {
                    this.time = prev.time;
                }
            }

            public int Measure()
            {
                return measure;
            }

            //Overwrite
            public override string ToString()
            {
                return  measure.ToString().PadLeft(3, '0') + "," + 
                        beat.ToString().PadLeft(2, '0') + "," + 
                        time.ToString().PadLeft(2, '0');
            }
        }
		class FxEffect
		{
			enum FxType
			{
			}

			private double attributes;
		}
		class Sp
		{
			enum SpType
			{
			}

			private double attributes;
		}

		class Vol
		{
			private int	pos;
			private int	flag;
			private int	flip;
			private int	filter;
			private int	expand;
            public Vol(int pos, int flag, int expand)
            {
                this.pos = pos;
                this.flag = flag;
                this.expand = expand;
                this.flip = 0;
                this.filter = 0;
            }
            public void setFlag(int flag)
            {
                this.flag = flag;
            }
            public void setExpand(int expand)
            {
                this.expand = expand;
            }
            public void Print()
            {
                Console.WriteLine("({0}, {1}, {2})", this.pos, this.flag, this.expand);
            }
            //Overwrite
            public override string ToString()
            {
                return pos.ToString() + "\t" +
                        flag.ToString() + "\t" +
                        flip.ToString() + "\t" +
                        filter.ToString() + "\t" +
                        expand.ToString();
            }
        }
		class Fx
		{
			private int	length;
			private FxEffect effect;
            public Fx(int length)
            {
                this.length = length;
            }
            public void addLength(int length)
            {
                this.length += length;
            }
            public void Print()
            {
                Console.WriteLine(this.length);
            }
            //Overwrite
            public override string ToString()
            {
                return length.ToString() + "\t" +
                        ((length == 0) ? 255 : 2).ToString();
            }
        }
		class Bt
		{
			private int length;
            public Bt(int length)
            {
                this.length = length;
            }
            public void addLength(int length)
            {
                this.length += length;
            }
            public void Print()
            {
                Console.WriteLine(this.length);
            }
            //Overwrite
            public override string ToString()
            {
                return length.ToString() + "\t" +
                        ((length == 0) ? 255 : 2).ToString();
            }
        }

        // From .vox
        static char[] whitespace = new char[] { ' ', '\t' };

        public Chart(Stream s)
        {
            // Parse .vox into Chart object
            StreamReader sr = new StreamReader(s);
            string line;
            
            GoToTag(sr, "#BEAT INFO");
            beat = new List<Tuple<TimePos, Tuple<int, int>>>();
            while (true)
            {
                line = sr.ReadLine();
                if (line.Contains("#END")) break;

                string[] tokens = line.Split(whitespace);
                beat.Add(new Tuple<TimePos, Tuple<int, int>>(new TimePos(tokens[0]),
                        new Tuple<int, int>(int.Parse(tokens[1]), int.Parse(tokens[2]))));
            }
            GoToTag(sr, "#BPM INFO");
            bpm = new List<Tuple<TimePos, double>>();
            while (true)
            {
                line = sr.ReadLine();
                if (line.Contains("#END")) break;

                string[] tokens = line.Split(whitespace);
                bpm.Add(new Tuple<TimePos, double>(new TimePos(tokens[0]),
                        Convert.ToDouble(tokens[1])));
            }

            GoToTag(sr, "#END POSITION");
            line = sr.ReadLine();
            endPos = new TimePos(line);

            volL = ParseVoxVol(sr, "#TRACK1");
            fxL = ParseVoxFx(sr, "#TRACK2");
            btA = ParseVoxBt(sr, "#TRACK3");
            btB = ParseVoxBt(sr, "#TRACK4");
            btC = ParseVoxBt(sr, "#TRACK5");
            btD = ParseVoxBt(sr, "#TRACK6");
            fxR = ParseVoxFx(sr, "#TRACK7");
            volR = ParseVoxVol(sr, "#TRACK8");
        }

        // Utils for vox parsing
        private static List<Tuple<TimePos, Vol>> ParseVoxVol(StreamReader sr, string tag)
        {
            List<Tuple<TimePos, Vol>> list = new List<Tuple<TimePos, Vol>>();

            GoToTag(sr, tag);
            while (true)
            {
                string line = sr.ReadLine();
                if (line.Contains("#END")) break;

                string[] tokens = line.Split(whitespace);
                list.Add(new Tuple<TimePos, Vol>(new TimePos(tokens[0]),
                            new Vol(int.Parse(tokens[1]),
                                    int.Parse(tokens[2]),
                                    int.Parse(tokens[5]))));
            }
            return list;
        }

        private static List<Tuple<TimePos, Bt>> ParseVoxBt(StreamReader sr, string tag)
        {
            List<Tuple<TimePos, Bt>> list = new List<Tuple<TimePos, Bt>>();

            GoToTag(sr, tag);
            while (true)
            {
                string line = sr.ReadLine();
                if (line.Contains("#END")) break;

                string[] tokens = line.Split(whitespace);
                list.Add(   new Tuple<TimePos, Bt>(new TimePos(tokens[0]),
                            new Bt(int.Parse(tokens[1]))));
            }
            return list;
        }

        private static List<Tuple<TimePos, Fx>> ParseVoxFx(StreamReader sr, string tag)
        {
            List<Tuple<TimePos, Fx>> list = new List<Tuple<TimePos, Fx>>();

            GoToTag(sr, tag);
            while (true)
            {
                string line = sr.ReadLine();
                if (line.Contains("#END")) break;

                string[] tokens = line.Split(whitespace);
                list.Add(new Tuple<TimePos, Fx>(new TimePos(tokens[0]),
                            new Fx(int.Parse(tokens[1]))));
            }
            return list;
        }

        // From .ksh
        public Chart(List<string> chartList, string initBpm, bool shift)
		{
            this.shift = shift;

            /****************************************
             initialize variables
            ****************************************/
            this.bpm = new List<Tuple<TimePos, double>>();
            this.beat = new List<Tuple<TimePos, Tuple<int, int>>>();
            this.fxList = new List<FxEffect>();
            this.volL = new List<Tuple<TimePos, Vol>>();
            this.fxL = new List<Tuple<TimePos, Fx>>();
            this.btA = new List<Tuple<TimePos, Bt>>();
            this.btB = new List<Tuple<TimePos, Bt>>();
            this.btC = new List<Tuple<TimePos, Bt>>();
            this.btD = new List<Tuple<TimePos, Bt>>();
            this.fxR = new List<Tuple<TimePos, Fx>>();
            this.volR = new List<Tuple<TimePos, Vol>>();
            this.sp = new List<Tuple<TimePos, Sp>>();

            /****************************************
             important variables and dictionaries
            ****************************************/
            int barCount = 0;
            Dictionary<int, int> index2barNbr = new Dictionary<int, int>();
            Dictionary<int, int> barNbr2rowNbr = new Dictionary<int, int>(); // rowNbr means the lines in the bar
            Dictionary<int, int> barNbr2beatUnit = new Dictionary<int, int>();
            Dictionary<int, Tuple<int, int>> barNbr2beat = new Dictionary<int, Tuple<int, int>>();
            Dictionary<int, TimePos> index2TimePos = new Dictionary<int, TimePos>();

            /****************************************
             some process related to bar
            ****************************************/
            // index2barNbr and barCount
            for (int i = 0; i < chartList.Count; i++)
            {
                index2barNbr.Add(i, barCount + 1);
                if (chartList[i].Substring(0, 2) == "--")
                    barCount++;
            }
            // barNbr2beat
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 5)
                    continue;
                if (chartList[i].Substring(0, 5) == "beat=")
                    barNbr2beat.Add(index2barNbr[i], 
                        new Tuple<int, int>((int)(chartList[i][5] - '0'), (int)(chartList[i][7] - '0')));
            }
            barNbr2beat[0] = new Tuple<int, int>(4, 4);
            for (int i = 1; i <= barCount; i++)
            {
                if (!barNbr2beat.ContainsKey(i))
                    barNbr2beat.Add(i, barNbr2beat[i - 1]);
            }
            // barNbr2rowNbr
            int rowNbrTmp = 0;
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Substring(0, 2) == "--")
                {
                    barNbr2rowNbr.Add(index2barNbr[i], rowNbrTmp);
                    rowNbrTmp = 0;
                }
                else if (char.IsNumber(chartList[i][0]))
                    rowNbrTmp++;
            }
            // barNbr2beatUnit
            for (int i = 1; i <= barCount; i++)
            {
                barNbr2beatUnit.Add(i, barNbr2beat[i].Item1 * 48 / barNbr2rowNbr[i]);
            }
            // index2TimePos
            rowNbrTmp = 0;
            int beatTotal;
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Substring(0, 2) == "--")
                {
                    rowNbrTmp = 0;
                    continue;
                }
                beatTotal = barNbr2beatUnit[index2barNbr[i]] * rowNbrTmp;
                index2TimePos.Add(i, new TimePos(index2barNbr[i] + (shift ? 1 : 0), beatTotal/48 + 1, beatTotal%48));
                if (char.IsNumber(chartList[i][0]))
                    rowNbrTmp++;
            }

            /****************************************
                               BT
            ****************************************/
            void getBTinfo(List<Tuple<TimePos, Bt>> bt, List<string> cL, Dictionary<int, TimePos> i2T,
                           Dictionary<int, int> i2bN, Dictionary<int, int> b2bU, int BTtype)
            {
                bool isLong = false;
                for (int i = 0; i < cL.Count; i++)
                {
                    if (char.IsNumber(cL[i][0]))
                    {
                        if (cL[i][BTtype] == '1')
                        {
                            isLong = false;
                            bt.Add(new Tuple<TimePos, Bt>(index2TimePos[i], new Bt(0)));
                        }
                        else if (cL[i][BTtype] == '2')
                        {
                            if (isLong)
                            {
                                bt[bt.Count - 1].Item2.addLength(b2bU[i2bN[i]]);
                            }
                            else
                            {
                                isLong = true;
                                bt.Add(new Tuple<TimePos, Bt>(index2TimePos[i], new Bt(b2bU[i2bN[i]])));
                            }
                        }
                        else
                        {
                            isLong = false;
                        }
                    }
                }
            }
            getBTinfo(this.btA, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 0);
            getBTinfo(this.btB, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 1);
            getBTinfo(this.btC, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 2);
            getBTinfo(this.btD, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 3);

            /****************************************
                               FX
            ****************************************/
            void getFXinfo(List<Tuple<TimePos, Fx>> fx, List<string> cL, Dictionary<int, TimePos> i2T,
                           Dictionary<int, int> i2bN, Dictionary<int, int> b2bU, int FXtype)
            {
                bool isLong = false;
                string effect;
                for (int i = 0; i < cL.Count; i++)
                {
                    if (char.IsNumber(cL[i][0]))
                    {
                        if (cL[i][FXtype] == '2')
                        {
                            isLong = false;
                            fx.Add(new Tuple<TimePos, Fx>(index2TimePos[i], new Fx(0)));
                        }
                        else if (cL[i][FXtype] == '1')
                        {
                            if (isLong)
                            {
                                fx[fx.Count - 1].Item2.addLength(b2bU[i2bN[i]]);
                            }
                            else
                            {
                                isLong = true;
                                fx.Add(new Tuple<TimePos, Fx>(index2TimePos[i], new Fx(b2bU[i2bN[i]])));
                            }
                        }
                        else
                        {
                            isLong = false;
                        }
                    }
                    else if (cL[i].Length < 5)
                        continue;
                    else if (cL[i].Substring(0, 5) == "fx-l=" && FXtype == 5)
                        effect = cL[i].Substring(5);
                    else if (cL[i].Substring(0, 5) == "fx-r=" && FXtype == 6)
                        effect = cL[i].Substring(5);
                }
            }
            getFXinfo(this.fxL, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 5);
            getFXinfo(this.fxR, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 6);

            /****************************************
                              VOL
            ****************************************/
            Dictionary<char, int> Pos = new Dictionary<char, int>();
            for (int i = 48; i < 58; i++)
                Pos[(char)i] = (int)(Math.Round(127.0 / 50.0 * (i-48)));
            for (int i = 65; i < 91; i++)
                Pos[(char)i] = (int)(Math.Round(127.0 / 50.0 * (i-55)));
            for (int i = 97; i < 112; i++)
                Pos[(char)i] = (int)(Math.Round(127.0 / 50.0 * (i-61)));
            
            void getVOLinfo(List<Tuple<TimePos, Vol>> vol, List<string> cL,
                            Dictionary<int, TimePos> i2T, int VOLtype)
            {
                bool inLine = false;
                bool expand = false;
                for (int i = 0; i < cL.Count; i++)
                {
                    if (char.IsNumber(cL[i][0]))
                    {
                        if (cL[i][VOLtype] == '-')
                        {
                            if (inLine)
                            {
                                vol[vol.Count - 1].Item2.setFlag(2);
                                inLine = expand = false;
                            }
                        }
                        else if (Pos.ContainsKey(cL[i][VOLtype]))
                        {
                            vol.Add(new Tuple<TimePos, Vol>(index2TimePos[i],
                                        new Vol(Pos[cL[i][VOLtype]], 0, 1)));
                            if (expand)
                                vol[vol.Count - 1].Item2.setExpand(2);
                            if (!inLine)
                            {
                                inLine = true;
                                vol[vol.Count - 1].Item2.setFlag(1);
                            }
                        }
                    }
                    else if (cL[i] == "laserrange_l=2x" && VOLtype == 8) expand = true;
                    else if (cL[i] == "laserrange_r=2x" && VOLtype == 9) expand = true;
                }
            }
            getVOLinfo(this.volL, chartList, index2TimePos, 8);
            getVOLinfo(this.volR, chartList, index2TimePos, 9);
            for (int i = 1; i < volL.Count; i++)
                volL[i].Item1.fixSlam(volL[i - 1].Item1);
            for (int i = 1; i < volR.Count; i++)
                volR[i].Item1.fixSlam(volR[i - 1].Item1);

            /****************************************
                              beat
            ****************************************/
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 5)
                    continue;
                if (chartList[i].Substring(0, 5) == "beat=")
                {
                    if (beat.Count == 0)
                        beat.Add(new Tuple<TimePos, Tuple<int, int>>(new TimePos("001,01,00"),
                        new Tuple<int, int>((int)(chartList[i][5] - '0'), (int)(chartList[i][7] - '0'))));
                    else
                        beat.Add(new Tuple<TimePos, Tuple<int, int>>(index2TimePos[i],
                        new Tuple<int, int>((int)(chartList[i][5] - '0'), (int)(chartList[i][7] - '0'))));
                }
            }

            /****************************************
                               bpm
            ****************************************/
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Substring(0, 2) == "t=")
                {
                    if (bpm.Count == 0)
                        bpm.Add(new Tuple<TimePos, double>(new TimePos("001,01,00"),
                            Convert.ToDouble(chartList[i].Substring(2))));
                    else
                        bpm.Add(new Tuple<TimePos, double>(index2TimePos[i],
                            Convert.ToDouble(chartList[i].Substring(2))));
                }
            }
            if (bpm.Count == 0) bpm.Add(new Tuple<TimePos, double>(new TimePos("001,01,00"),
                            Convert.ToDouble(initBpm)));

            /****************************************
                             endPos
            ****************************************/
            endPos = new TimePos(barCount + 1, 1, 0);
        }

        // Output to .vox
        public MemoryStream ToVox()
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);

            writer.Write(@"//====================================
// SOUND VOLTEX OUTPUT TEXT FILE
//====================================

#FORMAT VERSION
8
#END

#BEAT INFO
");
            foreach (Tuple<TimePos, Tuple<int, int>> b in beat)
            {
                writer.Write(b.Item1);
                writer.Write('\t');
                writer.Write(b.Item2.Item1);
                writer.Write('\t');
                writer.Write(b.Item2.Item2);
                writer.Write("\r\n");
            }
            writer.Write(@"#END

#BPM INFO
");
            foreach (Tuple<TimePos, double> b in bpm)
            {
                writer.Write(b.Item1);
                writer.Write('\t');
                writer.Write(b.Item2);
                writer.Write('\t');
                writer.Write("4");
                writer.Write("\r\n");
            }
            writer.Write(@"#END

#TILT MODE INFO
001,01,00	0
#END

#LYRIC INFO
#END

#END POSITION
");
            writer.Write(endPos);
            writer.Write("\r\n");
            writer.Write(@"#END

#TAB EFFECT INFO
1,	90.00,	400.00,	18000.00,	0.70
1,	90.00,	600.00,	15000.00,	5.00
2,	90.00,	40.00,	5000.00,	0.70
2,	90.00,	40.00,	2000.00,	3.00
3,	100.00,	30
#END

#FXBUTTON EFFECT INFO
#END

#TAB PARAM ASSIGN INFO
0,	0,	0.00,	0.00
0,	0,	0.00,	0.00
1,	0,	0.00,	0.00
1,	0,	0.00,	0.00
2,	0,	0.00,	0.00
2,	0,	0.00,	0.00
3,	0,	0.00,	0.00
3,	0,	0.00,	0.00
4,	0,	0.00,	0.00
4,	0,	0.00,	0.00
5,	0,	0.00,	0.00
5,	0,	0.00,	0.00
6,	0,	0.00,	0.00
6,	0,	0.00,	0.00
7,	0,	0.00,	0.00
7,	0,	0.00,	0.00
8,	0,	0.00,	0.00
8,	0,	0.00,	0.00
9,	0,	0.00,	0.00
9,	0,	0.00,	0.00
10,	0,	0.00,	0.00
10,	0,	0.00,	0.00
11,	0,	0.00,	0.00
11,	0,	0.00,	0.00
#END

#REVERB EFFECT PARAM
#END

//====================================
// TRACK INFO
//====================================

#TRACK1
");
            WriteTrack<Vol>(writer, volL);
            writer.Write(@"#END

//====================================

#TRACK2
");
            WriteTrack<Fx>(writer, fxL);
            writer.Write(@"#END

//====================================

#TRACK3
");
            WriteTrack<Bt>(writer, btA);
            writer.Write(@"#END

//====================================

#TRACK4
");
            WriteTrack<Bt>(writer, btB);
            writer.Write(@"#END

//====================================

#TRACK5
");
            WriteTrack<Bt>(writer, btC);
            writer.Write(@"#END

//====================================

#TRACK6
");
            WriteTrack<Bt>(writer, btD);
            writer.Write(@"#END

//====================================

#TRACK7
");
            WriteTrack<Fx>(writer, fxR);
            writer.Write(@"#END

//====================================

#TRACK8
");
            WriteTrack<Vol>(writer, volR);
            writer.Write(@"#END

//====================================

//====================================
// SPCONTROLER INFO
//====================================

#SPCONTROLER
001,01,00	AIRL_ScaX	1	0	0.00	1.00	0.00	0.00
001,01,00	AIRR_ScaX	1	0	0.00	2.00	0.00	0.00
#END");

            writer.Flush();
			return stream;
		}
		//public Stream ToKsh() { }

        // Utils

        public bool SomethingIsInFirstMeasure()
        {
            return ((volL.First<Tuple<TimePos, Vol>>().Item1.Measure() == 1) ||
                    (fxL.First<Tuple<TimePos, Fx>>().Item1.Measure() == 1) ||
                    (btA.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btB.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btC.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btD.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (fxR.First<Tuple<TimePos, Fx>>().Item1.Measure() == 1) ||
                    (volR.First<Tuple<TimePos, Vol>>().Item1.Measure() == 1));
        }

        private bool SomethingIsInFirstMeasureUtilList<T>(List<Tuple<TimePos, T>> list)
        {
            return (list.First<Tuple<TimePos, T>>().Item1.Measure() == 1);
        }

        public double FirstMesureLength()
        {
            return  Convert.ToDouble(beat.First<Tuple<TimePos, Tuple<int, int>>>().Item2.Item1) /
                    (Convert.ToDouble(beat.First<Tuple<TimePos, Tuple<int, int>>>().Item2.Item2) / 4.0) *
                    60.0 / (bpm.First<Tuple<TimePos, double>>().Item2);
        }

        private static void GoToTag(StreamReader sr, string tag)
        {
            while (sr.Peek() > 0)
            {
                string line = sr.ReadLine();
                if (line.Contains(tag))
                   return;
            }
        }

        private static void WriteTrack<T>(StreamWriter sw, List<Tuple<TimePos, T>> data)
        {
            foreach (Tuple<TimePos, T> b in data)
            {
                sw.Write(b.Item1);
                sw.Write('\t');
                sw.Write(b.Item2);
                sw.Write("\r\n");
            }
        }

        private List<Tuple<TimePos, Tuple<int, int>>>	beat;
		private List<Tuple<TimePos, double>>			bpm;

		private TimePos endPos;

		private List<FxEffect> fxList;

		private List<Tuple<TimePos, Vol>>	volL;
		private List<Tuple<TimePos, Fx>>	fxL;
		private List<Tuple<TimePos, Bt>>	btA;
		private List<Tuple<TimePos, Bt>>	btB;
		private List<Tuple<TimePos, Bt>>	btC;
		private List<Tuple<TimePos, Bt>>	btD;
		private List<Tuple<TimePos, Fx>>	fxR;
		private List<Tuple<TimePos, Vol>>	volR;

		private List<Tuple<TimePos, Sp>>	sp;

        private bool shift;

	}
}
