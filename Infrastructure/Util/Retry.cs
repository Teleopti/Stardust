using System;
using System.Linq;
using Polly;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class Retry
	{
		public static IRetryHandle Handle<TException>() where TException : Exception
		{
			return new RetryHandle<TException>();
		}
		
		class RetryHandle<TException> : IRetryHandle where TException : Exception
		{
			private Policy _policy;
			
			public IRetryHandle WaitAndRetry(params TimeSpan[] waitTimes)
			{
				_policy = Policy.Handle<TException>()
					.WaitAndRetry(new []{ TimeSpan.Zero }.Concat(waitTimes));
				return this;
			}

			public void Do(Action action)
			{
				(_policy ?? Policy.NoOp()).Execute(action);
			}
		}
	}
}