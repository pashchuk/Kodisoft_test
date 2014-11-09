using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
	}
}
