using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IEffectiveRestrictionStartTimeDecider
	{
		IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay);
	}
	
	public class EffectiveRestrictionStartTimeDecider : IEffectiveRestrictionStartTimeDecider
	{
		public IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay)
		{
			if (schedulingOptions.BreakPreferenceStartTimeByMax == TimeSpan.Zero) return effectiveRestriction;
			if (effectiveRestriction.IsPreferenceDay || !schedulingOptions.IsClassic()) return effectiveRestriction;
			var dataRestrictions = scheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>();
			var preference = (from r in dataRestrictions where r.Restriction != null select r.Restriction).FirstOrDefault();
			TimeSpan? start = null;
			TimeSpan? end = null;
			if (preference == null || !preference.StartTimeLimitation.HasValue()) return effectiveRestriction;
			if (preference.StartTimeLimitation.StartTime.HasValue) start = preference.StartTimeLimitation.StartTime.Value.Add(-schedulingOptions.BreakPreferenceStartTimeByMax);					
			if (preference.StartTimeLimitation.EndTime.HasValue) end = preference.StartTimeLimitation.EndTime.Value.Add(schedulingOptions.BreakPreferenceStartTimeByMax);		
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

	public class EffectiveRestrictionStartTimeDeciderOff : IEffectiveRestrictionStartTimeDecider
	{
		public IEffectiveRestriction Decide(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IScheduleDay scheduleDay)
		{
			return effectiveRestriction;
		}
	}
}