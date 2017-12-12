using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class PersonIdentityMatchResult
	{
		public string Identity { get; set; }
		public Guid PersonId { get; set; }
		public IdentityMatchField MatchField { get; set; }
	}

	public enum IdentityMatchField
	{
		EmploymentNumber = 0,
		ApplicationLogonName = 1,
		ExternalLogon = 2
	}
}