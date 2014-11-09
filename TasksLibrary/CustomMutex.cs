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
		private readonly Task<Releaser> releaser;
		private static readonly Task complete_task = Task.FromResult(true);
		private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
		private bool _isFree;

		public CustomMutex()
		{
			_isFree = true;
			releaser = Task.FromResult(new Releaser(this));
		}

		public Task<Releaser> LockSection()
		{
			var wait = Lock();
			return wait.IsCompleted ?
				releaser :
				wait.ContinueWith((_, state) => new Releaser((CustomMutex) state),
					this, CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default); 
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
				var waiter = new TaskCompletionSource<bool>();
				waiters.Enqueue(waiter);
				return waiter.Task;
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

		public struct Releaser : IDisposable
		{
			private readonly CustomMutex mutex;

			internal Releaser(CustomMutex mutex)
			{
				this.mutex = mutex;
			}
			public void Dispose()
			{
				if(mutex!=null)
					mutex.Release();
			}
		}
	}
}
