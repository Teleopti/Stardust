using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IForecastedCallsProvider
	{
		ForecastedCallsModel Load(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateTime? latestStatisticsTime, int minutesPerInterval, DateTime? currentDateTime = null);
	}
}