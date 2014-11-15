using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows;
using System.Runtime.Serialization;

namespace TasksLibrary
{
	public static class Extensions
	{
		public static Task CustomContinueWith(this Task task, Action action)
		{
			return task.ContinueWith(a => action(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static Task CustomContinueWith(this Task task, Action<Task> action)
		{
			return task.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext());
		}
		public static Task CustomContinueWith<TResult>(this Task<TResult> task, Action<Task<TResult>> action)
		{
			return task.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> CustomContinueWith<TResult>(this Task task, Func<TResult> func)
		{
			return await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<Task<TResult>> CustomContinueWith<TResult>(this Task task, Func<Task<TResult>> func)
		{
			return await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> CustomContinueWith<TResult>(this Task task, Func<Task, TResult> func)
		{
			return await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<Task<TResult>> CustomContinueWith<TResult>(this Task task, Func<Task, Task<TResult>> func)
		{
			return await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}
		public static T DeepClone<T>(this T obj)
		{
			if (obj == null) return default(T);
			var type = obj.GetType();
			if (type.IsValueType || type.IsEnum)
				return obj;
			if (type == typeof(string))
			{
				return (T) Convert.ChangeType(new String((obj.ToString()).ToCharArray()), type);
			}
			var ctor = type.GetConstructor(Type.EmptyTypes);
			var result = ctor != null ? ctor.Invoke(null) : FormatterServices.GetUninitializedObject(type);
			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.FieldType.IsValueType || field.FieldType.IsEnum || field.FieldType == typeof (string))
					field.SetValue(result, field.GetValue(obj));
				else
				{
					if (field.FieldType.IsArray)
						field.SetValue(result, arrayClone(field.FieldType.GetElementType(), ((Array) field.GetValue(obj))));
					else
						field.SetValue(result, field.GetValue(obj).DeepClone<object>());
				}
			}
			return (T)result;
		}

		private static object arrayClone(Type elemType,Array source)
		{
			var result = Array.CreateInstance(elemType, source.Length);
			for (int i = 0; i < result.Length; i++)
			{
				var value = source.GetValue(i);
				if (value == null)
				{
					result.SetValue(null, i);
					continue;
				}
				result.SetValue(
					value.GetType().IsValueType || value.GetType().IsEnum || value is string
						? value
						: value.GetType().IsArray ? arrayClone(value.GetType().GetElementType(), (Array) value) : value.DeepClone(), i);
			}
			return result;
		}
	}

	public static class Cloner
	{
		private static Dictionary<Type,Delegate> _cache = new Dictionary<Type, Delegate>();
		public static T ILClone<T>(this T obj)
		{
			Delegate myExec = null;
			Type objectType = typeof (T);
			if (!_cache.TryGetValue(objectType, out myExec))
			{
				DynamicMethod method = new DynamicMethod("DoClone", objectType, new Type[] {objectType}, true);
				ILGenerator generator = method.GetILGenerator();
				if (objectType == typeof (string))
				{
					return (T) Convert.ChangeType(new String((obj.ToString()).ToCharArray()), objectType);
				}
				if (objectType.IsValueType || objectType.IsEnum)
				{
					return obj;
				}
				generator.DeclareLocal(objectType);
				ConstructorInfo constructor = objectType.GetConstructor(new Type[] {});
				if (constructor == null)
					throw new NullReferenceException(string.Format("The {0} type doesn't have a default ctor", objectType));
				generator.Emit(OpCodes.Newobj, constructor);
				generator.Emit(OpCodes.Stloc_0);
				foreach (var field in objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (field.FieldType.IsValueType || field.FieldType == typeof (string))
						CopyValueType(generator, field);
					if (field.FieldType.IsClass)
						CopyRefType(generator, field);
				}
				generator.Emit(OpCodes.Ldloc_0);
				generator.Emit(OpCodes.Ret);
				myExec = method.CreateDelegate(typeof (Func<T, T>));
				_cache.Add(objectType, myExec);
			}
			return ((Func<T, T>) myExec)(obj);
		}

		private static void CopyValueType(ILGenerator generator,FieldInfo field)
		{
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Stfld, field);
		}

		private static void CopyRefType(ILGenerator generator, FieldInfo field)
		{
			if (field.FieldType.GetInterface("IEnumerable") != null)
			{
				if (field.FieldType.IsGenericType)
				{
					var objectType = field.FieldType.GetGenericArguments()[0];
					var type = Type.GetType(string.Format(
						"System.Collections.Generic.IEnumerable`1[{0}]", objectType.FullName));
					var ctor = type.GetType().GetConstructor(new Type[] {type});
					if (ctor == null) throw new NullReferenceException(
						string.Format("{0} doesn't have a copy-construtor", type));
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldfld, field);
					generator.Emit(OpCodes.Newobj, ctor);
					generator.Emit(OpCodes.Stloc_0);
					generator.Emit(OpCodes.Ldloc_0);
					generator.Emit(OpCodes.Stfld);
//						generator.Emit(OpCodes.Stloc_0);
				}
			}
			else
			{
				var ctor = field.GetType().GetConstructor(Type.EmptyTypes);
				if (ctor == null) throw new NullReferenceException(
					string.Format("{0} doesn't have a default construtor", field.GetType()));
				generator.Emit(OpCodes.Newobj, ctor);
				generator.Emit(OpCodes.Stloc_1);
				generator.Emit(OpCodes.Ldloc_0);
				generator.Emit(OpCodes.Ldloc_1);
				generator.Emit(OpCodes.Stfld, field);
				foreach (var f in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (field.FieldType.IsValueType || field.FieldType.IsEnum || field.FieldType == typeof (string))
					{
						generator.Emit(OpCodes.Ldloc_1);
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Ldfld, field);
						generator.Emit(OpCodes.Ldfld, f);
						generator.Emit(OpCodes.Stfld, f);
					}
					if (field.FieldType.IsClass)
						CopyRefType(generator, field);
				}
			}
		}
	}
}
