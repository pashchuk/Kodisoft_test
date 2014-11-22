using System;

namespace PluginLibrary
{
	public interface IPlugin
	{
		object Modify(object param);
	}
}
