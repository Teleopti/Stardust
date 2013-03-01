﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface IActivityRestrictionsShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
	}

	public class ActivityRestrictionsShiftFilter : IActivityRestrictionsShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
		{
			IList<IActivityRestriction> activityRestrictions = restriction.ActivityRestrictionCollection;
			if (activityRestrictions.Count == 0)
				return shiftList;

			IList<IShiftProjectionCache> workShiftsWithActivity = new List<IShiftProjectionCache>();

			foreach (var projectionCache in shiftList)
			{
				if (restriction.VisualLayerCollectionSatisfiesActivityRestriction(scheduleDayDateOnly, agentTimeZone,
																				  projectionCache.MainShiftProjection.OfType<IActivityRestrictableVisualLayer>()))
				{
					workShiftsWithActivity.Add(projectionCache);
				}
			}

			finderResult.AddFilterResults(
				new WorkShiftFilterResult(UserTexts.Resources.FilterOnPreferenceActivity, shiftList.Count,
					workShiftsWithActivity.Count));

			return workShiftsWithActivity;
		}
	}
}
