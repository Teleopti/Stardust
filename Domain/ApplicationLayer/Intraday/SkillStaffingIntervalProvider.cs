using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SkillStaffingIntervalProvider
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly MergeSkillStaffIntervalLightForSkillArea _mergeIntervals;

		public SkillStaffingIntervalProvider(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, SplitSkillStaffInterval splitSkillStaffInterval, ISkillAreaRepository skillAreaRepository, MergeSkillStaffIntervalLightForSkillArea mergeIntervals)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillAreaRepository = skillAreaRepository;
			_mergeIntervals = mergeIntervals;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkill(Guid skillId, DateTimePeriod period, TimeSpan resolution)
		{
			var skillIntervals = _scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skillId, period).ToList();
			return _splitSkillStaffInterval.Split(skillIntervals, resolution);
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkillArea(Guid id, DateTimePeriod period, TimeSpan resolution)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIntervals =
				_scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(
					skillArea.Skills.Select(x => x.Id).ToArray(), period);
			var splitedIntervals = _splitSkillStaffInterval.Split(skillIntervals.ToList(), resolution);
			return _mergeIntervals.Merge(splitedIntervals.ToList(), id);
		}
	}
}
