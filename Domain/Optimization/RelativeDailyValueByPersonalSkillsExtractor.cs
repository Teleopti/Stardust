using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
	    private readonly IScheduleMatrixPro _scheduleMatrix;

	    public RelativeDailyValueByPersonalSkillsExtractor(IScheduleMatrixPro scheduleMatrix,
	                                                       IAdvancedPreferences advancedPreferences,
	                                                       ISkillStaffPeriodToSkillIntervalDataMapper
		                                                       skillStaffPeriodToSkillIntervalDataMapper,
	                                                       ISkillIntervalDataDivider skillIntervalDataDivider,
															ISkillIntervalDataAggregator skillIntervalDataAggregator)
	    {
		    _scheduleMatrix = scheduleMatrix;
		    _advancedPreferences = advancedPreferences;
		    _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
		    _skillIntervalDataDivider = skillIntervalDataDivider;
		    _skillIntervalDataAggregator = skillIntervalDataAggregator;
	    }

	    public IList<double?> Values()
        {
            IList<double?> ret = new List<double?>();

            foreach (IScheduleDayPro scheduleDayPro in _scheduleMatrix.EffectivePeriodDays)
            {
                double? value = DayValue(scheduleDayPro.Day);
                ret.Add(value);
            }

            return ret;
        }

        public double? DayValue(DateOnly scheduleDay)
        {
            IList<double> intradayRelativePersonnelDeficits =
                GetIntradayRelativePersonnelDeficits(scheduleDay);

            IPopulationStatisticsCalculator calculator = new PopulationStatisticsCalculator();

            double? result = null;

            foreach (double personnelDeficit in intradayRelativePersonnelDeficits)
            {
                calculator.AddItem(personnelDeficit);
            }
            if (calculator.Count > 0)
            {
                calculator.Analyze();
				switch (_advancedPreferences.TargetValueCalculation)
				{
					case TargetValueOptions.StandardDeviation:
						result = calculator.StandardDeviation;
						break;

					case TargetValueOptions.RootMeanSquare:
						result = calculator.RootMeanSquare;
						break;

					case TargetValueOptions.Teleopti:
						result = calculator.Teleopti;
						break;

				}

            }
            return result;
        }

        // todo: move to extractor methods
        private IList<double> GetIntradayRelativePersonnelDeficits(DateOnly scheduleDay)
        {
            IList<ISkill> personsActiveSkills = extractPersonalSkillList(scheduleDay);
	        var minResolution = personsActiveSkills.Min(skill => skill.DefaultResolution);

            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
               scheduleDay.Date, scheduleDay.Date.AddDays(1),
               TeleoptiPrincipal.Current.Regional.TimeZone);

			IList<IList<ISkillIntervalData>> nestedList = new List<IList<ISkillIntervalData>>();
			foreach (var personsActiveSkill in personsActiveSkills)
	        {
				IList<ISkillStaffPeriod> personsSkillStaffPeriods =
				_scheduleMatrix.SchedulingStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new[] { personsActiveSkill }, dateTimePeriod);

				if(!personsSkillStaffPeriods.Any())
					continue;

				var skillIntervalDataList =
				_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(personsSkillStaffPeriods);

				var splittedSkillIntervalDataList = _skillIntervalDataDivider.SplitSkillIntervalData(skillIntervalDataList,
																									  minResolution);
				nestedList.Add(splittedSkillIntervalDataList);
	        }

	        var aggregatedByPeriodSkillIntevalDataList =
				_skillIntervalDataAggregator.AggregateSkillIntervalData(nestedList);

            bool useMinPersonnel = _advancedPreferences.UseMinimumStaffing;
            bool useMaxPersonnel = _advancedPreferences.UseMaximumStaffing;

			return skillStaffPeriodsRelativeDifference(aggregatedByPeriodSkillIntevalDataList, useMinPersonnel, useMaxPersonnel);
        }

        private static IList<double> skillStaffPeriodsRelativeDifference(IEnumerable<ISkillIntervalData> skillIntervalDataList, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if (considerMinStaffing && considerMaxStaffing)
				return skillIntervalDataList.Select(s => s.RelativeDifferenceBoosted()).ToList();
            if (considerMinStaffing)
				return skillIntervalDataList.Select(s => s.RelativeDifferenceMinStaffBoosted()).ToList();
            if (considerMaxStaffing)
				return skillIntervalDataList.Select(s => s.RelativeDifferenceMaxStaffBoosted()).ToList();
			return skillIntervalDataList.Select(s => s.RelativeDifference()).ToList();
        }

        //todo: move to extractor methods
        private IList<ISkill> extractPersonalSkillList(DateOnly scheduleDate)
        {
            IList<IPersonSkill> personalSkills = _scheduleMatrix.Person.Period(scheduleDate).PersonSkillCollection;
            IList<ISkill> skillList = new List<ISkill>();
            foreach (IPersonSkill personalSkill in personalSkills)
            {
				if(((IDeleteTag)personalSkill.Skill).IsDeleted)
					continue;
	            
                skillList.Add(personalSkill.Skill);
            }
            return skillList;
        }
    }
}