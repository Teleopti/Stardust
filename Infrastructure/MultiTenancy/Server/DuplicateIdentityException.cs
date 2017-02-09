using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
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