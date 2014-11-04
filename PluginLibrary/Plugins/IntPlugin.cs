using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLibrary.Plugins
{
	public class IntPlugin:Plugin<int>, ICloneable
	{
		private int multiplier;

		public IntPlugin(int multiplier)
		{
			this.multiplier = multiplier;
		}
		public override int Modify(int param)
		{
			return param*multiplier;
		}

		public object Clone()
		{
			return new IntPlugin(this.multiplier);
		}
	}
}
