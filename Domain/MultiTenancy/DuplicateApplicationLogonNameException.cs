using System;

namespace Teleopti.Ccc.Domain.MultiTenancy
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