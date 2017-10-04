using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class EffectiveRestrictionStartTimeDecider : IEffectiveRestrictionStartTimeDecider
	{
		public IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay)
		{
			if (schedulingOptions.BreakPreferenceStartTimeByMax == TimeSpan.Zero) return effectiveRestriction;
			if (effectiveRestriction.IsPreferenceDay || !schedulingOptions.IsClassic()) return effectiveRestriction;
			TimeSpan? start = null;
			TimeSpan? end = null;
			var dataRestrictions = scheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>();
			var preference = (from r in dataRestrictions where r.Restriction != null select r.Restriction).FirstOrDefault();
			if (preference == null || !preference.StartTimeLimitation.HasValue() && preference.ShiftCategory == null) return effectiveRestriction;
			if (preference.StartTimeLimitation.StartTime.HasValue) start = preference.StartTimeLimitation.StartTime.Value.Add(-schedulingOptions.BreakPreferenceStartTimeByMax);					
			if (preference.StartTimeLimitation.EndTime.HasValue) end = preference.StartTimeLimitation.EndTime.Value.Add(schedulingOptions.BreakPreferenceStartTimeByMax);		
			if (preference.ShiftCategory != null)
			{
				var ruleSetBag = scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly)?.RuleSetBag;
				if (ruleSetBag != null)
				{	
					foreach (var ruleSet in ruleSetBag.RuleSetCollection)
					{
						if(!ruleSet.TemplateGenerator.Category.Equals(preference.ShiftCategory)) continue;
						var rulesetStartPeriod = ruleSet.TemplateGenerator.StartPeriod.Period;
						var adjustedRuleSetStart = rulesetStartPeriod.StartTime.Add(-schedulingOptions.BreakPreferenceStartTimeByMax);
						var adjustedRuleSetEnd = rulesetStartPeriod.EndTime.Add(schedulingOptions.BreakPreferenceStartTimeByMax);
						if (!start.HasValue || adjustedRuleSetStart < start) start = adjustedRuleSetStart;
						if (!end.HasValue || adjustedRuleSetEnd > end) end = adjustedRuleSetEnd;
					}
				}
			}
			var adjustedStartTimeRestriction = new EffectiveRestriction(
				new StartTimeLimitation(start, end), 
				new EndTimeLimitation(), 
				new WorkTimeLimitation(), 
				null,
				null,
				null,
				new List<IActivityRestriction>());

			return adjustedStartTimeRestriction;
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002)]
	public interface IEffectiveRestrictionStartTimeDecider
	{
		IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay);
	}
	

	[RemoveMeWithToggle(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002)]
	public class EffectiveRestrictionStartTimeDeciderOff : IEffectiveRestrictionStartTimeDecider
	{
		public IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay)
		{
			return effectiveRestriction;
		}
	}
}