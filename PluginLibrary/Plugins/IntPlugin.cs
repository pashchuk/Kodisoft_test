using System;

namespace PluginLibrary.Plugins
{
	public class IntPlugin:Plugin<int>
	{
		private int multiplier;

		public IntPlugin() { }

		public IntPlugin(int multiplier)
		{
			this.multiplier = multiplier;
		}
		public override int Modify(int param)
		{
			return param*multiplier;
		}
	}
}
