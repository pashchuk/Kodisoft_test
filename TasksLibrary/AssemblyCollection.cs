using System;
using System.Collections.Generic;
using System.Reflection;
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
			//get the default constructors and types in all assemblies 
			//in AppDomain using multiply threads
			Parallel.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
			{
				foreach (var type in assembly.GetTypes())
				{
					//get ctor and save it in dictionary
					var ctor = type.GetConstructor(Type.EmptyTypes);
					if (ctor != null)
						cache.Add(type, ctor);
				}
			});
			//add event which will scan every downloaded assembly
			AppDomain.CurrentDomain.AssemblyLoad += (s, args) =>
			{
				foreach (var type in args.LoadedAssembly.GetTypes())
				{
					var ctor = type.GetConstructor(Type.EmptyTypes);
					if (ctor != null)
						lock (_lockObj)
						{
							cache.Add(type, ctor);
						}
				}
			};
		}

		//create a object of type T using cached ctor
		public T Create<T>()
		{
			ConstructorInfo ctor;
			lock (_lockObj)
			{
				if (!cache.TryGetValue(typeof (T), out ctor))
					throw new NullReferenceException(string.Format("Type {0} is not exist in cached types", typeof (T)));
				return (T)ctor.Invoke(null);	
			}
		}
	}
}
