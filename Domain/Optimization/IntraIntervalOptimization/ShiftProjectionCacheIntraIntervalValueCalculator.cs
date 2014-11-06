using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IShiftProjectionCacheIntraIntervalValueCalculator
	{
		double Calculate(IList<int> samplesBefore, IList<int> samplesToAdd);
	}

	public class ShiftProjectionCacheIntraIntervalValueCalculator : IShiftProjectionCacheIntraIntervalValueCalculator
	{
		public double Calculate(IList<int> samplesBefore, IList<int> samplesToAdd)
		{
			var minBefore = double.MaxValue;
			var maxBefore = double.MinValue;

			var minAfter = double.MaxValue;
			var maxAfter = double.MinValue;

			var samplesAfter = samplesBefore.Select((t, i) => t + samplesToAdd[i]).ToList();

			foreach (var i in samplesBefore)
			{
				if (i < minBefore) minBefore = i;
				if (i > maxBefore) maxBefore = i;
			}

			foreach (var i in samplesAfter)
			{
				if (i < minAfter) minAfter = i;
				if (i > maxAfter) maxAfter = i;
			}

			var valueBefore = minBefore / maxBefore;
			var valueAfter = minAfter / maxAfter;

			return valueBefore - valueAfter;
		}
	}
}
