using System;
using System.Collections.Generic;
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

		//this func return Task with releaser result
		//mutex if free it will return cached releaser
		//but if not it will return non copleted task with null releaser
		//Once the task is comleted, this func will create a new releaser for this task
		public Task<Releaser> LockSection()
		{
			var wait = Lock();
			return wait.IsCompleted ?
				releaser :
				wait.ContinueWith((_, state) => new Releaser((CustomMutex) state),
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
					return complete_task;
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
			}
			if (toRelease != null)
				toRelease.SetResult(true);
		}

		//Releaser store reference on mutex and when
		//thread go out of "using" scope, Dispose methode will be call
		//and release mutex
//		public struct Releaser : IDisposable
//		{
//			private readonly CustomMutex mutex;
//
//			internal Releaser(CustomMutex mutex)
//			{
//				this.mutex = mutex;
//			}
//			public void Dispose()
//			{
//				if(mutex!=null)
//					mutex.Release();
//			}
//		}
		
		public class Releaser : IDisposable
		{
			private bool _disposed = false;

			private readonly CustomMutex mutex;

			internal Releaser(CustomMutex mutex)
			{
				this.mutex = mutex;
			}
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (_disposed) return;
				if(disposing)
					if (mutex != null)
						mutex.Release();
				_disposed = true;
			}

			~Releaser()
			{
				Dispose(false);
			}
		}
	}
}
