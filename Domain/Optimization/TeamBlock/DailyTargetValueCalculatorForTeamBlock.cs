using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface IDailyTargetValueCalculatorForTeamBlock
    {
        double TargetValue(ITeamBlockInfo teamBlockInfo , IAdvancedPreferences advancedPreferences);
    }

    public class DailyTargetValueCalculatorForTeamBlock : IDailyTargetValueCalculatorForTeamBlock
    {
        private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
        private readonly ISkillResolutionProvider _resolutionProvider;
        private readonly ISkillIntervalDataDivider _intervalDataDivider;
        private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
        private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
        private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

        public DailyTargetValueCalculatorForTeamBlock(ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier, ISkillResolutionProvider resolutionProvider, ISkillIntervalDataDivider intervalDataDivider, ISkillIntervalDataAggregator intervalDataAggregator, IDayIntervalDataCalculator dayIntervalDataCalculator, ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper, ISchedulingResultStateHolder schedulingResultStateHolder, IGroupPersonSkillAggregator groupPersonSkillAggregator)
        {
            _skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
            _resolutionProvider = resolutionProvider;
            _intervalDataDivider = intervalDataDivider;
            _intervalDataAggregator = intervalDataAggregator;
            _dayIntervalDataCalculator = dayIntervalDataCalculator;
            _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _groupPersonSkillAggregator = groupPersonSkillAggregator;
        }

        public double TargetValue(ITeamBlockInfo teamBlockInfo , IAdvancedPreferences advancedPreferences)
        {
            var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
            var skills = _groupPersonSkillAggregator.AggregatedSkills(groupPerson, dateOnlyPeriod).ToList() ;
            var minimumResolution = _resolutionProvider.MinimumResolution(skills);

            var skillIntervalPerDayList = getSkillIntervalListForEachDay(dateOnlyList,skills ,minimumResolution );

            var aggregatedValues = mapToDoubleList(calculateMedianValue(skillIntervalPerDayList, minimumResolution));
            
            ITeamBlockTargetValueCalculator calculator = new TeamBlockTargetValueCalculator();

            foreach (double personnelDeficit in aggregatedValues)
            {
                calculator.AddItem(personnelDeficit);
            }
            if (calculator.Count > 0)
            {
                calculator.Analyze();
                switch (advancedPreferences.TargetValueCalculation)
                {
                    case TargetValueOptions.StandardDeviation:
                        return calculator.StandardDeviation;
                    case TargetValueOptions.RootMeanSquare:
                         return calculator.RootMeanSquare;
                    case TargetValueOptions.Teleopti:
                        return calculator.Teleopti;
                }

            }
            return 0;
        }

        private IEnumerable<double> mapToDoubleList(IList<ISkillIntervalData> skillIntervalDatas)
        {
            return skillIntervalDatas.Select(interval => Math.Abs(interval.CurrentDemand)).ToList();
        }

        private IList<ISkillIntervalData> calculateMedianValue(IDictionary<DateOnly, IList<ISkillIntervalData>> skillIntervalPerDayList, int minimumResolution)
        {
            var intervalData = _dayIntervalDataCalculator.Calculate(minimumResolution, skillIntervalPerDayList);
            return intervalData.Values.ToList();
        }

        private IDictionary<DateOnly, IList<ISkillIntervalData>> getSkillIntervalListForEachDay( IList<DateOnly> dateOnlyList, IList<ISkill > skills, int minimumResolution)
        {
            var result = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            
            
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            
            foreach (var skillDay in skillDays)
            {
                var currentDate = skillDay.CurrentDate;
                var skill = skillDay.Skill;
                if (skill != null && !skills.Contains(skill)) continue ;
                if (skillDay.SkillStaffPeriodCollection.Count == 0) continue ;
                var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection);
                mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
                var adjustedMapedData = new List<ISkillIntervalData>();
                foreach (var data in mappedData)
                {
                    var appliedData = _skillIntervalDataSkillFactorApplier.ApplyFactors(data, skill);
                    adjustedMapedData.Add(appliedData);
                }
                var aggregatedSkillsIntervals = new List<IList<ISkillIntervalData>> { adjustedMapedData, mappedData };
                var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(aggregatedSkillsIntervals);
                result.Add(currentDate, dayIntervalData);
        
            }
            return result;
        }

        


        
	
    }
}
