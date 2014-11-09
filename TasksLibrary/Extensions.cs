using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

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
		public static async Task<TResult> CustomContinueWith<TResult>(this Task task, Func<TResult> func)
		{
			return await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}
		public async static Task<TResult> CustomContinueWith<TResult>(this Task task, Func<Task<TResult>> func)
		{
			return await await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> CustomContinueWith<TResult>(this Task task, Func<Task, TResult> func)
		{
			return await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> CustomContinueWith<TResult>(this Task task, Func<Task, Task<TResult>> func)
		{
			return await await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
