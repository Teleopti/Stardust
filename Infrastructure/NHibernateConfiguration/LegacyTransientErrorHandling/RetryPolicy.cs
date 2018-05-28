using System;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class RetryPolicy<T> : RetryPolicy where T : ITransientErrorDetectionStrategy, new()
	{
		public RetryPolicy(RetryStrategy retryStrategy)
			: base((ITransientErrorDetectionStrategy)new T(), retryStrategy)
		{
		}

		public RetryPolicy(int retryCount)
			: base((ITransientErrorDetectionStrategy)new T(), retryCount)
		{
		}

		public RetryPolicy(int retryCount, TimeSpan retryInterval)
			: base((ITransientErrorDetectionStrategy)new T(), retryCount, retryInterval)
		{
		}

		public RetryPolicy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: base((ITransientErrorDetectionStrategy)new T(), retryCount, minBackoff, maxBackoff, deltaBackoff)
		{
		}

		public RetryPolicy(int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: base((ITransientErrorDetectionStrategy)new T(), retryCount, initialInterval, increment)
		{
		}
	}

	public class RetryPolicy
	{
		private static RetryPolicy noRetry = new RetryPolicy((ITransientErrorDetectionStrategy)new RetryPolicy.TransientErrorIgnoreStrategy(), RetryStrategy.NoRetry);
		private static RetryPolicy defaultFixed = new RetryPolicy((ITransientErrorDetectionStrategy)new RetryPolicy.TransientErrorCatchAllStrategy(), RetryStrategy.DefaultFixed);
		private static RetryPolicy defaultProgressive = new RetryPolicy((ITransientErrorDetectionStrategy)new RetryPolicy.TransientErrorCatchAllStrategy(), RetryStrategy.DefaultProgressive);
		private static RetryPolicy defaultExponential = new RetryPolicy((ITransientErrorDetectionStrategy)new RetryPolicy.TransientErrorCatchAllStrategy(), RetryStrategy.DefaultExponential);

		public static RetryPolicy NoRetry
		{
			get
			{
				return RetryPolicy.noRetry;
			}
		}

		public static RetryPolicy DefaultFixed
		{
			get
			{
				return RetryPolicy.defaultFixed;
			}
		}

		public static RetryPolicy DefaultProgressive
		{
			get
			{
				return RetryPolicy.defaultProgressive;
			}
		}

		public static RetryPolicy DefaultExponential
		{
			get
			{
				return RetryPolicy.defaultExponential;
			}
		}

		public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
		{
			this.ErrorDetectionStrategy = errorDetectionStrategy;
			if (errorDetectionStrategy == null)
				throw new InvalidOperationException("The error detection strategy type must implement the ITransientErrorDetectionStrategy interface.");
			this.RetryStrategy = retryStrategy;
		}

		public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
			: this(errorDetectionStrategy, (RetryStrategy)new FixedInterval(retryCount))
		{
		}

		public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
			: this(errorDetectionStrategy, (RetryStrategy)new FixedInterval(retryCount, retryInterval))
		{
		}

		public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: this(errorDetectionStrategy, (RetryStrategy)new ExponentialBackoff(retryCount, minBackoff, maxBackoff, deltaBackoff))
		{
		}

		public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: this(errorDetectionStrategy, (RetryStrategy)new Incremental(retryCount, initialInterval, increment))
		{
		}

		public event EventHandler<RetryingEventArgs> Retrying;

		public RetryStrategy RetryStrategy { get; private set; }

		public ITransientErrorDetectionStrategy ErrorDetectionStrategy { get; private set; }

		public virtual void ExecuteAction(Action action)
		{
			this.ExecuteAction<object>((Func<object>)(() =>
			{
				action();
				return (object)null;
			}));
		}

		public virtual TResult ExecuteAction<TResult>(Func<TResult> func)
		{
			int retryCount = 0;
			TimeSpan delay = TimeSpan.Zero;
			ShouldRetry shouldRetry = this.RetryStrategy.GetShouldRetry();
			Exception currentException = null;
			while (true)
			{
				do
				{
					try
					{
						return func();
					}
#pragma warning disable 618
					catch (RetryLimitExceededException ex)
#pragma warning restore 618
					{
						if (ex.InnerException != null)
							throw ex.InnerException;
						return default(TResult);
					}
					catch (Exception ex)
					{
						if (this.ErrorDetectionStrategy.IsTransient(ex))
						{
							currentException = ex;
							if (shouldRetry(retryCount++, ex, out delay))
								goto label_9;
						}
						throw;
					}
					label_9:
					if (delay.TotalMilliseconds < 0.0)
						delay = TimeSpan.Zero;
					this.OnRetrying(retryCount, currentException, delay);
				}
				while (retryCount <= 1 && this.RetryStrategy.FastFirstRetry);
				Task.Delay(delay).Wait();
			}
		}

		public Task ExecuteAsync(Func<Task> taskAction)
		{
			return this.ExecuteAsync(taskAction, new CancellationToken());
		}

		public Task ExecuteAsync(Func<Task> taskAction, CancellationToken cancellationToken)
		{
			if (taskAction == null)
				throw new ArgumentNullException(nameof(taskAction));
			return (Task)new AsyncExecution(taskAction, this.RetryStrategy.GetShouldRetry(), new Func<Exception, bool>(this.ErrorDetectionStrategy.IsTransient), new Action<int, Exception, TimeSpan>(this.OnRetrying), this.RetryStrategy.FastFirstRetry, cancellationToken).ExecuteAsync();
		}

		public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
		{
			return this.ExecuteAsync<TResult>(taskFunc, new CancellationToken());
		}

		public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
		{
			if (taskFunc == null)
				throw new ArgumentNullException(nameof(taskFunc));
			return new AsyncExecution<TResult>(taskFunc, this.RetryStrategy.GetShouldRetry(), new Func<Exception, bool>(this.ErrorDetectionStrategy.IsTransient), new Action<int, Exception, TimeSpan>(this.OnRetrying), this.RetryStrategy.FastFirstRetry, cancellationToken).ExecuteAsync();
		}

		protected virtual void OnRetrying(int retryCount, Exception lastError, TimeSpan delay)
		{
			if (this.Retrying == null)
				return;
			this.Retrying((object)this, new RetryingEventArgs(retryCount, delay, lastError));
		}

		private sealed class TransientErrorIgnoreStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				return false;
			}
		}

		private sealed class TransientErrorCatchAllStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				return true;
			}
		}
	}
}