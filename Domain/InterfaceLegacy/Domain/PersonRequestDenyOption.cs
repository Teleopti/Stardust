﻿using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[Flags]
	public enum PersonRequestDenyOption
	{
		None = 0,
		AutoDeny = 1,
		AlreadyAbsence = 2,
		RequestExpired = 4
	}
}
