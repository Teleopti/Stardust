using System;

namespace Teleopti.Wfm.Adherence.States
{
	public class NoLoggedOutStateGroupsException : Exception
	{
		public NoLoggedOutStateGroupsException(string message) : base(message)
		{
		}
	}
}