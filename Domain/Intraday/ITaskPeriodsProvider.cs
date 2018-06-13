using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ITaskPeriodsProvider
	{
		IEnumerable<ITemplateTaskPeriod> Load(ISkillDay skillDay, int minutesPerInterval, DateTime? latestStatisticsTime);
		IEnumerable<ITemplateTaskPeriod> Load(ISkillDay skillDay, int minutesPerInterval, DateTime? latestStatisticsTime, DateTime? nullableCurrentDateTime);
	}
}