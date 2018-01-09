using System;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public interface IRetryHandle
	{
		IRetryHandle WaitAndRetry(params TimeSpan[] waitTimes);
		void Do(Action action);
	}
}