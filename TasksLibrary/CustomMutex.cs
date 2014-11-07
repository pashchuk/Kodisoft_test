using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TasksLibrary
{
	public class CustomMutex
	{
		private static readonly Task complete_task = Task.FromResult(true);
		private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
		private bool _isFree;

		public CustomMutex()
		{
			_isFree = true;
		}
		public Task Lock()
		{
			lock (waiters)
			{
				if (_isFree)
				{
					_isFree = false;
					return complete_task;
				}
				else
				{
					var waiter = new TaskCompletionSource<bool>();
					waiters.Enqueue(waiter);
					return waiter.Task;
				}
			}
		}
		public void Release()
		{
			TaskCompletionSource<bool> toRelease = null;
			lock (waiters)
			{
				if (waiters.Count != 0)
					toRelease = waiters.Dequeue();
				else
					_isFree = true;
			}
			if (toRelease != null)
				toRelease.SetResult(true);
		}
	}
}
