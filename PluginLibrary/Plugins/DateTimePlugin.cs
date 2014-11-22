using System;

namespace PluginLibrary.Plugins
{
	public class DateTimePlugin : Plugin<DateTime>
	{
		private int timeZone;

		private DateTimePlugin() { }
		public DateTimePlugin(int targetTimeZone)
		{
			if (targetTimeZone < -12 && targetTimeZone > 12)
				throw new ArgumentException("Time zone must be not more than +12 and not less than -12");
			this.timeZone = targetTimeZone;
		}
		public override DateTime Modify(DateTime param)
		{
			int diff = timeZone - (param.Hour - param.ToUniversalTime().Hour);
			param = param.AddHours(diff);
			return param;
		}
	}
}
