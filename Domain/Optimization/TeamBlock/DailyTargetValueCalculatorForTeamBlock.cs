﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface IDailyTargetValueCalculatorForTeamBlock
    {
        double TargetValue(ITeamBlockInfo teamBlockInfo , IAdvancedPreferences advancedPreferences);
    }

    public class DailyTargetValueCalculatorForTeamBlock : IDailyTargetValueCalculatorForTeamBlock
    {
        //private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
        private readonly ISkillResolutionProvider _resolutionProvider;
        private readonly ISkillIntervalDataDivider _intervalDataDivider;
        private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
        private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
        private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
        private readonly ILocateMissingIntervalsIfMidNightBreak _locateMissingIntervalsIfMidNightBreak;
        private readonly IFilterOutIntervalsAfterMidNight _filterOutIntervalsAfterMidNight;

        public DailyTargetValueCalculatorForTeamBlock(ISkillResolutionProvider resolutionProvider, ISkillIntervalDataDivider intervalDataDivider, 
                                ISkillIntervalDataAggregator intervalDataAggregator, IDayIntervalDataCalculator dayIntervalDataCalculator, ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
                            ISchedulingResultStateHolder schedulingResultStateHolder, IGroupPersonSkillAggregator groupPersonSkillAggregator,ILocateMissingIntervalsIfMidNightBreak locateMissingIntervalsIfMidNightBreak, IFilterOutIntervalsAfterMidNight filterOutIntervalsAfterMidNight)
        {
            //_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
            _resolutionProvider = resolutionProvider;
            _intervalDataDivider = intervalDataDivider;
            _intervalDataAggregator = intervalDataAggregator;
            _dayIntervalDataCalculator = dayIntervalDataCalculator;
            _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _groupPersonSkillAggregator = groupPersonSkillAggregator;
            _locateMissingIntervalsIfMidNightBreak = locateMissingIntervalsIfMidNightBreak;
            _filterOutIntervalsAfterMidNight = filterOutIntervalsAfterMidNight;
        }

        public double TargetValue(ITeamBlockInfo teamBlockInfo , IAdvancedPreferences advancedPreferences)
        {
            var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupMembers, dateOnlyPeriod).ToList();
            var minimumResolution = _resolutionProvider.MinimumResolution(skills);

            var skillIntervalPerDayList = getSkillIntervalListForEachDay(dateOnlyList,skills ,minimumResolution );
            var finalSkillIntervalData = calculateMedianValue(skillIntervalPerDayList);
            return  getTargetValue(finalSkillIntervalData, advancedPreferences.TargetValueCalculation);
        }

        private double getTargetValue(IList<ISkillIntervalData> finalSkillIntervalData, TargetValueOptions targetValueCalculation)
        {
            if (targetValueCalculation == TargetValueOptions.RootMeanSquare)
            {
                var aggregatedValues = finalSkillIntervalData.Select(interval => interval.AbsoluteDifference).ToList();
                return Calculation.Variances.RMS(aggregatedValues);
            }
            else
            {
                var aggregatedValues = finalSkillIntervalData.Select(interval => interval.RelativeDifference()).ToList();
                if (targetValueCalculation == TargetValueOptions.StandardDeviation)
                    return Calculation.Variances.StandardDeviation(aggregatedValues);
                return Calculation.Variances.Teleopti(aggregatedValues);
            }
            
        }

        private IList<ISkillIntervalData> calculateMedianValue(IDictionary<DateOnly, IList<ISkillIntervalData>> skillIntervalPerDayList)
        {
            var intervalData = _dayIntervalDataCalculator.Calculate(skillIntervalPerDayList);
            return intervalData.Values.ToList();
        }

        private IDictionary<DateOnly, IList<ISkillIntervalData>> getSkillIntervalListForEachDay(IList<DateOnly> dateOnlyList, IList<ISkill> skills, int minimumResolution)
        {
            var result = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            
            
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            
            foreach (var skillDay in skillDays)
            {
                var currentDate = skillDay.CurrentDate;
                var skill = skillDay.Skill;
                if (skill != null && !skills.Contains(skill)) continue ;
                if (skillDay.SkillStaffPeriodCollection.Count == 0) continue ;
                IList<ISkillStaffPeriod> skillStaffPeriodCollection = skillDay.SkillStaffPeriodCollection;
                if (skill.MidnightBreakOffset != TimeSpan.Zero)
                {
                    var missingIntervals = _locateMissingIntervalsIfMidNightBreak.GetMissingSkillStaffPeriods(currentDate, skill, TimeZoneGuard.Instance.TimeZone );
                    skillStaffPeriodCollection = skillStaffPeriodCollection.Concat(missingIntervals).ToList()  ;
                    skillStaffPeriodCollection = _filterOutIntervalsAfterMidNight.Filter(skillStaffPeriodCollection, currentDate, TimeZoneGuard.Instance.TimeZone);
                }

				var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriodCollection, currentDate, TimeZoneGuard.Instance.TimeZone);
                mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);

                if(!result.ContainsKey(currentDate ))
                    result.Add(currentDate, mappedData);
                else
                {
                    var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>> { result[currentDate], mappedData });
                    result[currentDate] = dayIntervalData;
                }
        
            }

           

            return result;
        }
        
	
    }
}
