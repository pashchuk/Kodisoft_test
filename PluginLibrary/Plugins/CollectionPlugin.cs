﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginLibrary.Plugins
{
	public class CollectionPlugin:Plugin<IEnumerable<object>>
	{
		public CollectionPlugin() { }
		public IEnumerable<IPlugin> Plugins { get; set; } 
		public override IEnumerable<object> Modify(IEnumerable<object> param)
		{
			if (Plugins == null || !Plugins.Any()) throw new ArgumentNullException("Plugins is null");
			if (param == null || !param.Any() || param.Count() != Plugins.Count())
				throw new ArgumentException("param is invalid");
			List<object> result = new List<object>();
			var paramEnumerator = param.GetEnumerator();
			foreach (var plugin in Plugins)
			{
				if (!paramEnumerator.MoveNext()) throw new IndexOutOfRangeException();
				result.Add(plugin.Modify(paramEnumerator.Current));
			}
			return result;
		}
	}
}
