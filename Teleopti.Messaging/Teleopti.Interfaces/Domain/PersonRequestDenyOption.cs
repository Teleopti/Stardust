using System;

namespace Teleopti.Interfaces.Domain
{
	[Flags]
	public enum PersonRequestDenyOption
	{
		None = 0,
		AutoDeny = 1,
		AlreadyAbsence = 2
	}
}
