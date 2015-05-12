using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IDisallowedShiftProjectionCashesFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> notAllowedShiftProjectionCaches, IList<IShiftProjectionCache> shiftProjectionCaches, IWorkShiftFinderResult finderResult);
	}

	public class DisallowedShiftProjectionCashesFilter : IDisallowedShiftProjectionCashesFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> notAllowedShiftProjectionCaches, IList<IShiftProjectionCache> shiftProjectionCaches, IWorkShiftFinderResult finderResult)
		{
			if (shiftProjectionCaches == null) return null;
			if (finderResult == null) return null;
			if (shiftProjectionCaches.Count == 0) return shiftProjectionCaches;
			if (notAllowedShiftProjectionCaches.Count == 0) return shiftProjectionCaches;

			var before = shiftProjectionCaches.Count;
			var notAllowedHashes = notAllowedShiftProjectionCaches.Select(notAllowedShiftProjectionCache => notAllowedShiftProjectionCache.GetHashCode()).ToList();
			var result = shiftProjectionCaches.Where(shiftProjectionCache => !notAllowedHashes.Contains(shiftProjectionCache.GetHashCode())).ToList();

			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnIntraIntervals, " "), before, result.Count));
			return result;
		}
	}
}
