using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ITaskPeriodsProvider
	{
		IEnumerable<ISkillStaffPeriodView> Load(ISkillDay skillDay, int minutesPerInterval, DateTime? latestStatisticsTime, DateTime? nullableCurrentDateTime);
	}
}