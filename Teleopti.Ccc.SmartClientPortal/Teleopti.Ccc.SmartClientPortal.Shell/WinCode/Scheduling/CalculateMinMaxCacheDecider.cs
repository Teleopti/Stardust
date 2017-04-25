using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface ICalculateMinMaxCacheDecider
	{
		bool ShouldCacheBeDisabled(ISchedulerStateHolder stateHolder, SchedulingOptions schedulingOptions, IEffectiveRestrictionCreator effectiveRestrictionCreator, int cacheEntryLimit);
	}

	public class CalculateMinMaxCacheDecider : ICalculateMinMaxCacheDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ShouldCacheBeDisabled(ISchedulerStateHolder stateHolder, SchedulingOptions schedulingOptions, IEffectiveRestrictionCreator effectiveRestrictionCreator, int cacheEntryLimit)
		{
			var persons = stateHolder.FilteredCombinedAgentsDictionary.Values;
			var uniqueRestrictions = new HashSet<IEffectiveRestriction>();
			var uniqueBags = new HashSet<IRuleSetBag>();
			var uniqueRuleSets = new HashSet<IWorkShiftRuleSet>();
			var dic = stateHolder.Schedules;

			foreach (var person in persons)
			{
				var range = dic[person];
				var scheduleDays = range.ScheduledDayCollection(stateHolder.RequestedPeriod.DateOnlyPeriod);
				foreach (var scheduleDay in scheduleDays)
				{
					var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
					uniqueRestrictions.Add(effectiveRestriction);
					var personPeriod = person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly);
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