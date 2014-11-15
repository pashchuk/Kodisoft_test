using System;

namespace PluginLibrary
{
	public abstract class PluginBase<T, U> where T:Plugin<U>
	{
		public T Plugin { get; set; }
		public U Data { get; set; }

		public virtual void Print()
		{
			Console.WriteLine("Before applying plugin: {0}",Plugin.GetType().Name);
			Console.WriteLine(Data.ToString());
			Data = Plugin.Modify(Data);
			Console.WriteLine("After applying plugin: {0}", Plugin.GetType().Name);
			Console.WriteLine(Data.ToString());
		}
	}
}
