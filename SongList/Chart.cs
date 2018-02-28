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
			private int		pos;
			private bool	flag;
			private bool	flip;
			private int		filter;
			private bool	expand;		
		}
		class Fx
		{
			private int	length;
			private FxEffect effect;
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
