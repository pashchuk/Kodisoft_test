using System;

namespace PluginLibrary
{
	public interface IPlugin : ICloneable
	{
		object Modify(object param);
	}
}
