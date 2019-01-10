using System;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class PersonNotFoundException : Exception
	{
		public PersonNotFoundException(string message) : base(message)
		{
		}

		public PersonNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}