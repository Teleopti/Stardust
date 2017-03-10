using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class CommonMainShiftFilter
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public CommonMainShiftFilter(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction effectiveRestriction)
		{
			if (shiftList == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			if (effectiveRestriction.CommonMainShift != null)
			{
				var shift = shiftList.FirstOrDefault(x => _scheduleDayEquator.MainShiftBasicEquals(x.TheMainShift, effectiveRestriction.CommonMainShift));
				return shift != null ? new List<IShiftProjectionCache> { shift } : null;
			}
			return shiftList;
		}
	}
}
