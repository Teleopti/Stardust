using System;
using System.Runtime.ExceptionServices;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IExceptionRethrower
	{
		void Rethrow(Exception e);
	}

	public class ExceptionLogger : IExceptionRethrower
	{
		public void Rethrow(Exception e)
		{
		}
	}

	public class ExceptionRethrower : IExceptionRethrower
	{
		public void Rethrow(Exception e)
		{
			ExceptionDispatchInfo.Capture(e).Throw();
		}
	}
}