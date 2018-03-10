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
            // for Vol, false: no slam true: do slam
            public bool fixSlam(TimePos prev)
            {
                if (this.measure == prev.measure && this.beat == prev.beat
                                                 && this.time == prev.time + 6)
                {
                    this.time = prev.time;
                    return true;
                }
                return false;
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
            public enum FxType
            {
                Retrigger,
                Gate,
                Flanger,
                PitchShift,
                BitCrusher,
                Phaser,
                Wobble,
                TapeStop,
                Echo,
                SideChain,
                _effect8,
                None
            }
            static private Dictionary<string, List<double>> defaultAttributes = new Dictionary<string, List<double>>()
            {
                { "Retrigger;8",  new List<double>{ 1, 4, 95.00, 2.00, 1.00, 0.85, 0.15 } },
                { "Retrigger;12", new List<double>{ 1, 6, 95.00, 2.00, 1.00, 0.85, 0.15 } },
                { "Retrigger;16", new List<double>{ 1, 8, 95.00, 2.00, 1.00, 0.75, 0.1 } },
                { "Retrigger;24", new List<double>{ 1, 12, 95.00, 2.00, 1.00, 0.80, 0.1 } },
                { "Retrigger;32", new List<double>{ 1, 16, 95.00, 2.00, 1.00, 0.87, 0.13 } },
                { "Gate;4",       new List<double>{ 2, 98.00, 2, 1.00 } },
                { "Gate;8",       new List<double>{ 2, 98.00, 4, 1.00 } },
                { "Gate;12",      new List<double>{ 2, 98.00, 6, 1.00 } },
                { "Gate;16",      new List<double>{ 2, 98.00, 8, 1.00 } },
                { "Gate;24",      new List<double>{ 2, 98.00, 12, 1.00 } },
                { "Gate;32",      new List<double>{ 2, 98.00, 16, 1.00 } },
                { "Flanger",      new List<double>{ 3, 75.00, 2.00, 0.50, 90, 2.00 } },
                { "PitchShift;12",new List<double>{ 9, 72.00, 12.00 } },
                { "BitCrusher;10",new List<double>{ 7, 100.00, 12 } },
                { "Phaser",       new List<double>{0} },
                { "Wobble;12",    new List<double>{ 6, 0, 3, 80.00, 500.00, 18000.00, 4.00, 1.40 } },
                { "TapeStop;50",  new List<double>{ 4, 100.00, 8.00, 0.40 } },
                { "Echo;4;60",    new List<double>{ 1, 4, 100.00, 4.00, 0.60, 1.00, 0.85 } },
                { "SideChain",    new List<double>{ 5, 90.00, 1.00, 45, 50, 60 } },
                { "None",         new List<double>{0} },
            };
            public FxEffect(FxType type, string strType)
            {
                this.type = type;
                if (defaultAttributes.ContainsKey(strType))
                    attributes = defaultAttributes[strType];
                else
                    attributes = defaultAttributes["None"];
                    //throw new Exception("invalid FxType");
            }
            public bool isNone()
            {
                return this.type == FxType.None;
            }
            public bool isPhaser()
            {
                return this.type == FxType.Phaser;
            }
            // override equals
            public override bool Equals(object obj)
            {
                return obj is FxEffect && this == (FxEffect)obj;
            }
            public override int GetHashCode()
            {
                double tmp = 0;
                foreach (var att in attributes)
                    tmp += att;
                return (int)tmp;
            }
            public static bool operator == (FxEffect x, FxEffect y)
            {
                return x.attributes.SequenceEqual(y.attributes);
            }
            public static bool operator !=(FxEffect x, FxEffect y)
            {
                return !x.attributes.SequenceEqual(y.attributes);
            }
            public void Print()
            {
                Console.WriteLine(this.type);
            }
            //Overwrite
            public override string ToString()
            {
                string tmp = "";
                for (int i = 0; i<attributes.Count-1; i++)
                    tmp = tmp + attributes[i].ToString() + ",\t";
                return tmp + attributes[attributes.Count-1].ToString();
            }
            FxType type;
            private List<double> attributes;
		}
		class Sp
		{
            string type;
            private int length;
            private double beginAttribute;
            private double endAttribute;
            public const double empty = 100;
            public Sp(string type, int length, double beginAttribute, double endAttribute = empty)
            {
                this.type = type;
                this.length = length;
                this.beginAttribute = beginAttribute;
                this.endAttribute = (endAttribute==empty)? beginAttribute: endAttribute;
            }
            public void addLength(int length)
            {
                this.length += length;
            }
            public void setEndAttribute(double endAttribute)
            {
                this.endAttribute = endAttribute;
            }
            //Overwrite
            public override string ToString()
            {
                return type + "\t2\t" +
                        length.ToString() + "\t" +
                        beginAttribute.ToString() + "\t" +
                        endAttribute.ToString() + "\t0.00\t0.00";
            }
        }

		class Vol
		{
			private int	pos;
			private int	flag;
			private int	flip;
			private int	filter;
			private int	expand;
            public Vol(int pos, int flag, int filter, int expand)
            {
                this.pos = pos;
                this.flag = flag;
                this.flip = 0;
                this.filter = filter;
                this.expand = expand;
            }
            public void setFlag(int flag)
            {
                this.flag = flag;
            }
            public void setExpand(int expand)
            {
                this.expand = expand;
            }
            public void setFlip(int flip)
            {
                this.flip = flip;
            }
            public int getPos()
            {
                return this.pos;
            }
            public int getFlag()
            {
                return this.flag;
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
            public Fx(int length, FxEffect effect)
            {
                this.length = length;
                this.effect = effect;
            }
            public void addLength(int length)
            {
                this.length += length;
            }
            public FxEffect getFxEffect()
            {
                return this.effect;
            }
            public void Print()
            {
                Console.WriteLine(this.length);
                this.effect.Print();
            }
            //Overwrite
            public override string ToString()
            {
                return length.ToString() + "\t" ;
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
                                    int.Parse(tokens[4]),
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
                if (chartList[i].Length < 2)
                    continue;
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
                if (chartList[i].Length < 2)
                    continue;
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
                if (chartList[i].Length < 2)
                    continue;
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
                    if (chartList[i].Length < 2)
                        continue;
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
                FxEffect effect = new FxEffect(FxEffect.FxType.None, "None");
                for (int i = 0; i < cL.Count; i++)
                {
                    if (chartList[i].Length < 2)
                        continue;
                    if (char.IsNumber(cL[i][0]))
                    {
                        if (cL[i][FXtype] == '2')
                        {
                            isLong = false;
                            effect = new FxEffect(FxEffect.FxType.None, "None");
                            fx.Add(new Tuple<TimePos, Fx>(index2TimePos[i], new Fx(0, effect)));
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
                                fx.Add(new Tuple<TimePos, Fx>(index2TimePos[i], new Fx(b2bU[i2bN[i]], effect)));
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
                    {
                        effect = parseEffect(cL[i].Substring(5));
                        toFxList(ref effect);
                    }
                    else if (cL[i].Substring(0, 5) == "fx-r=" && FXtype == 6)
                    {
                        effect = parseEffect(cL[i].Substring(5));
                        toFxList(ref effect);
                    }
                }
            }
            FxEffect parseEffect(string eff)
            {
                if (eff.Length == 0) return new FxEffect(FxEffect.FxType.None, "None");
                else if (eff[0] == 'R') return new FxEffect(FxEffect.FxType.Retrigger, eff);
                else if (eff[0] == 'G') return new FxEffect(FxEffect.FxType.Gate, eff);
                else if (eff[0] == 'F') return new FxEffect(FxEffect.FxType.Flanger, eff);
                else if (eff[0] == 'B') return new FxEffect(FxEffect.FxType.BitCrusher, eff);
                else if (eff[0] == 'W') return new FxEffect(FxEffect.FxType.Wobble, eff);
                else if (eff[0] == 'T') return new FxEffect(FxEffect.FxType.TapeStop, eff);
                else if (eff[0] == 'E') return new FxEffect(FxEffect.FxType.Echo, eff);
                else if (eff[0] == 'S') return new FxEffect(FxEffect.FxType.SideChain, eff);
                else if (eff[1] == 'i') return new FxEffect(FxEffect.FxType.PitchShift, eff);
                else if (eff[1] == 'h') return new FxEffect(FxEffect.FxType.Phaser, eff);
                else return new FxEffect(FxEffect.FxType.None, "None");
            }
            void toFxList(ref FxEffect fe)
            {
                if (fe.isNone()) return;
                // change "Phaser" effect's priority
                if (fe.isPhaser()) return;
                if (this.fxList.Count == 12)
                {
                    // FXeffect type over 12
                    if (!this.fxList.Contains(fe))
                    {
                        fe = new FxEffect(FxEffect.FxType.None, "None");
                        // should throw some exception
                        Console.WriteLine("too many type of fxEffect");
                        return;
                    }
                }
                else if (!this.fxList.Contains(fe))
                    this.fxList.Add(fe);
            }
            getFXinfo(this.fxL, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 5);
            getFXinfo(this.fxR, chartList, index2TimePos, index2barNbr, barNbr2beatUnit, 6);

            /****************************************
                              VOL
            ****************************************/
            Dictionary<string, Tuple<bool, int>> TimePos2Flip = new Dictionary<string, Tuple<bool, int>>();
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
                int flt = 0;
                for (int i = 0; i < cL.Count; i++)
                {
                    if (chartList[i].Length < 2)
                        continue;
                    if (char.IsNumber(cL[i][0]))
                    {
                        // for flip, true: left false: right
                        if (cL[i].Length > 11)
                        {
                            if (cL[i][11] == '>')
                                TimePos2Flip[index2TimePos[i].ToString()] = new Tuple<bool, int>(false, 5);
                            else if (cL[i][11] == '<')
                                TimePos2Flip[index2TimePos[i].ToString()] = new Tuple<bool, int>(true, 5);
                            else
                            {
                                int flipNbr = Convert.ToInt32(cL[i].Substring(12));
                                if (flipNbr <= 96) flipNbr = 2;
                                else if (flipNbr >= 192) flipNbr = 1;
                                else flipNbr = 3;

                                if (cL[i][11] == ')')
                                    TimePos2Flip[index2TimePos[i].ToString()] = new Tuple<bool, int>(false, flipNbr);
                                else if (cL[i][11] == '(')
                                    TimePos2Flip[index2TimePos[i].ToString()] = new Tuple<bool, int>(true, flipNbr);
                            }
                        }
                        // else
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
                                        new Vol(Pos[cL[i][VOLtype]], 0, flt, 1)));
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
                    else if (cL[i] == "filtertype=hpf1") flt = 4;
                    else if (cL[i] == "filtertype=lpf1") flt = 2;
                    else if (cL[i] == "filtertype=bitc") flt = 5;
                    else if (cL[i] == "filtertype=peak") flt = 0;
                }
            }
            getVOLinfo(this.volL, chartList, index2TimePos, 8);
            getVOLinfo(this.volR, chartList, index2TimePos, 9);
            // fix slam and set flip
            void fixSlamAndSetFlip(List<Tuple<TimePos, Vol>> vo)
            {
                for (int i = 1; i < vo.Count; i++)
                    if (vo[i].Item1.fixSlam(vo[i - 1].Item1))
                    {
                        if (TimePos2Flip.ContainsKey(vo[i - 1].Item1.ToString()))
                        {
                            if (vo[i].Item2.getPos() > vo[i - 1].Item2.getPos() &&
                                !TimePos2Flip[vo[i - 1].Item1.ToString()].Item1)
                            {
                                vo[i - 1].Item2.setFlip(TimePos2Flip[vo[i - 1].Item1.ToString()].Item2);
                            }
                            else if (vo[i].Item2.getPos() < vo[i - 1].Item2.getPos() &&
                                TimePos2Flip[vo[i - 1].Item1.ToString()].Item1)
                            {
                                vo[i - 1].Item2.setFlip(TimePos2Flip[vo[i - 1].Item1.ToString()].Item2);
                            }
                        }
                    }
            }
            fixSlamAndSetFlip(volL);
            fixSlamAndSetFlip(volR);

            void fixInfiniteLaser(List<Tuple<TimePos, Vol>> vo)
            {
                int point = 2;
                while(point < vo.Count)
                {
                    if (vo[point].Item2.getPos() == vo[point - 1].Item2.getPos() && 
                        vo[point - 1].Item2.getPos() == vo[point - 2].Item2.getPos())
                    {
                        if (vo[point - 1].Item2.getFlag() != 2 && vo[point - 2].Item2.getFlag() != 2)
                        {
                            vo.RemoveAt(point-1);
                            continue;
                        }
                    }
                    point++;
                }
            }
            fixInfiniteLaser(volL);
            fixInfiniteLaser(volR);

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
            if (beat.Count == 0) beat.Add(new Tuple<TimePos, Tuple<int, int>>(new TimePos("001,01,00"),
                             new Tuple<int, int>(4, 4)));

            /****************************************
                               bpm
            ****************************************/
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 2)
                    continue;
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
            endPos = new TimePos(barCount + 2, 1, 0);

            /****************************************
                               Sp
            ****************************************/
            // "CAM_RotX"
            double CamX = 0;
            double effectRatio = 0.0050;
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 9) continue;
                if (chartList[i].Substring(0, 9) == "zoom_top=")
                {
                    CamX = Math.Round(Convert.ToDouble(chartList[i].Substring(9)) * (effectRatio), 2);
                    break;
                }
            }
            sp.Add(new Tuple<TimePos, Sp>(new TimePos("001,01,00"), 
                                          new Sp("CAM_RotX", 0, CamX)));
            if (shift) sp[sp.Count - 1].Item2.addLength(beat[0].Item2.Item1 * 48);
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 2)
                    continue;
                if (char.IsNumber(chartList[i][0]))
                    sp[sp.Count - 1].Item2.addLength(barNbr2beatUnit[index2barNbr[i]]);
                else if (chartList[i].Length < 9) continue;
                else if (chartList[i].Substring(0, 9) == "zoom_top=")
                {
                    CamX = Math.Round(Convert.ToDouble(chartList[i].Substring(9)) * (effectRatio), 2);
                    sp[sp.Count - 1].Item2.setEndAttribute(CamX);
                    sp.Add(new Tuple<TimePos, Sp>(index2TimePos[i],
                                                  new Sp("CAM_RotX", 0, CamX)));
                }
            }
            sp[sp.Count - 1].Item2.addLength(barNbr2beat[barCount].Item1 * 48);
            // "CAM_Radi"
            double CamR = 0;
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 12) continue;
                if (chartList[i].Substring(0, 12) == "zoom_bottom=")
                {
                    CamR = Math.Round(Convert.ToDouble(chartList[i].Substring(12)) * (-effectRatio), 2);
                    break;
                }
            }
            sp.Add(new Tuple<TimePos, Sp>(new TimePos("001,01,00"),
                                          new Sp("CAM_Radi", 0, CamR)));
            if (shift) sp[sp.Count - 1].Item2.addLength(beat[0].Item2.Item1 * 48);
            for (int i = 0; i < chartList.Count; i++)
            {
                if (chartList[i].Length < 2)
                    continue;
                if (char.IsNumber(chartList[i][0]))
                    sp[sp.Count - 1].Item2.addLength(barNbr2beatUnit[index2barNbr[i]]);
                else if (chartList[i].Length < 12) continue;
                else if (chartList[i].Substring(0, 12) == "zoom_bottom=")
                {
                    CamR = Math.Round(Convert.ToDouble(chartList[i].Substring(12)) * (-effectRatio), 2);
                    sp[sp.Count - 1].Item2.setEndAttribute(CamR);
                    sp.Add(new Tuple<TimePos, Sp>(index2TimePos[i],
                                                  new Sp("CAM_Radi", 0, CamR)));
                }
            }
            sp[sp.Count - 1].Item2.addLength(barNbr2beat[barCount].Item1 * 48);
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
");
            foreach (FxEffect fL in fxList)
            {
                writer.Write(fL);
                writer.Write("\r\n");
                writer.Write("0,	0,	0,	0,	0,	0,	0\r\n\r\n");
            }
            for (int i = fxList.Count; i < 12; i++)
                writer.Write("1, 4, 95.00, 2.00, 1.00, 0.85, 0.15\r\n0, 0, 0, 0, 0, 0, 0\r\n\r\n");
            writer.Write(@"#END

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
            WriteTrackFx(writer, fxL);
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
            WriteTrackFx(writer, fxR);
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
001,01,00	Realize	3	0	36.12	60.12	110.12	0.00
001,01,00	Realize	4	0	0.62	0.72	1.03	0.00
001,01,00	AIRL_ScaX	1	0	0.00	1.00	0.00	0.00
001,01,00	AIRR_ScaX	1	0	0.00	2.00	0.00	0.00
");
            WriteTrack<Sp>(writer, sp);
            writer.Write(@"#END

//====================================
");
            writer.Flush();
			return stream;
		}
		//public Stream ToKsh() { }

        // Utils

        public bool SomethingIsInFirstMeasure()
        {
            bool b = false;

            if (volL.Count != 0)
                if (volL[0].Item1.Measure() == 1) b = true;
            if (fxL.Count != 0)
                if (fxL[0].Item1.Measure() == 1) b = true;
            if (btA.Count != 0)
                if (btA[0].Item1.Measure() == 1) b = true;
            if (btB.Count != 0)
                if (btB[0].Item1.Measure() == 1) b = true;
            if (btC.Count != 0)
                if (btC[0].Item1.Measure() == 1) b = true;
            if (btD.Count != 0)
                if (btD[0].Item1.Measure() == 1) b = true;
            if (fxR.Count != 0)
                if (fxR[0].Item1.Measure() == 1) b = true;
            if (volR.Count != 0)
                if (volR[0].Item1.Measure() == 1) b = true;

            return b;
            /*
                    return ((volL.First<Tuple<TimePos, Vol>>().Item1.Measure() == 1)) ||
                    (fxL.First<Tuple<TimePos, Fx>>().Item1.Measure() == 1) ||
                    (btA.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btB.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btC.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (btD.First<Tuple<TimePos, Bt>>().Item1.Measure() == 1) ||
                    (fxR.First<Tuple<TimePos, Fx>>().Item1.Measure() == 1) ||
                    (volR.First<Tuple<TimePos, Vol>>().Item1.Measure() == 1));
                    */
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

        private void WriteTrackFx(StreamWriter sw, List<Tuple<TimePos, Fx>> data)
        {
            foreach (Tuple<TimePos, Fx> b in data)
            {
                sw.Write(b.Item1);
                sw.Write('\t');
                bool found = false;
                for (int i=0; i<fxList.Count; i++)
                {
                    if (b.Item2.getFxEffect() == fxList[i])
                    {
                        sw.Write(b.Item2 + (i+2).ToString());
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    sw.Write(b.Item2 + (255).ToString());
                }
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
