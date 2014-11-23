using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TasksLibrary
{
	public class CustomMutex
	{
		private readonly Task _completed_task = Task.FromResult(true); 
		private volatile Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
		private volatile bool _isFree;

		public CustomMutex()
		{
			_isFree = true;
		}

		//this func return Task with releaser result
		//mutex if free it will return cached releaser
		//but if not it will return non copleted task with null releaser
		//Once the task is comleted, this func will create a new releaser for this task
		public Task<Releaser> LockSection()
		{
			var wait = Lock();
			return wait.IsCompleted
				? Task.FromResult(new Releaser(this))
				: wait.ContinueWith((_, state) => new Releaser((CustomMutex) state),
					this, CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
		
		//this func return completed task if mutex is free
		//but if mutex is not available for other threads this func
		//enqueue queue of waiting threads and return non completed task
		public Task Lock()
		{
			lock (waiters)
			{
				if (_isFree)
				{
					_isFree = false;
					return _completed_task;
				}
				var waiter = new TaskCompletionSource<bool>();
				waiters.Enqueue(waiter);
				return waiter.Task;
			}
		}

		//this func release a mutex and dequeue first waiting task
		//and set it as completed
		public void Release()
		{
			TaskCompletionSource<bool> toRelease = null;
			lock (waiters)
			{
				if (waiters.Count != 0)
					toRelease = waiters.Dequeue();
				else
					_isFree = true;

				if (toRelease != null)
					toRelease.SetResult(true);
			}
		}
		//Releaser store reference on mutex and when
		//thread go out of "using" scope, Dispose methode will be call
		//and release mutex
		public class Releaser : IDisposable
		{
			private bool _disposed = false;

			private readonly CustomMutex mutex;

			public Releaser(CustomMutex mutex)
			{
				this.mutex = mutex;
			}
			public void Dispose()
			{
//				Debug.WriteLine("Call Dispose");
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (_disposed) return;
				if (disposing)
					if (mutex != null)
						mutex.Release();
				_disposed = true;
			}

			~Releaser()
			{
//				Debug.WriteLine("Call Finalize");
				Dispose(false);
			}
		}
	}
}
