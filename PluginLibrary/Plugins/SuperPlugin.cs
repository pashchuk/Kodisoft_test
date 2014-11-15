using System;

namespace PluginLibrary.Plugins
{
	public class SuperPlugin:Plugin<int>
	{
		public SuperPlugin() { }
		public Plugin<int> helpPlugin { get; set; }
		public override int Modify(int param)
		{
			if (helpPlugin==null) throw new ArgumentNullException("Help plugin is null");
			return helpPlugin.Modify(param)*2;
		}
	}
}
