using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using TasksLibrary;

namespace WpfTest
{
	public partial class MainWindow
	{
		//in this method i use two different version of deep cloning such as
		//Deep clone(by using reflection) and Deep clone (by using MSIL).
		//After task will be completed, you'll see that IL method is much faster than reflection version
		private static string TestClone(BackgroundWorker worker)
		{
			var result = new StringBuilder();
			var watch = new Stopwatch();
			result.Append("--------Test Clone methods----------\r\n");
			//test clone method speed (10M iteration per method)
			var test = Enumerable.Repeat(new TestClass(157.536f)
			{Propery = 100, StringProperty = "test test"}, 10000000);
			//test clone method with reflection
			int iter = 200000;
			int current = 0;
			watch.Start();
			foreach (var intPlugin in test)
			{
				intPlugin.DeepClone();
				if (current++%1000 == 0) worker.ReportProgress(current/iter);
			}

			watch.Stop();
			result.Append(string.Format("Clone time (reflection) : {0}\r\n", watch.Elapsed));
			//test dynamic IL clone method
			watch.Restart();
			foreach (var intPlugin in test)
			{
				intPlugin.ILClone();
				if (current++ % 1000 == 0) worker.ReportProgress(current / iter);
			}
			watch.Stop();
			worker.ReportProgress(100);
			//save result of cloning process
			result.Append(string.Format("Clone time (dynamic IL) : {0}\r\n", watch.Elapsed));
			result.Append(string.Format("--------Test Clone result----------\r\n"));
			var testClone1 = "test test";
			var testClone2 = 254;
			var testClone3 = new TestClass(25.37) {Propery = 10, StringProperty = "test result"};
			var testCloneResult1 = testClone1.DeepClone();
			var testCloneResult2 = testClone2.ILClone();
			var testCloneResult3 = testClone3.DeepClone();
			result.Append(string.Format("Clone str: {0} | {1}\r\n", testClone1, testCloneResult1));
			result.Append(string.Format("Clone int: {0} | {1}\r\n", testClone2, testCloneResult2));
			result.Append(string.Format("Clone ref: {0} {1} | {2} {3}\r\n", testClone3.Propery, testClone3.StringProperty,
				testCloneResult3.Propery,
				testClone3.StringProperty));
			return result.ToString();
		}

		private async void TestContinueWith()
		{
			var str = new StringBuilder();
			TextBox.Text += "--------Test ContinueWith methods started--------\r\n";
			TextBox.Text += "Main thread id = " + Thread.CurrentThread.ManagedThreadId + "\r\n";
			
			//test ContinueWith (Action)
			TextBox.Text += "---1 test ContinueWith (Action)---\r\n";
			Task.Run(() => Thread.Sleep(1000)).CustomContinueWith(() => { TextBox.Text += "1 ContinueWith (Action) complete\r\n"; });
			
			//test ContinueWith (Action<Task>)
			//I overloaded new ContinueWith method (Action<Task<Result>>)
			//that allow me to get a return value from prev task.
			TextBox.Text += "---2 test ContinueWith (Action<Task>)---\r\n";
			for (var i = 0; i < 4; i++)
				Task.Factory.StartNew(() =>
				{
					Thread.Sleep(1000);
					return Thread.CurrentThread.ManagedThreadId;
				}).CustomContinueWith(task =>
				{
					TextBox.Text += string.Format("2 Previous task id = {0}. Thread id = {1}\r\n", task.Id, task.Result);
					TextBox.Text += string.Format("2 Continue task id = {0}. Thread id = {1}\r\n", Task.CurrentId,
						Thread.CurrentThread.ManagedThreadId);
				});
			
			//test ContinueWith (Func<T>)
			str.Append("---3 test ContinueWith (Func<T>)---\r\n");
			str.Append(await Task.Run(() => Thread.Sleep(1000)).CustomContinueWith(() => "3 ContinueWith (Func<T>) complete\r\n"));
			
			//test ContinueWith (Func<Task<T>>)
			str.Append("---4 test ContinueWith (Func<Task<T>>)---\r\n");
			str.Append(
				await await Task.Run(() => Thread.Sleep(1000))
					.CustomContinueWith(() => Task.Run(() => "4 ContinueWith (Func<Task<T>>) complete\r\n")));

			//test ContinueWith (Func<Task, T>)
			str.Append("---5 test ContinueWith (Func<Task, T>)---\r\n");
			str.Append(
				await Task.Run(() =>Thread.Sleep(1000)).CustomContinueWith(t =>
				{
					return string.Format("5 ContinueWith (Func<Task, T>) complete..prev task id = {0}\r\n", t.Id);
				}));
			
			//test ContinueWith (Func<Task, Task<T>>)
			str.Append("---6 test ContinueWith (Func<Task, Task<T>>)---\r\n");
			str.Append(
				await await Task.Run(() => Thread.Sleep(1000)).CustomContinueWith(t =>
				{
					return Task.Run(() =>
						string.Format("6 ContinueWith (Func<Task, Task<T>>) complete..prev task id = {0}\r\n", t.Id));
				}));

			TextBox.Text += str.ToString();

		}

		private async void TestMutexLock()
		{
			var str = new StringBuilder();
			TextBox.Text += "--------Test Mutex.Lock started--------\r\n";
			TextBox.Text += "--------Before using mutex--------\r\n";
			var tasks = new List<Task>();
			//create 20 new tasks
			for (int i = 0; i < 20; i++)
				tasks.Add(Task.Factory.StartNew(() => testWithoutMutex(str)));
			Task.WaitAll(tasks.ToArray());
			//print result of completed tasks
			TextBox.Text += str.ToString();
			TextBox.Text += "--------After using mutex--------\r\n";
			tasks.Clear();
			str.Clear();
			var mutex = new CustomMutex();
			//create 20 new tasks but they synchronized by mutex
			for (int i = 0; i < 20; i++)
				tasks.Add(Task.Factory.StartNew(() => testWithMutex(mutex, str)));
//			Task.WaitAll(tasks.ToArray());
			foreach (var task in tasks)
				await task;
			//print result
			TextBox.Text += str.ToString();
			str.Clear();
		}

		private async void TestUsingMutexLock()
		{
			var str = new StringBuilder();
			TextBox.Text += "--------Test using(Mutex.Lock) started--------\r\n";
			TextBox.Text += "--------Before using mutex--------\r\n";
			var tasks = new List<Task>();
			//create 20 new tasks
			for (int i = 0; i < 20; i++)
				tasks.Add(Task.Factory.StartNew(() => testWithoutMutex(str)));
			Task.WaitAll(tasks.ToArray());
			//print result of completed tasks
			TextBox.Text += str.ToString();
			TextBox.Text += "--------After using mutex--------\r\n";
			tasks.Clear();
			str.Clear();
			var mutex = new CustomMutex();
			//create 20 new tasks but they synchronized by mutex using "using" scope
			for (int i = 0; i < 20; i++)
				tasks.Add(Task.Factory.StartNew(() => testUsingWithMutex(mutex, str)));
			//Task.WaitAll(tasks.ToArray());
			foreach (var task in tasks)
				await task;
			//print result
			TextBox.Text += str.ToString();
			str.Clear();
		}

		private void TestRandomGenerator()
		{
			var str = new StringBuilder();
			TextBox.Text += "--------Test Random Generator started--------\r\n";
			TextBox.Text += "---Print 1000 random digits---\r\n";
			//init random with max value 30
			var random = new RandomGenerator(30);
			//get 500 random digits
			for (int i = 0; i < 500; i++)
				str.Append(random.GetOneDigit() + " ");
			//get 500 random digits using iterator
			foreach (var dig in random.GetDigits(500))
				str.Append(dig + " ");
			//print result
			TextBox.Text += str.ToString();
		}

		private void TestAssemblyCollection()
		{
			var collection = new AssemblyCollection();
			TextBox.Text += "--------Test Random Generator started--------\r\n";
			//create 3 simple object for example
			var a = collection.Create<TestClass>();
			var b = collection.Create<Button>();
			var c = collection.Create<CustomMutex>();
			TextBox.Text += string.Concat("created TestClass object: int property = ", a.Propery, "\r\n");
			TextBox.Text += string.Concat("created Button object: font size = ", b.FontSize, "\r\n");
			TextBox.Text += string.Concat("created CustomMutex object: full name = ", c.GetType(), "\r\n");
		}

		private async Task testWithMutex(CustomMutex mutex, StringBuilder result)
		{
			await mutex.Lock();
			for (int j = 0; j < 20; j++)
				result.Append(j + " ");
			result.Append("\r\n");
			mutex.Release();
		}

		private async Task testUsingWithMutex(CustomMutex mutex, StringBuilder result)
		{
			using (await mutex.LockSection())
			{
				for (int j = 0; j < 20; j++)
					result.Append(j + " ");
				result.Append("\r\n");
			}
		}

		private void testWithoutMutex(StringBuilder result)
		{
			for (int j = 0; j < 20; j++)
				result.Append(j + " ");
			result.Append("\r\n");
		}
		private class TestClass
		{
			public int Propery { get; set; }
			private double _field;
			public string StringProperty { get; set; }

			public TestClass() { }

			public TestClass(double field)
			{
				_field = field;
			}
		}
	}
}
