using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillDayPeriodIntervalDataGenerator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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
	    private readonly IOpenHourRestrictionForTeamBlock _openHourRestrictionForTeamBlock;

	    public SkillDayPeriodIntervalDataGenerator(ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillResolutionProvider resolutionProvider,
			ISkillIntervalDataDivider intervalDataDivider,
			ISkillIntervalDataAggregator intervalDataAggregator,
			IDayIntervalDataCalculator dayIntervalDataCalculator,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IGroupPersonSkillAggregator groupPersonSkillAggregator, IOpenHourRestrictionForTeamBlock openHourRestrictionForTeamBlock )
		{
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_resolutionProvider = resolutionProvider;
			_intervalDataDivider = intervalDataDivider;
			_intervalDataAggregator = intervalDataAggregator;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		    _openHourRestrictionForTeamBlock = openHourRestrictionForTeamBlock;
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
		    var openHoursPerActivity = _openHourRestrictionForTeamBlock.GetOpenHoursPerActivity(teamBlockInfo);
            var minimumResolution = _resolutionProvider.MinimumResolution(skills);
            var activityData = new Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>>();
            for (var i = 0; i < skillDays.Count; i++)
            {
                ISkillDay skillDayToday;
                ISkillDay skillDayTomorrow;
                getTwoConsectiveDays(i, out skillDayToday, out skillDayTomorrow, skillDays);

                aggregateDataOnSkill(skillDayToday, skillDayTomorrow, skills, minimumResolution, activityData, openHoursPerActivity);
            }

            foreach (var activityBasedData in activityData)
            {
                var intervalData = _dayIntervalDataCalculator.Calculate(minimumResolution, activityBasedData.Value);
                activityIntervalData.Add(activityBasedData.Key, intervalData);
            }

            return activityIntervalData;
        }
        private void getTwoConsectiveDays(int index, out ISkillDay today, out ISkillDay tomorrow, IList<ISkillDay> skillDays)
        {
            tomorrow = new SkillDay();
            today = skillDays[index];
            if (index + 1 < skillDays.Count)
                tomorrow = skillDays[index + 1];
            if (today.Skill != tomorrow.Skill)
                tomorrow = new SkillDay();
        }

        private IList<ISkillStaffPeriod> getSkillIntervalBetweenOpenHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriodCollection, TimePeriod openHoursPerActivity)
        {
	        return
		        skillStaffPeriodCollection.Where(
			        o =>
			        openHoursPerActivity.Contains(new TimePeriod(o.Period.StartDateTime.TimeOfDay,
			                                                     o.Period.EndDateTime.Date > o.Period.StartDateTime.Date
				                                                     ? o.Period.EndDateTime.TimeOfDay.Add(TimeSpan.FromDays(1))
				                                                     : o.Period.EndDateTime.TimeOfDay))).ToList();
        }

        private void aggregateDataOnSkill(ISkillDay skillDayToday, ISkillDay skillDayTomorrow, 
                                        List<ISkill> skills, int minimumResolution, Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>> activityData, 
                                        IDictionary<IActivity, TimePeriod> openHoursPerActivity)
	    {
            var currentDate = skillDayToday.CurrentDate;
            var skill = skillDayToday.Skill;
	        if (!skills.Contains(skill)) return;
	        var activity = skill.Activity;
            if (skillDayToday.SkillStaffPeriodCollection.Count == 0) return;
            var skillIntervalBetweenOpenHour = getSkillIntervalBetweenOpenHours(skillDayToday.SkillStaffPeriodCollection, openHoursPerActivity[activity ] );
            List<ISkillIntervalData> mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillIntervalBetweenOpenHour).ToList();
            if (skillDayTomorrow.Id != null && openHoursPerActivity[activity ].EndTime > TimeSpan.FromDays(1))
            {
                skillIntervalBetweenOpenHour = getSkillIntervalBetweenOpenHours(skillDayTomorrow.SkillStaffPeriodCollection, openHoursPerActivity[activity]);
                mappedData.AddRange(
                    _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillIntervalBetweenOpenHour)
                                                              .ToList());
            }
                
            mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution).ToList();
	        var adjustedMapedData = new List<ISkillIntervalData>();
	        foreach (var data in mappedData)
	        {
	            var appliedData = _skillIntervalDataSkillFactorApplier.ApplyFactors(data, skill);
	            //TODO shall we remove it or not??? ASAD 
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