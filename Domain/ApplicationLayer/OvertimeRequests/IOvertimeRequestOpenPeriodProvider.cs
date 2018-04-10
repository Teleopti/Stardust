﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestOpenPeriodProvider
	{
		IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetOvertimeRequestOpenPeriods(IPerson person, DateTimePeriod period);
	}
}
