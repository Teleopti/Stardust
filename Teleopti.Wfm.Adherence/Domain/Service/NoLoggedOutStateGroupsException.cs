using System;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class NoLoggedOutStateGroupsException : Exception
	{
		public NoLoggedOutStateGroupsException(string message) : base(message)
		{
		}
	}
}