﻿

using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface ICalculateMinMaxCacheDecider
	{
		bool ShouldCacheBeDisabled(ISchedulerStateHolder stateHolder, ISchedulingOptions schedulingOptions, IEffectiveRestrictionCreator effectiveRestrictionCreator, int cacheEntryLimit);
	}

	public class CalculateMinMaxCacheDecider : ICalculateMinMaxCacheDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ShouldCacheBeDisabled(ISchedulerStateHolder stateHolder, ISchedulingOptions schedulingOptions, IEffectiveRestrictionCreator effectiveRestrictionCreator, int cacheEntryLimit)
		{
			var persons = stateHolder.FilteredPersonDictionary.Values;
			var dates = stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection();
			var uniqueRestrictions = new HashSet<IEffectiveRestriction>();
			var uniqueBags = new HashSet<IRuleSetBag>();
			var uniqueRuleSets = new HashSet<IWorkShiftRuleSet>();
			var dic = stateHolder.Schedules;

			foreach (var person in persons)
			{
				var range = dic[person];
				foreach (var date in dates)
				{
					var scheduleDay = range.ScheduledDay(date);
					var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
					uniqueRestrictions.Add(effectiveRestriction);
					var personPeriod = person.Period(date);
					if (personPeriod == null)
						continue;
					
					var bag = personPeriod.RuleSetBag;
					if (bag == null)
						continue;

					uniqueBags.Add(bag);
				}
			}

			foreach (var ruleSetBag in uniqueBags)
			{
				foreach (var ruleSet in ruleSetBag.RuleSetCollection)
				{
					uniqueRuleSets.Add(ruleSet);
				}
			}

			if (uniqueRuleSets.Count * uniqueRestrictions.Count > cacheEntryLimit)
				return true;

			return false;
		}
	}
}