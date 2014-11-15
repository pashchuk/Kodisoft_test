using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TasksLibrary
{
	public static class Extensions
	{

		#region Continue With methods

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

		#endregion

		//this method create a deep clone of any object of any type with
		//any ctor. but if it doesn't have a default ctor that maybe
		//using of this object will be hard
		public static T DeepClone<T>(this T obj)
		{
			if (obj == null) return default(T);
			var type = obj.GetType();
			//return current value if object is has ValueType type
			if (type.IsValueType || type.IsEnum)
				return obj;
			if (type == typeof(string))
			{
				return (T) Convert.ChangeType(new String((obj.ToString()).ToCharArray()), type);
			}
			//get default ctor
			var ctor = type.GetConstructor(Type.EmptyTypes);
			//try to create instance using a default ctor
			//else create non initialize instatce
			var result = ctor != null ? ctor.Invoke(null) : FormatterServices.GetUninitializedObject(type);
			//get all public and private field
			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				//copy valueType field
				if (field.FieldType.IsValueType || field.FieldType.IsEnum || field.FieldType == typeof (string))
					field.SetValue(result, field.GetValue(obj));
				else//copy reference type
				{
					if (field.FieldType.IsArray)
						field.SetValue(result, arrayClone(field.FieldType.GetElementType(), ((Array) field.GetValue(obj))));
					else
						field.SetValue(result, field.GetValue(obj).DeepClone<object>());
				}
			}
			return (T)result;
		}

		//help func to clone array type of field
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

	//this class represent a deep clone function which using
	//low-level "Intermediate Language (CIL)"
	public static class Cloner
	{
		//cache for created functions
		private static Dictionary<Type,Delegate> _cache = new Dictionary<Type, Delegate>();

		//this method create a deep clone of any object of any type which
		//have a default ctor. but if it doesn't have a default ctor that 
		//throw a NullReferenceException
		public static T ILClone<T>(this T obj)
		{
			Delegate myExec = null;
			Type objectType = typeof (T);
			if (!_cache.TryGetValue(objectType, out myExec))
			{
				DynamicMethod method = new DynamicMethod("DoClone", objectType, new Type[] {objectType}, true);
				//get IL generator
				ILGenerator generator = method.GetILGenerator();
				if (objectType == typeof (string))
				{
					return (T) Convert.ChangeType(new String((obj.ToString()).ToCharArray()), objectType);
				}
				if (objectType.IsValueType || objectType.IsEnum)
				{
					return obj;
				}
				//declare local variable
				generator.DeclareLocal(objectType);
				//try to get a defult ctor
				ConstructorInfo constructor = objectType.GetConstructor(new Type[] {});
				if (constructor == null)
					throw new NullReferenceException(string.Format("The {0} type doesn't have a default ctor", objectType));
				//create instance using default ctor
				generator.Emit(OpCodes.Newobj, constructor);
				generator.Emit(OpCodes.Stloc_0);
				//get all fields in current object
				foreach (var field in objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					//copy valuetype field
					if (field.FieldType.IsValueType || field.FieldType == typeof (string))
						CopyValueType(generator, field);
					//copy reference field
					if (field.FieldType.IsClass)
						CopyRefType(generator, field);
				}
				//load saved clone object to stack
				generator.Emit(OpCodes.Ldloc_0);
				//return first object in stack
				generator.Emit(OpCodes.Ret);
				//create and add pointer on created function to local cache
				myExec = method.CreateDelegate(typeof (Func<T, T>));
				_cache.Add(objectType, myExec);
			}
			//execute local function and return clone
			return ((Func<T, T>) myExec)(obj);
		}

		//copy valuetype field
		private static void CopyValueType(ILGenerator generator,FieldInfo field)
		{
			//load clone object to stack
			generator.Emit(OpCodes.Ldloc_0);
			//load empty object to stack
			generator.Emit(OpCodes.Ldarg_0);
			//replace empty object from stack on field
			generator.Emit(OpCodes.Ldfld, field);
			//save field in clone object and store this object in local variable
			generator.Emit(OpCodes.Stfld, field);
		}

		//copy reference field
		private static void CopyRefType(ILGenerator generator, FieldInfo field)
		{
			//clone collection
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
			//clone other reference type
			else
			{
				//try to get a default ctor
				var ctor = field.GetType().GetConstructor(Type.EmptyTypes);
				if (ctor == null) throw new NullReferenceException(
					string.Format("{0} doesn't have a default construtor", field.GetType()));
				//create instance of new object
				generator.Emit(OpCodes.Newobj, ctor);
				//save it to local variable
				generator.Emit(OpCodes.Stloc_1);
				//load clone object to stack from local var
				generator.Emit(OpCodes.Ldloc_0);
				//load new object to stack from local var
				generator.Emit(OpCodes.Ldloc_1);
				//save new object in clone object and load it to local variable
				generator.Emit(OpCodes.Stfld, field);
				//get all fields
				foreach (var f in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (field.FieldType.IsValueType || field.FieldType.IsEnum || field.FieldType == typeof (string))
					{
						//load new object to stack from local variable
						generator.Emit(OpCodes.Ldloc_1);
						//load empty object to stack
						generator.Emit(OpCodes.Ldarg_0);
						//replace empty object on needed field
						generator.Emit(OpCodes.Ldfld, field);
						//replace needed field on local field
						generator.Emit(OpCodes.Ldfld, f);
						//store this field
						generator.Emit(OpCodes.Stfld, f);
					}
					if (field.FieldType.IsClass)
						CopyRefType(generator, field);
				}
			}
		}
	}
}
