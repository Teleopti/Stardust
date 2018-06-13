using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IReforecastedStaffingProvider
	{
		double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingList, IList<SkillIntervalStatistics> actualCallsPerSkillList, Dictionary<Guid, List<SkillIntervalStatistics>> forecastedCallsPerSkillDictionary, DateTime? latestStatsTime, int minutesPerInterval, DateTime[] timeSeries);
	}
}