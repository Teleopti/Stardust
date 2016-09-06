using System;

namespace Teleopti.Ccc.Infrastructure.DistributedLock
{
	[Serializable]
	public class DistributedLockException : Exception
	{
		public DistributedLockException(string message) : base(message)
		{
		}
	}
}