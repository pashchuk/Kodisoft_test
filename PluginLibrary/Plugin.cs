using System;

namespace PluginLibrary
{
	public abstract class Plugin<T> : IPlugin
	{
		public abstract T Modify(T param);

		object ICloneable.Clone()
		{
			return Activator.CreateInstance(this.GetType());
		}
		object IPlugin.Modify(object param)
		{
			return Modify((T) param);
		}

	}
}
