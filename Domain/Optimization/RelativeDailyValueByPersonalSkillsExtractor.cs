using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the daily intraday relative standard deviations filtered by the
    /// persons active skills on the given day.
    /// </summary>
    public class RelativeDailyValueByPersonalSkillsExtractor : IScheduleResultDailyValueCalculator
    {
        private readonly IAdvancedPreferences _advancedPreferences;
	    private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
	    private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
	    private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
	    private readonly PersonalSkillsProvider _personalSkillsProvider;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IUserTimeZone _userTimeZone;
	    private readonly IScheduleMatrixPro _scheduleMatrix;

	    public RelativeDailyValueByPersonalSkillsExtractor(IScheduleMatrixPro scheduleMatrix,
	                                                       IAdvancedPreferences advancedPreferences,
	                                                       ISkillStaffPeriodToSkillIntervalDataMapper
		                                                       skillStaffPeriodToSkillIntervalDataMapper,
	                                                       ISkillIntervalDataDivider skillIntervalDataDivider,
															ISkillIntervalDataAggregator skillIntervalDataAggregator,
															PersonalSkillsProvider personalSkillsProvider,
															ISchedulingResultStateHolder schedulingResultStateHolder,
															IUserTimeZone userTimeZone)
	    {
		    _scheduleMatrix = scheduleMatrix;
		    _advancedPreferences = advancedPreferences;
		    _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
		    _skillIntervalDataDivider = skillIntervalDataDivider;
		    _skillIntervalDataAggregator = skillIntervalDataAggregator;
		    _personalSkillsProvider = personalSkillsProvider;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		    _userTimeZone = userTimeZone;
	    }

	    public IList<double?> Values()
	    {
		    IList<double?> ret =
			    _scheduleMatrix.EffectivePeriodDays.Select(scheduleDayPro => DayValue(scheduleDayPro.Day)).ToList();
			
            return ret;
        }

        public double? DayValue(DateOnly scheduleDay)
        {
            IList<double> intradayRelativePersonnelDeficits =
                getIntradayRelativePersonnelDeficits(scheduleDay);

            double? result = null;

            if (intradayRelativePersonnelDeficits.Any())
            {
				switch (_advancedPreferences.TargetValueCalculation)
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
            IEnumerable<ISkill> personsActiveSkills = extractPersonalSkillList(scheduleDay).ToList();
			var minResolution = 15;
			if(personsActiveSkills.Any())
				minResolution = personsActiveSkills.Min(skill => skill.DefaultResolution);

            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
               scheduleDay.Date, scheduleDay.Date.AddDays(1),
							 _userTimeZone.TimeZone());

			IList<IList<ISkillIntervalData>> nestedList = new List<IList<ISkillIntervalData>>();

			foreach (var personsActiveSkill in personsActiveSkills)
			{
				IList<ISkillStaffPeriod> personsSkillStaffPeriods =
				_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new[] { personsActiveSkill }, dateTimePeriod);

				if (!personsSkillStaffPeriods.Any())
					continue;

				var skillIntervalDataList =
				_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(personsSkillStaffPeriods, scheduleDay, TimeZoneGuard.Instance.CurrentTimeZone());
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

        private IEnumerable<ISkill> extractPersonalSkillList(DateOnly scheduleDate)
        {
	        return _personalSkillsProvider.PersonSkills(_scheduleMatrix.Person.Period(scheduleDate)).Select(personSkill => personSkill.Skill).ToList();
        }
    }
}