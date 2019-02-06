using System;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class DeadLockVictimException : DataSourceException
	{
		public DeadLockVictimException(string message, Exception innerException):base(message, innerException)
		{
		}
	}
}