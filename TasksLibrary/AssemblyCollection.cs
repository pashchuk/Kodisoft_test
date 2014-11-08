using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace TasksLibrary
{
	public class AssemblyCollection
	{
		private readonly object _lockObj = new object();
		private Dictionary<Type, ConstructorInfo> cache;

		public AssemblyCollection()
		{
			cache = new Dictionary<Type, ConstructorInfo>();
			Parallel.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
			{
				foreach (var type in assembly.GetTypes())
				{
					var ctor = type.GetConstructor(Type.EmptyTypes);
					if (ctor != null)
						cache.Add(type, ctor);
				}
			});
			AppDomain.CurrentDomain.AssemblyLoad += (s, args) =>
			{
				foreach (var type in args.LoadedAssembly.GetTypes())
				{
					var ctor = type.GetConstructor(Type.EmptyTypes);
					if (ctor != null)
						cache.Add(type, ctor);
				}
			};
		}
		public T Create<T>()
		{
			ConstructorInfo ctor;
			if (!cache.TryGetValue(typeof (T), out ctor))
				throw new NullReferenceException(string.Format("Type {0} is not exist in cached types", typeof (T)));
			lock (_lockObj)
			{
				return (T)ctor.Invoke(null);	
			}
		}
	}
}
