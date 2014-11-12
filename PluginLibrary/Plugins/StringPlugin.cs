using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLibrary.Plugins
{
	public class StringPlugin : Plugin<string>
	{
		public StringPlugin() { }
		public override string Modify(string param)
		{
			return param.ToUpper();
		}
	}
}
