using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IValidDateTimePeriodShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult);
	}

	public class ValidDateTimePeriodShiftFilter : IValidDateTimePeriodShiftFilter
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ValidDateTimePeriodShiftFilter(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			var cntBefore = shiftList.Count;
			IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
			foreach (IShiftProjectionCache proj in shiftList)
			{
				var mainShiftPeriod = proj.TheMainShift.LayerCollection.OuterPeriod();
				if (mainShiftPeriod.HasValue)
				{
					if (validPeriod.Contains(mainShiftPeriod.Value))
					{
						workShiftsWithinPeriod.Add(proj);
					}
				}
			}
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
								  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
								  validPeriod.StartDateTimeLocal(currentTimeZone), validPeriod.EndDateTimeLocal(currentTimeZone)), cntBefore,
					workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}
	}
}
