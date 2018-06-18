using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IRequiredStaffingProvider
	{
		double?[] DataSeries(IList<StaffingIntervalModel> requiredStaffingPerSkill, DateTime? latestStatsTime, int minutesPerInterval, DateTime[] timeSeries);
		IList<StaffingIntervalModel> Load(IList<SkillIntervalStatistics> actualStatistics, IList<ISkill> skills, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, IList<StaffingIntervalModel> forecastedStaffingIntervals, TimeSpan resolution, IList<SkillDayStatsRange> skillDayStatsRange, Dictionary<Guid, int> workloadBacklog);
	}
}