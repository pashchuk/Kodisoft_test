using System;

namespace PluginLibrary
{
	public abstract class Plugin<T> : IPlugin
	{
		public abstract T Modify(T param);

		object IPlugin.Modify(object param)
		{
			return Modify((T) param);
		}

	}
}
