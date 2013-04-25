using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ICommonMainShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction effectiveRestriction);
	}
	
	public class CommonMainShiftFilter : ICommonMainShiftFilter
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
				if (shift != null)
					return new List<IShiftProjectionCache> { shift };
				return null;
			}
			return shiftList;
		}
	}
}
