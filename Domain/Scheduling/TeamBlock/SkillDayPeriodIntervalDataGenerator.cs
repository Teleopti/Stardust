using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillDayPeriodIntervalDataGenerator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        //IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> Generate(ITeamBlockInfo teamBlockInfo);

        IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> GeneratePerDay(ITeamBlockInfo teamBlockInfo);
	}

	public class SkillDayPeriodIntervalDataGenerator : ISkillDayPeriodIntervalDataGenerator
	{
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly ISkillResolutionProvider _resolutionProvider;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public SkillDayPeriodIntervalDataGenerator(ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillResolutionProvider resolutionProvider,
			ISkillIntervalDataDivider intervalDataDivider,
			ISkillIntervalDataAggregator intervalDataAggregator,
			IDayIntervalDataCalculator dayIntervalDataCalculator,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
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



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> GeneratePerDay(ITeamBlockInfo teamBlockInfo )
        {
            var groupPerson =teamBlockInfo.TeamInfo .GroupPerson;
            var dateOnlyList =teamBlockInfo.BlockInfo .BlockPeriod.DayCollection();
            var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
            var skills = _groupPersonSkillAggregator.AggregatedSkills(groupPerson, dateOnlyPeriod).ToList();

            var minimumResolution = _resolutionProvider.MinimumResolution(skills);
            var activityData = new Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>>();
            for(var i= 0;i<skillDays.Count ;i++)
            {
                ISkillDay skillDayToday;
                ISkillDay skillDayTomorrow;
                getTwoConsectiveDays(i, out skillDayToday, out skillDayTomorrow,skillDays);

                aggregateDataOnSkill(skillDayToday, skillDayTomorrow, skills, minimumResolution, activityData);
            }

            foreach (var activityBasedData in activityData)
            {
                var intervalData = _dayIntervalDataCalculator.CalculatePerfect(minimumResolution, activityBasedData.Value);
                activityIntervalData.Add(activityBasedData.Key, intervalData);
            }

            return activityIntervalData;
        }

        private void getTwoConsectiveDays(int index, out ISkillDay today, out ISkillDay tomorrow, IList<ISkillDay  > skillDays   )
        {
            tomorrow = new SkillDay();
            today = skillDays[index];
            if (index + 1 < skillDays.Count)
                tomorrow = skillDays[index + 1];
            if (today.Skill != tomorrow.Skill)
                tomorrow = new SkillDay();
        }

	    private void aggregateDataOnSkill(ISkillDay skillDayToday, ISkillDay skillDayTomorrow, List<ISkill> skills, int minimumResolution, Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>> activityData)
	    {
	        var currentDate = skillDayToday.CurrentDate;
	        var skill = skillDayToday.Skill;
	        if (!skills.Contains(skill)) return;
	        var activity = skill.Activity;
	        if (skillDayToday.SkillStaffPeriodCollection.Count == 0) return;
	        
            List<ISkillIntervalData> mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDayToday.SkillStaffPeriodCollection).ToList() ;
            if (skillDayTomorrow.Id != null)
                mappedData.AddRange(
	                _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDayTomorrow.SkillStaffPeriodCollection)
	                                                          .ToList());
	        mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution).ToList()  ;
	        var adjustedMapedData = new List<ISkillIntervalData>();
	        foreach (var data in mappedData)
	        {
	            var appliedData = _skillIntervalDataSkillFactorApplier.ApplyFactors(data, skill);
	            adjustedMapedData.Add(appliedData);
	        }
	        IDictionary<DateOnly, IList<ISkillIntervalData>> intervalData;
	        activityData.TryGetValue(activity, out intervalData);
	        if (intervalData == null)
	        {
	            var dayIntevalData = new Dictionary<DateOnly, IList<ISkillIntervalData>> {{currentDate, adjustedMapedData}};
	            activityData.Add(activity, dayIntevalData);
	        }
	        else
	        {
	            IList<ISkillIntervalData> skillIntervalData;
	            activityData[activity].TryGetValue(currentDate, out skillIntervalData);
	            if (skillIntervalData == null)
	            {
	                activityData[activity].Add(currentDate, adjustedMapedData);
	            }
	            else
	            {
	                var data = new List<IList<ISkillIntervalData>> {adjustedMapedData, skillIntervalData};
	                var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(data);
	                activityData[activity][currentDate] = dayIntervalData;
	            }
	        }
	    }
	}
}