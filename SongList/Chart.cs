using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SongList
{
	public class Chart
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
            public void Print()
            {
                Console.WriteLine("({0}, {1}, {2})", this.pos, this.flag, this.expand);
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
        }

		// From .vox
		public Chart(Stream s)
		{
			// Parse .vox into Chart object
		}

		// From .ksh
		public Chart(List<string> chartList)
		{
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
                index2TimePos.Add(i, new TimePos(index2barNbr[i], beatTotal/48 + 1, beatTotal%48));
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
            Dictionary<char, int> Pos1 =
                new Dictionary<char, int>()
                {
                    {'0', 0}, {'5', 12}, {'A', 25}, {'F', 38}, {'K', 50},
                    {'P', 63}, {'U', 76}, {'Z', 88}, {'e', 101}, {'j', 114}, {'o', 127}
                };
            Dictionary<char, int> Pos2 =
                new Dictionary<char, int>()
                {
                    {'0', 0}, {'2', 5}, {'5', 12}, {'7', 17}, {'A', 25},
                    {'C', 30}, {'F', 38}, {'H', 43}, {'K', 50}, {'M', 55},
                    {'P', 63}, {'S', 71}, {'U', 76}, {'X', 83}, {'Z', 88},
                    {'b', 93}, {'e', 101}, {'h', 109}, {'j', 114}, {'m', 121}, {'o', 127}
                };
            void getVOLinfo(List<Tuple<TimePos, Vol>> vol, List<string> cL, Dictionary<int, TimePos> i2T,
                           Dictionary<char, int> p1, Dictionary<char, int> p2, int VOLtype)
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
                        else if (p2.ContainsKey(cL[i][VOLtype]))
                        {
                            if (expand)
                            {
                                vol.Add(new Tuple<TimePos, Vol>(index2TimePos[i],
                                        new Vol(p2[cL[i][VOLtype]], 0, 2)));
                            }
                            else
                            {
                                vol.Add(new Tuple<TimePos, Vol>(index2TimePos[i],
                                        new Vol(p1[cL[i][VOLtype]], 0, 1)));
                            }
                            if (!inLine)
                            {
                                inLine = true;
                                vol[vol.Count - 1].Item2.setFlag(1);
                            }
                        }
                    }
                    else if (cL[i] == "laserrange_l=2x" && VOLtype == 8)
                        expand = true;
                    else if (cL[i] == "laserrange_r=2x" && VOLtype == 9)
                        expand = true;
                }
            }
            getVOLinfo(this.volL, chartList, index2TimePos, Pos1, Pos2, 8);
            getVOLinfo(this.volR, chartList, index2TimePos, Pos1, Pos2, 9);
            for (int i = 1; i < volL.Count; i++)
            {
                volL[i].Item1.fixSlam(volL[i - 1].Item1);
            }
            for (int i = 1; i < volR.Count; i++)
            {
                volR[i].Item1.fixSlam(volR[i - 1].Item1);
            }

            /****************************************
                              beat
            ****************************************/
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 5)
                    continue;
                if (chartList[i].Substring(0, 5) == "beat=")
                    beat.Add(new Tuple<TimePos, Tuple<int, int>>(index2TimePos[i],
                        new Tuple<int, int>((int)(chartList[i][5] - '0'), (int)(chartList[i][7] - '0'))));
            }

            /****************************************
                               bpm
            ****************************************/
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Substring(0, 2) == "t=")
                    bpm.Add(new Tuple<TimePos, double>(index2TimePos[i], 
                            Convert.ToDouble(chartList[i].Substring(2))));
            }
            
            /****************************************
                             endPos
            ****************************************/
            endPos = new TimePos(barCount + 2, 1, 0);
        }

        // Output to .vox
        public Stream ToVox()
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);

			return stream;
		}
		//public Stream ToKsh() { }

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

	}
}
