using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	internal class AsyncExecution<TResult>
	{
		private readonly Func<Task<TResult>> taskFunc;
		private readonly ShouldRetry shouldRetry;
		private readonly Func<Exception, bool> isTransient;
		private readonly Action<int, Exception, TimeSpan> onRetrying;
		private readonly bool fastFirstRetry;
		private readonly CancellationToken cancellationToken;
		private Task<TResult> previousTask;
		private int retryCount;

		public AsyncExecution(Func<Task<TResult>> taskFunc, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken)
		{
			this.taskFunc = taskFunc;
			this.shouldRetry = shouldRetry;
			this.isTransient = isTransient;
			this.onRetrying = onRetrying;
			this.fastFirstRetry = fastFirstRetry;
			this.cancellationToken = cancellationToken;
		}

		internal Task<TResult> ExecuteAsync()
		{
			return this.ExecuteAsyncImpl((Task)null);
		}

		private Task<TResult> ExecuteAsyncImpl(Task ignore)
		{
			if (this.cancellationToken.IsCancellationRequested)
			{
				if (this.previousTask != null)
					return this.previousTask;
				TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
				completionSource.TrySetCanceled();
				return completionSource.Task;
			}
			Task<TResult> task;
			try
			{
				task = this.taskFunc();
			}
			catch (Exception ex)
			{
				if (this.isTransient(ex))
				{
					TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
					completionSource.TrySetException(ex);
					task = completionSource.Task;
				}
				else
					throw;
			}
			if (task == null)
				throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "TaskCannotBeNull", new object[1]
				{
					(object) "taskFunc"
				}), "taskFunc");
			if (task.Status == TaskStatus.RanToCompletion)
				return task;
			if (task.Status == TaskStatus.Created)
				throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "TaskMustBeScheduled", new object[1]
				{
					(object) "taskFunc"
				}), "taskFunc");
			return task.ContinueWith<Task<TResult>>(new Func<Task<TResult>, Task<TResult>>(this.ExecuteAsyncContinueWith), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap<TResult>();
		}

		private Task<TResult> ExecuteAsyncContinueWith(Task<TResult> runningTask)
		{
			if (!runningTask.IsFaulted || this.cancellationToken.IsCancellationRequested)
				return runningTask;
			TimeSpan delay = TimeSpan.Zero;
			Exception innerException = runningTask.Exception.InnerException;
#pragma warning disable 618
			if (innerException is RetryLimitExceededException)
#pragma warning restore 618
			{
				TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
				if (innerException.InnerException != null)
					completionSource.TrySetException(innerException.InnerException);
				else
					completionSource.TrySetCanceled();
				return completionSource.Task;
			}
			if (!this.isTransient(innerException) || !this.shouldRetry(this.retryCount++, innerException, out delay))
				return runningTask;
			if (delay < TimeSpan.Zero)
				delay = TimeSpan.Zero;
			this.onRetrying(this.retryCount, innerException, delay);
			this.previousTask = runningTask;
			if (delay > TimeSpan.Zero && (this.retryCount > 1 || !this.fastFirstRetry))
				return Task.Delay(delay).ContinueWith<Task<TResult>>(new Func<Task, Task<TResult>>(this.ExecuteAsyncImpl), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap<TResult>();
			return this.ExecuteAsyncImpl((Task)null);
		}
	}

	internal class AsyncExecution : AsyncExecution<bool>
	{
		private static Task<bool> cachedBoolTask;

		public AsyncExecution(Func<Task> taskAction, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken)
			: base((Func<Task<bool>>)(() => AsyncExecution.StartAsGenericTask(taskAction)), shouldRetry, isTransient, onRetrying, fastFirstRetry, cancellationToken)
		{
		}

		private static Task<bool> StartAsGenericTask(Func<Task> taskAction)
		{
			Task task = taskAction();
			if (task == null)
				throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "TaskCannotBeNull", new object[1]
				{
					(object) nameof (taskAction)
				}), nameof(taskAction));
			if (task.Status == TaskStatus.RanToCompletion)
				return AsyncExecution.GetCachedTask();
			if (task.Status == TaskStatus.Created)
				throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "TaskMustBeScheduled", new object[1]
				{
					(object) nameof (taskAction)
				}), nameof(taskAction));
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			task.ContinueWith((Action<Task>)(t =>
			{
				if (t.IsFaulted)
					tcs.TrySetException((IEnumerable<Exception>)t.Exception.InnerExceptions);
				else if (t.IsCanceled)
					tcs.TrySetCanceled();
				else
					tcs.TrySetResult(true);
			}), TaskContinuationOptions.ExecuteSynchronously);
			return tcs.Task;
		}

		private static Task<bool> GetCachedTask()
		{
			if (AsyncExecution.cachedBoolTask == null)
			{
				TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
				completionSource.TrySetResult(true);
				AsyncExecution.cachedBoolTask = completionSource.Task;
			}
			return AsyncExecution.cachedBoolTask;
		}
	}
}