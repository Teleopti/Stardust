using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDailyValueByAllSkillsExtractor
	{
		double ValueForPeriod(DateOnlyPeriod period, TargetValueOptions targetValueOptions);
		IList<double?> Values(DateOnlyPeriod period, TargetValueOptions targetValueOptions);
		double? DayValue(DateOnly scheduleDay, TargetValueOptions targetValueOptions);
	}

	public class DailyValueByAllSkillsExtractor : IDailyValueByAllSkillsExtractor
	{
	    private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
	    private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
	    private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly ISchedulingResultStateHolder _stateholder;

		public DailyValueByAllSkillsExtractor(ISkillStaffPeriodToSkillIntervalDataMapper
		                                                       skillStaffPeriodToSkillIntervalDataMapper,
	                                                       ISkillIntervalDataDivider skillIntervalDataDivider,
															ISkillIntervalDataAggregator skillIntervalDataAggregator,
			ISchedulingResultStateHolder stateholder)
	    {
		    _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
		    _skillIntervalDataDivider = skillIntervalDataDivider;
		    _skillIntervalDataAggregator = skillIntervalDataAggregator;
			_stateholder = stateholder;
	    }

		public double ValueForPeriod(DateOnlyPeriod period, TargetValueOptions targetValueOptions)
		{
			double total = 0;
			foreach (var value in Values(period, targetValueOptions))
			{
				if(value.HasValue)
				total += value.Value;
			}

			return total;
		}

		public IList<double?> Values(DateOnlyPeriod period, TargetValueOptions targetValueOptions)
        {
            IList<double?> ret = new List<double?>();

            foreach (var dateOnly in period.DayCollection())
            {
                double? value = DayValue(dateOnly, targetValueOptions);
                ret.Add(value);
            }

            return ret;
        }

        public double? DayValue(DateOnly scheduleDay, TargetValueOptions targetValueOptions)
        {
            IList<double> intradayRelativePersonnelDeficits =
                getIntradayRelativePersonnelDeficits(scheduleDay);

            double? result = null;

            if (intradayRelativePersonnelDeficits.Any())
            {
				switch (targetValueOptions)
				{
					case TargetValueOptions.StandardDeviation:
						result = Calculation.Variances.StandardDeviation(intradayRelativePersonnelDeficits);
						break;

					case TargetValueOptions.RootMeanSquare:
						result = Calculation.Variances.RMS(intradayRelativePersonnelDeficits);
						break;

					case TargetValueOptions.Teleopti:
						result = Calculation.Variances.Teleopti(intradayRelativePersonnelDeficits);
						break;

				}

            }
            return result;
        }

        // todo: move to extractor methods
        private IList<double> getIntradayRelativePersonnelDeficits(DateOnly scheduleDay)
        {
			IEnumerable<ISkill> activeSkills = _stateholder.VisibleSkills;
			var minResolution = 15;
			if (activeSkills.Any())
				minResolution = activeSkills.Min(skill => skill.DefaultResolution);

            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
               scheduleDay.Date, scheduleDay.Date.AddDays(1),
               TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

			IList<IList<ISkillIntervalData>> nestedList = new List<IList<ISkillIntervalData>>();

			foreach (var skill in activeSkills)
			{
				IList<ISkillStaffPeriod> personsSkillStaffPeriods =
				_stateholder.SkillStaffPeriodHolder.SkillStaffPeriodList(new[] { skill }, dateTimePeriod);

				if (!personsSkillStaffPeriods.Any())
					continue;

				var skillIntervalDataList =
				_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(personsSkillStaffPeriods, scheduleDay, TimeZoneGuard.Instance.TimeZone);
				var splittedSkillIntervalDataList = _skillIntervalDataDivider.SplitSkillIntervalData(skillIntervalDataList,
																									  minResolution);
				nestedList.Add(splittedSkillIntervalDataList);
			}

			var aggregatedByPeriodSkillIntervalDataList =
				_skillIntervalDataAggregator.AggregateSkillIntervalData(nestedList);

			var resultingList = skillStaffPeriodsRelativeDifference(aggregatedByPeriodSkillIntervalDataList);
	        return resultingList;
        }

		private static IList<double> skillStaffPeriodsRelativeDifference(IEnumerable<ISkillIntervalData> skillIntervalDataList)
        {
			return skillIntervalDataList.Select(s => s.RelativeDifferenceBoosted()).ToList();
        }

	}
}