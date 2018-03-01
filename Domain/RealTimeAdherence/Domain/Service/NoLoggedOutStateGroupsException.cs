using System;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class NoLoggedOutStateGroupsException : Exception
	{
		public NoLoggedOutStateGroupsException(string message) : base(message)
		{
		}
	}
}