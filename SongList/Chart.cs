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
		}

		// From .vox
		public Chart(Stream s)
		{
			// Parse .vox into Chart object
		}

		// From .ksh
		public Chart(List<string> s)
		{
			// Parse .ksh into Chart object
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
