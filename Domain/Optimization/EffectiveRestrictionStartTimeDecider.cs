using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class EffectiveRestrictionStartTimeDecider : IEffectiveRestrictionStartTimeDecider
	{
		//agent's missing personperiod
		//Agent missing rulesetbag
		//kolla effectiveRestriction.IsPreferenceDay?
		
		public IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay)
		{
			var restriction = scheduleDay.PreferenceDay()?.Restriction;
			if (jumpOutEarly(schedulingOptions, restriction)) 
				return effectiveRestriction;

			var start = restriction.StartTimeLimitation.StartTime?.Add(-schedulingOptions.BreakPreferenceStartTimeByMax);
			var end = restriction.StartTimeLimitation.EndTime?.Add(schedulingOptions.BreakPreferenceStartTimeByMax);
			foreach (var ruleSet in scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly).RuleSetBag
				.RuleSetCollection.Where(x => x.TemplateGenerator.Category.Equals(restriction.ShiftCategory)))
			{
				start = TimeSpanExtensions.TakeMin(start, ruleSet.TemplateGenerator.StartPeriod.Period.StartTime.Add(-schedulingOptions.BreakPreferenceStartTimeByMax));
				end = TimeSpanExtensions.TakeMax(end, ruleSet.TemplateGenerator.StartPeriod.Period.EndTime.Add(schedulingOptions.BreakPreferenceStartTimeByMax));
			}
			return new EffectiveRestriction(
				new StartTimeLimitation(start, end), 
				new EndTimeLimitation(), 
				new WorkTimeLimitation(), 
				null,
				null,
				null,
				new List<IActivityRestriction>());
		}

		private static bool jumpOutEarly(SchedulingOptions schedulingOptions, IPreferenceRestriction restriction)
		{
			return schedulingOptions.BreakPreferenceStartTimeByMax == TimeSpan.Zero || !schedulingOptions.IsClassic() ||
				   restriction == null || !restriction.StartTimeLimitation.HasValue();
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