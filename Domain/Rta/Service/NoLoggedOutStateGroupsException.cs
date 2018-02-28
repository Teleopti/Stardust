using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class NoLoggedOutStateGroupsException : Exception
	{
		public NoLoggedOutStateGroupsException(string message) : base(message)
		{
		}
	}
}