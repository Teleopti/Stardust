using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class ActivityRestrictionsShiftFilter
	{
		public IList<ShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, IPerson person, IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, WorkShiftFinderResult finderResult)
		{
		    if (restriction == null || person == null || shiftList == null || finderResult == null ) return null;

            IList<IActivityRestriction> activityRestrictions = restriction.ActivityRestrictionCollection;
			if (activityRestrictions.Count == 0)
				return shiftList;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var workShiftsWithActivity =
				shiftList.Where(
					s =>
						restriction.VisualLayerCollectionSatisfiesActivityRestriction(scheduleDayDateOnly, timeZone,
							s.MainShiftProjection.OfType<IActivityRestrictableVisualLayer>())).ToList();

			finderResult.AddFilterResults(
				new WorkShiftFilterResult(UserTexts.Resources.FilterOnPreferenceActivity, shiftList.Count,
					workShiftsWithActivity.Count));

			return workShiftsWithActivity;
		}
	}
}
