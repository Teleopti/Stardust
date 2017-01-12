using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ICommonActivityFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction);
	}

	public class CommonActivityFilter : ICommonActivityFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;
			if (effectiveRestriction == null) return shiftList;
			if (!(schedulingOptions.TeamSameActivity && schedulingOptions.UseTeam)) return shiftList;

			if (effectiveRestriction.CommonActivity == null) return shiftList;

			var retList = (from shift in shiftList.AsParallel()
						   let visualLayerPeriodList =
				shift.TheMainShift.ProjectionService()
					.CreateProjection()
					.Where(c => c.Payload.Id == schedulingOptions.CommonActivity.Id)
					.Select(c => c.Period)
					.ToArray()
				where effectiveRestriction.CommonActivity.Periods.All(visualLayerPeriodList.Contains)
				select shift).ToList();
			return retList;
		}
	}
}