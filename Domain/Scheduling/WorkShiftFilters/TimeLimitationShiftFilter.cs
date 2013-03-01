using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface ITimeLimitationShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult);
	}

	public class TimeLimitationShiftFilter : ITimeLimitationShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult)
		{
			var cntBefore = shiftList.Count;
			IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
			foreach (IShiftProjectionCache proj in shiftList)
			{
				var mainShiftPeriod = proj.TheMainShift.LayerCollection.Period();
				if (mainShiftPeriod.HasValue)
				{
					if (validPeriod.Contains(mainShiftPeriod.Value))
					{
						workShiftsWithinPeriod.Add(proj);
					}
				}
			}
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(TeleoptiPrincipal.Current.Regional.Culture,
								  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
								  validPeriod.LocalStartDateTime, validPeriod.LocalEndDateTime), cntBefore,
					workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}
	}
}
