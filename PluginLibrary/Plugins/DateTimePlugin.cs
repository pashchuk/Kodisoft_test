using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLibrary.Plugins
{
	public class DateTimePlugin : Plugin<DateTime> , ICloneable
	{
		private int timeZone;

		public DateTimePlugin(int targetTimeZone)
		{
			if (targetTimeZone < -12 && targetTimeZone > 12)
				throw new ArgumentException("Time zone must be not more than +12 and not less than -12");
			this.timeZone = targetTimeZone;
		}
		public override DateTime Modify(DateTime param)
		{
			int diff = timeZone - param.Hour - param.ToUniversalTime().Hour;
			param.AddHours(diff);
			return param;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
