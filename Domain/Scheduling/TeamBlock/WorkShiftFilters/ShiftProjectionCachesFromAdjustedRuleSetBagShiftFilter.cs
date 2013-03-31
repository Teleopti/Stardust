using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly scheduleDateOnly, IPerson person, bool forRestrictionsOnly, IEffectiveRestriction restriction);
	}

	public class ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter : IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter
	{
		private readonly IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
		private readonly IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
		private readonly IRuleSetToShiftsGenerator _ruleSetToShiftsGenerator;
		private readonly IDictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>> _ruleSetListDictionary = new Dictionary<IWorkShiftRuleSet, IList<IShiftProjectionCache>>();

		public ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter(IRuleSetDeletedActivityChecker ruleSetDeletedActivityChecker,
			IRuleSetDeletedShiftCategoryChecker rulesSetDeletedShiftCategoryChecker,
			IRuleSetToShiftsGenerator ruleSetToShiftsGenerator)
		{
			_ruleSetDeletedActivityChecker = ruleSetDeletedActivityChecker;
			_rulesSetDeletedShiftCategoryChecker = rulesSetDeletedShiftCategoryChecker;
			_ruleSetToShiftsGenerator = ruleSetToShiftsGenerator;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly scheduleDateOnly, IPerson person,  bool forRestrictionsOnly, IEffectiveRestriction restriction)
		{
			var shiftProjectionCaches = new List<IShiftProjectionCache>();
			if (person == null)
				return shiftProjectionCaches;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var personPeriod = person.Period(scheduleDateOnly);
			var bag = personPeriod.RuleSetBag;

			var ruleSets = bag.RuleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions == forRestrictionsOnly).ToList();

			foreach (IWorkShiftRuleSet ruleSet in ruleSets)
			{
				if (ruleSet.IsValidDate(scheduleDateOnly))
				{
					var clonedRuleSet = (IWorkShiftRuleSet)ruleSet.Clone();

					if (restriction != null)
					{
						var start = resolveTime(clonedRuleSet.TemplateGenerator.StartPeriod.Period.StartTime, restriction.StartTimeLimitation.StartTime, false);
						var end = resolveTime(clonedRuleSet.TemplateGenerator.StartPeriod.Period.EndTime, restriction.StartTimeLimitation.EndTime, true);
						if (start > end)
							continue;
						var startTimePeriod = new TimePeriod(start, end);

						start = resolveTime(clonedRuleSet.TemplateGenerator.EndPeriod.Period.StartTime, restriction.EndTimeLimitation.StartTime, false);
						end = resolveTime(clonedRuleSet.TemplateGenerator.EndPeriod.Period.EndTime, restriction.EndTimeLimitation.EndTime, true);
						if (start > end)
							continue;
						var endTimePeriod = new TimePeriod(start, end);

						if (endTimePeriod.EndTime < startTimePeriod.StartTime)
							continue;

						if (startTimePeriod.EndTime > endTimePeriod.EndTime)
							startTimePeriod = new TimePeriod(startTimePeriod.StartTime, endTimePeriod.EndTime);

						if (endTimePeriod.StartTime < startTimePeriod.StartTime)
							endTimePeriod = new TimePeriod(startTimePeriod.StartTime, endTimePeriod.EndTime);

						clonedRuleSet.TemplateGenerator.StartPeriod = new TimePeriodWithSegment(startTimePeriod, clonedRuleSet.TemplateGenerator.StartPeriod.Segment);
						clonedRuleSet.TemplateGenerator.EndPeriod = new TimePeriodWithSegment(endTimePeriod, clonedRuleSet.TemplateGenerator.EndPeriod.Segment);
					}

					if (!_ruleSetDeletedActivityChecker.ContainsDeletedActivity(clonedRuleSet) && !_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(clonedRuleSet))
					{
						IEnumerable<IShiftProjectionCache> ruleSetList = getShiftsForRuleset(clonedRuleSet);
						if (ruleSetList != null)
						{
							foreach (var projectionCache in ruleSetList)
							{
								shiftProjectionCaches.Add(projectionCache);
								projectionCache.SetDate(scheduleDateOnly, timeZone);
							}
						}
					}
				}
			}

			return shiftProjectionCaches;
		}

		private static TimeSpan resolveTime(TimeSpan thisTime, TimeSpan? otherTime, bool min)
		{
			if (!otherTime.HasValue)
				return thisTime;

			if (min)
				return TimeSpan.FromTicks(Math.Min(otherTime.Value.Ticks, thisTime.Ticks));

			return TimeSpan.FromTicks(Math.Max(otherTime.Value.Ticks, thisTime.Ticks));
		}

		private IEnumerable<IShiftProjectionCache> getShiftsForRuleset(IWorkShiftRuleSet ruleSet)
		{
			IList<IShiftProjectionCache> retList;

			if (!_ruleSetListDictionary.TryGetValue(ruleSet, out retList))
			{
				retList = _ruleSetToShiftsGenerator.Generate(ruleSet).ToList();
				_ruleSetListDictionary.Add(ruleSet, retList);
			}
			return retList;
		}
	}
}
