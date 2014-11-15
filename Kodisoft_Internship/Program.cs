﻿using PluginLibrary;
using PluginLibrary.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TasksLibrary;

namespace Kodisoft_Internship
{
	class Program
	{
		static void Main(string[] args)
		{
			var watch = new Stopwatch();
			#region Plugins test
			//create collection with different plugins
			List<IPlugin> plugins = new List<IPlugin>()
			{
				new StringPlugin(),
				new DateTimePlugin(0),
				new IntPlugin(10)
			};

			//crete collection plugin and apply created plugins
			CollectionPlugin plg = new CollectionPlugin() { Plugins = plugins };
			var result = plg.Modify(new List<object>() { "asdfg", DateTime.Now, 25 });
			foreach (var a in result)
				Console.WriteLine(a.ToString());

			//init simple custom plugins based on PluginBase<T> class
			//for work with plugins
			UsePluginBaseClass simpleUsePluginClass = new UsePluginBaseClass()
			{
				Data = 37,
				Plugin = new IntPlugin(3)
			};
			UsePluginBaseClass difficultUsePluginClass = new UsePluginBaseClass()
			{
				Data = 37,
				Plugin = new SuperPlugin() { helpPlugin = new IntPlugin(3) }
			};
			AnotherUsePluginBaseClass verydifficultUsePluginClass = new AnotherUsePluginBaseClass()
			{
				Plugin = new CollectionPlugin() {Plugins = plugins},
				Data = new List<object> {"asdfg", DateTime.Now, 25}
			};
			simpleUsePluginClass.Print();
			difficultUsePluginClass.Print();
			verydifficultUsePluginClass.Print();
			CustomMutex mutex = new CustomMutex();
			Task[] tasks = new Task[10];
			for (int i = 0; i < 10; i++)
			{
				tasks[i] = Task.Run(async () =>
				{
					using (await mutex.LockSection())
					{
						Console.WriteLine("Thread : {0:D}", Thread.CurrentThread.ManagedThreadId);
						for (int j = 0; j < 10; j++)
							Console.Write("{0:D} ", j);
						Console.WriteLine();
					}
				});
			}
			#endregion
			Task.WaitAll(tasks);
			watch.Start();
			AssemblyCollection collection = new AssemblyCollection();
			watch.Stop();
			Console.WriteLine(watch.Elapsed);
			try
			{
				CustomMutex mut = collection.Create<CustomMutex>();
				Random generator = collection.Create<Random>();
				Exception ex = collection.Create<Exception>();
			}
			catch (NullReferenceException ex) {
				Console.WriteLine(ex.Message);
			}
			Console.ReadLine();
		}
	}

	public class Test
	{
		private int asd;
		public RandomGenerator generator;
		public int Asd { get; set; }
	}
	public class UsePluginBaseClass : PluginBase<Plugin<int>, int>
	{
		public override void Print()
		{
			base.Print();
		}
	}

	public class AnotherUsePluginBaseClass : PluginBase<CollectionPlugin, IEnumerable<object>>
	{
		public override void Print()
		{
			Console.WriteLine("Collection Plugin started");
			Console.WriteLine("Before applying plugin:);");
			int count = 0;
			foreach (var plugin in Plugin.Plugins)
				Console.WriteLine("Plugin name = {0} : data = {1}", plugin.GetType().Name, Data.ElementAt(count++));
			count = 0;
			Data = Plugin.Modify(Data);
			Console.WriteLine("After applying plugin:);");
			foreach (var plugin in Plugin.Plugins)
				Console.WriteLine("Plugin name = {0} : data = {1}", plugin.GetType().Name, Data.ElementAt(count++));
			Console.WriteLine("Collection plugin complete");
		}
	}
}
