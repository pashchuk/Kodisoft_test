using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TasksLibrary
{
	public static class Extensions
	{
		public static Task ContinueWith(this Task task, Action action)
		{ 
			return task.ContinueWith(a => action(), TaskScheduler.FromCurrentSynchronizationContext());
		}
		public static Task ContinueWith(this Task task, Action<Task> action)
		{
			return task.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext());
		}
		public static async Task<TResult> ContinueWith<TResult>(this Task task, Func<TResult> func)
		{
			return await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}
		public async static Task<TResult> ContinueWith<TResult>(this Task task, Func<Task<TResult>> func)
		{
			return await await task.ContinueWith(ignored => func(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> ContinueWith<TResult>(this Task task, Func<Task, TResult> func)
		{
			return await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public static async Task<TResult> ContinueWith<TResult>(this Task task, Func<Task, Task<TResult>> func)
		{
			return await await task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
