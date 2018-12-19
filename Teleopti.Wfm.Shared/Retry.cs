using System;
using System.Collections.Generic;
using Polly;
using Teleopti.Ccc.Domain.Collection2;

namespace Teleopti.Ccc.Infrastructure.Util2
{
	public class Retry
	{
		public static IRetryHandle Handle<TException>() where TException : Exception
		{
			return new RetryHandle<TException>();
		}
		
		class RetryHandle<TException> : IRetryHandle where TException : Exception
		{
			private readonly List<TimeSpan> _waitTimes = new List<TimeSpan>();
			
			public IRetryHandle WaitAndRetry(params TimeSpan[] waitTimes)
			{
				_waitTimes.AddRange(waitTimes);
				return this;
			}

			public void Do(Action action)
			{
				var policy = _waitTimes.IsEmpty()
					? (Policy)Policy.NoOp()
					: Policy.Handle<TException>().WaitAndRetry(_waitTimes);

				policy.Execute(action);
			}
			
			public T Execute<T>(Func<T> action)
			{
				var policy = _waitTimes.IsEmpty()
					? (Policy)Policy.NoOp()
					: Policy.Handle<TException>().WaitAndRetry(_waitTimes);

				return policy.Execute(action);
			}
		}
	}
}