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
			var minAfter = double.MaxValue;
			var maxAfter = double.MinValue;

			if (samplesBefore.Count == 0)
			{
				foreach (var i in samplesToAdd)
				{
					samplesBefore.Add(1);
				}
			}

			var samplesAfter = samplesBefore.Select((t, i) => t + samplesToAdd[i]).ToArray();

			foreach (var i in samplesAfter)
			{
				if (i < minAfter) minAfter = i;
				if (i > maxAfter) maxAfter = i;
			}

			var valueAfter = minAfter / maxAfter;

			return valueAfter;
		}
	}
}
