using System;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	public class DistributedLockException : Exception
	{
		public DistributedLockException(string message) : base(message)
		{
		}
	}
}