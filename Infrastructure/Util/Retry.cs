using System;
using System.Collections.Generic;
using System.Threading;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class Retry
	{
		public static IRetryHandle Handle<TException>()
		{
			return new RetryHandle<TException>();
		}
		
		class RetryHandle<TException> : IRetryHandle
		{
			private readonly Stack<TimeSpan> waitingTimes = new Stack<TimeSpan>();
			
			public IRetryHandle WaitAndRetry(params TimeSpan[] waitTimes)
			{
				foreach (var t in waitTimes)
				{
					waitingTimes.Push(t);
				}
				return this;
			}

			public void Do(Action action)
			{
				try
				{
					action.Invoke();
				}
				catch (Exception e) when(e is TException)
				{
					if (waitingTimes.Count == 0)
						throw;

					Thread.Sleep(waitingTimes.Pop());
					Do(action);
				}
			}
		}

	}
}