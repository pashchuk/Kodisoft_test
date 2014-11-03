using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginLibrary;
using PluginLibrary.Plugins;

namespace Kodisoft_Internship
{
	class Program
	{
		static void Main(string[] args)
		{
			List<IPlugin> plugins = new List<IPlugin>()
			{
				new StringPlugin(),
				new DateTimePlugin(0),
				new IntPlugin(10)
			};
			CollectionPlugin plg = new CollectionPlugin() {Plugins = plugins};
			var result = plg.Modify(new List<object>() {"asdfg", DateTime.Now, 25});
			foreach (var a in result)
			{
				Console.WriteLine(a.ToString());
			}
			UsePluginBaseClass simpleUsePluginClass = new UsePluginBaseClass()
			{
				Data = 37,
				Plugin = new IntPlugin(3)
			};
			UsePluginBaseClass difficultUsePluginClass = new UsePluginBaseClass()
			{
				Data = 37,
				Plugin = new SuperPlugin() {helpPlugin = new IntPlugin(3)}
			};
			AnotherUsePluginBaseClass verydifficultUsePluginClass = new AnotherUsePluginBaseClass()
			{
				Data = 37,
				Plugin = new SuperPlugin() {helpPlugin = new IntPlugin(3)}
			};
			simpleUsePluginClass.Print();
			difficultUsePluginClass.Print();
			verydifficultUsePluginClass.Print();
			Console.ReadLine();
		}
	}

	public class UsePluginBaseClass : PluginBase<Plugin<int>, int>
	{
		public override void Print()
		{
			Console.WriteLine("Before applying plugin: IntPlugin");
			Console.WriteLine("{0:D}",base.Data);
			Data = Plugin.Modify(base.Data);
			Console.WriteLine("After applying plugin: IntPlugin");
			Console.WriteLine("{0:D}", base.Data);
		}
	}

	public class AnotherUsePluginBaseClass : PluginBase<Plugin<int>, int>
	{
		public override void Print()
		{
			base.Print();
		}
	}
}
