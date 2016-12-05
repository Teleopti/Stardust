using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SkillStaffingIntervalProvider
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;

		public SkillStaffingIntervalProvider(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, SplitSkillStaffInterval splitSkillStaffInterval)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_splitSkillStaffInterval = splitSkillStaffInterval;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution)
		{
			var skillIntervals = _scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skillIdList, period);
			var splittedIntervals = _splitSkillStaffInterval.Split(skillIntervals.ToList(), resolution);
			return splittedIntervals
				.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime)
				.ToList();
		}
	}
}
