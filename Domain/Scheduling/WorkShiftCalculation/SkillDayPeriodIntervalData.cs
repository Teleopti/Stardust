using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public class SkillDayPeriodIntervalData : ISkillDayPeriodIntervalData
    {
    	private readonly ISkillIntervalDataSkillFactorApplyer _skillIntervalDataSkillFactorApplyer;
    	private readonly IIntervalDataCalculator _intervalDataMedianCalculator;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public SkillDayPeriodIntervalData()
        {
            
        }

		public SkillDayPeriodIntervalData(ISkillIntervalDataSkillFactorApplyer skillIntervalDataSkillFactorApplyer, IIntervalDataCalculator intervalDataMedianCalculator, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
			_skillIntervalDataSkillFactorApplyer = skillIntervalDataSkillFactorApplyer;
			_intervalDataMedianCalculator = intervalDataMedianCalculator;
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

        //return a dictionary that has activity as a key and the value as the existing dic
		public IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> GetIntervalDistribution(List<DateOnly> dateOnlyList)
        {
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
        	var skills = skillDays.Select(x => x.Skill).Distinct().ToList();
        	var activities = skills.Select(x => x.Activity).Distinct().ToList();
			
        	var skillSkillIntervalData = new Dictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>>();
        	foreach (var skill in skills)
        	{
        		var skillKey = skill;
				var skillDaysWithSkill = skillDays.Where(x => x.Skill.Equals(skillKey)).ToList();
				var intervalDataOfOneSkill = getIntervalDataOfOneSkill(skill, skillDaysWithSkill);
        		skillSkillIntervalData.Add(skill, intervalDataOfOneSkill);
        	}

			var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();

			foreach (var activity in activities)
			{
				IList<ISkill> oldSkills = new List<ISkill>();
				IDictionary<TimeSpan, ISkillIntervalData> skillIntervalData = null;
				foreach (KeyValuePair<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> pair in skillSkillIntervalData)
				{
					ISkill skill = pair.Key;
					if (skill.Activity == activity)
					{
						if (oldSkills.Contains(skill)) break;

						if (skillIntervalData == null)
							skillIntervalData = pair.Value;

						oldSkills.Add(skill);
						var nextSkill = findNextSkillWithSameActivity(activity, oldSkills, skillSkillIntervalData);
						if (nextSkill != null)
						{
							var nextSkillData = skillSkillIntervalData[nextSkill];
							skillIntervalData = combineSkillIntervalData(skillIntervalData, nextSkillData);
						}
					}
				}
				if (skillIntervalData != null)
				{
					activityIntervalData.Add(activity, skillIntervalData);
				}
			}

			return activityIntervalData;
        }

		private static ISkill findNextSkillWithSameActivity(IActivity activity, ICollection<ISkill> oldSkills, IEnumerable<KeyValuePair<ISkill, IDictionary<TimeSpan, ISkillIntervalData>>> skills)
		{
			foreach (var skill in skills)
			{
				if (skill.Key.Activity == activity && !oldSkills.Contains(skill.Key))
					return skill.Key;
			}
			return null;
		}

		private static IDictionary<TimeSpan, ISkillIntervalData> combineSkillIntervalData(IDictionary<TimeSpan, ISkillIntervalData> dictionaryTo, IDictionary<TimeSpan, ISkillIntervalData> dictionaryFrom)
		{
			dictionaryTo = controlIfPeriodIsMissing(dictionaryTo, dictionaryFrom);
			foreach (KeyValuePair<TimeSpan, ISkillIntervalData> valuePair in dictionaryTo)
			{
				ISkillIntervalData skillIntervalData;
				if (dictionaryFrom.TryGetValue(valuePair.Key, out skillIntervalData))
				{
					valuePair.Value.ForecastedDemand += skillIntervalData.ForecastedDemand;
				}
			}
			return dictionaryTo;
		}

		private static IDictionary<TimeSpan, ISkillIntervalData> controlIfPeriodIsMissing(IDictionary<TimeSpan, ISkillIntervalData> dictionaryToControl,
			IDictionary<TimeSpan, ISkillIntervalData> theOtherDictionary)
		{
			foreach (KeyValuePair<TimeSpan, ISkillIntervalData> pair in theOtherDictionary)
			{
				if (!dictionaryToControl.ContainsKey(pair.Key))
				{
					var intervalData = new SkillIntervalData(pair.Value.Period, 0, 0, 0, 0, 0);
					dictionaryToControl.Add(pair.Key, intervalData);
				}
			}

			return dictionaryToControl;
		}

    	private Dictionary<TimeSpan, ISkillIntervalData> getIntervalDataOfOneSkill(ISkill skill, IList<ISkillDay> skillDays)
    	{
    		var intervalBasedData = new Dictionary<TimeSpan, List<double>>();
    		var firstDay = skillDays.FirstOrDefault();
    		if (firstDay == null) throw new ArgumentException("skillDays are empty");
    		var resolution = firstDay.Skill.DefaultResolution;
    		for (var i = 0; i < TimeSpan.FromDays(2).TotalMinutes/resolution; i++)
    		{
    			intervalBasedData.Add(TimeSpan.FromMinutes(i*resolution), new List<double>());
    		}
    		foreach (var period in skillDays.SelectMany(s => s.SkillStaffPeriodCollection))
    		{
    			var periodTimeSpan = period.Period.StartDateTime.TimeOfDay;
    			if (intervalBasedData.ContainsKey(periodTimeSpan))
    			{
    				var invervalData = intervalBasedData[periodTimeSpan];
    				invervalData.Add(period.AbsoluteDifference);
    			}
    		}
    		var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
    		var result = new Dictionary<TimeSpan, ISkillIntervalData>();
    		foreach (var intervalPair in intervalBasedData)
    		{
    			var startTime = baseDate.Date.AddMinutes(intervalPair.Key.TotalMinutes);
    			var endTime = startTime.AddMinutes(resolution);
    			var calculatedDemand = _intervalDataMedianCalculator.Calculate(intervalPair.Value);
    			ISkillIntervalData skillIntervalData = new SkillIntervalData(new DateTimePeriod(startTime, endTime), calculatedDemand, 0, 0, 0, 0);
				skillIntervalData = _skillIntervalDataSkillFactorApplyer.ApplyFactors(skillIntervalData, skill);
    			result.Add(intervalPair.Key, skillIntervalData);
    		}
    		return result;
    	}
    }

    public interface ISkillDayPeriodIntervalData
    {
		IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> GetIntervalDistribution(List<DateOnly> dateOnlyList);
    }
}