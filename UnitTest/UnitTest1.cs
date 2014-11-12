using System;
using System.CodeDom;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginLibrary.Plugins;
using TasksLibrary;

namespace UnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void Test_GetRandomDigit()
		{
			//arrange
			int maxvalue = 50;
			RandomGenerator rnd = new RandomGenerator(50);

			//act
			int rndDigit = rnd.RandomDigit;

			//assert
			Assert.IsFalse(rndDigit < 0);
			Assert.IsFalse(rndDigit > maxvalue);
		}

		[TestMethod]
		public void Test_GetDigits()
		{
			//arrange
			int maxValue = 100;
			RandomGenerator rnd = new RandomGenerator(maxValue);

			//act
			foreach (var digit in rnd.GetDigits(50))
			{
				Assert.IsFalse(digit < 0);
				Assert.IsFalse(digit > maxValue);
			}
		}
		[TestMethod]
		public void Test_Create()
		{
			//arrange
			var coll = new AssemblyCollection();
			NullReferenceException exception = null;

			//act
			try
			{
				var str = coll.Create<String>();
			}
			catch (NullReferenceException ex) { exception = ex; }
			var mutex = coll.Create<CustomMutex>();
			var random = coll.Create<Random>();

			//assert
			Assert.IsTrue(mutex != null);
			Assert.IsTrue(random != null);
			Assert.IsTrue(exception != null);
		}

		[TestMethod]
		public async Task Test_LockSection()
		{
			//arrange
			CustomMutex mutex = new CustomMutex();
			Task<CustomMutex.Releaser> task = null;

			//act
			using (await mutex.LockSection())
			{
				task = mutex.LockSection();
				Assert.IsTrue(task != null);
				Assert.IsFalse(task.IsCompleted);
			}
			Assert.IsTrue(task.IsCompleted);
			Assert.IsNotNull(task.Result);
		}

		[TestMethod]
		public async Task Test_Lock()
		{
			//arrange
			var mutex = new CustomMutex();

			//act
			var task1 = mutex.Lock();
			Assert.IsTrue(task1.IsCompleted);
			var task2 = mutex.Lock();
			Assert.IsFalse(task2.IsCompleted);
			await task1;
			mutex.Release();
			Assert.IsTrue(task2.IsCompleted);
			await task2;
			mutex.Release();
		}

		[TestMethod]
		public void Test_CloneString()
		{
			//arrange
			var str = "test test";

			//act
			var clone = str.DeepClone();

			//assert
			Assert.AreEqual(str, clone);
			Assert.AreNotSame(str, clone);
		}

		[TestMethod]
		public void Test_CloneValueType()
		{
			//arrange
			int a = 10;
			double b = 20.357;
			var c = BindingFlags.NonPublic;

			//act
			var a_clone = a.DeepClone();
			var b_clone = b.DeepClone();
			var c_clone = c.DeepClone();

			//assert
			Assert.AreEqual(a_clone, a);
			Assert.AreEqual(b_clone, b);
			Assert.AreEqual(c_clone, c);
			Assert.AreNotSame(a_clone, a);
			Assert.AreNotSame(b_clone, b);
			Assert.AreNotSame(c_clone, c);
		}

		[TestMethod]
		public void Test_CloneRef()
		{
			//arrange
			var plugin = new IntPlugin(10);
			var result = plugin.Modify(2);

			//act
			var clone = plugin.DeepClone();
			var clone_res = clone.Modify(2);

			//assert
			Assert.AreNotSame(plugin, clone);
			Assert.AreEqual(result, clone_res);
		}

		[TestMethod]
		public void Test_ILCloneRef()
		{
			//arrange
			var a = new IntPlugin(10);
			var res = a.Modify(2);

			//act
			var clone = a.ILClone();
			var clone_res = clone.Modify(2);

			//assert
			Assert.AreNotSame(a, clone);
			Assert.AreEqual(res, clone_res);
		}

		[TestMethod]
		public void Test_ILCloneValueType()
		{
			//arrange
			int a = 10;
			double b = 20.357;
			var c = BindingFlags.NonPublic;

			//act
			var a_clone = a.ILClone();
			var b_clone = b.ILClone();
			var c_clone = c.ILClone();

			//assert
			Assert.AreEqual(a_clone, a);
			Assert.AreEqual(b_clone, b);
			Assert.AreEqual(c_clone, c);
			Assert.AreNotSame(a_clone, a);
			Assert.AreNotSame(b_clone, b);
			Assert.AreNotSame(c_clone, c);
		}
		[TestMethod]
		public void Test_ILCloneString()
		{
			//arrange
			var str = "test test";

			//act
			var clone = str.ILClone();

			//assert
			Assert.AreEqual(str, clone);
			Assert.AreNotSame(str, clone);
		}
	}
}
