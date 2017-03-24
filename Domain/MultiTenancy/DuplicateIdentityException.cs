using System;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class DuplicateIdentityException : Exception
	{
		public DuplicateIdentityException(Guid existingPerson)
		{
			ExistingPerson = existingPerson;
		}

		public Guid ExistingPerson { get; }
	}
}