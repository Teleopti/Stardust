using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DuplicateApplicationLogonNameException : Exception
	{
		public DuplicateApplicationLogonNameException(Guid person)
		{
			ExistingPerson = person;
		}

		public Guid ExistingPerson { get; }
	}
}