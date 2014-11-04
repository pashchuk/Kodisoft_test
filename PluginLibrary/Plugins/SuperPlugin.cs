﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLibrary.Plugins
{
	public class SuperPlugin:Plugin<int>,ICloneable
	{
		public Plugin<int> helpPlugin { get; set; }
		public override int Modify(int param)
		{
			if (helpPlugin==null) throw new ArgumentNullException("Help plugin is null");
			return helpPlugin.Modify(param)*2;
		}

		public object Clone()
		{
			return new SuperPlugin() {helpPlugin = (Plugin<int>) Activator.CreateInstance(this.helpPlugin.GetType())};
		}
	}
}