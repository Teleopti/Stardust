using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ICommonActivityFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShift);
	}

	public class CommonActivityFilter : ICommonActivityFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISchedulingOptions schedulingOptions, IShiftProjectionCache suggestedShift)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			if (suggestedShift == null) return shiftList;
			if (!(schedulingOptions.UseCommonActivity && schedulingOptions.UseGroupScheduling)) return shiftList;

			var activityPeriodsInSuggestedShift = new List<DateTimePeriod>();
			foreach (var projectedLayer in suggestedShift.MainShiftProjection)
			{
				if (projectedLayer.Payload.Id == schedulingOptions.CommonActivity.Id)
				{
					activityPeriodsInSuggestedShift.Add(projectedLayer.Period);
				}
			}
			IList<IShiftProjectionCache> activtyfinalShiftList = new List<IShiftProjectionCache>();
			foreach (var shift in shiftList)
			{
				IList<DateTimePeriod> visualLayerPeriodList = new List<DateTimePeriod>();
				foreach (var visualLayer in shift.TheMainShift.ProjectionService().CreateProjection().Where(c => c.Payload.Id == schedulingOptions.CommonActivity.Id))
				{
					visualLayerPeriodList.Add(visualLayer.Period);
				}

				if (activityPeriodsInSuggestedShift.All(visualLayerPeriodList.Contains))
				{
					activtyfinalShiftList.Add(shift);
				}
			}
			return activtyfinalShiftList;
		}
	}
}