using System;

namespace Teleopti.Ccc.Infrastructure.Util2
{
	public interface IRetryHandle
	{
		IRetryHandle WaitAndRetry(params TimeSpan[] waitTimes);
		void Do(Action action);
		T Execute<T>(Func<T> action);
	}
}