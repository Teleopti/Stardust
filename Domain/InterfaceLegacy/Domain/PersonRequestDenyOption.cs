using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[Flags]
	public enum PersonRequestDenyOption
	{
		None = 0,
		AutoDeny = 1,
		AlreadyAbsence = 2,
		RequestExpired = 4,
		InsufficientPersonAccount = 8,
		AllPersonSkillsClosed = 16,
		TechnicalIssues = 32
	}
}
