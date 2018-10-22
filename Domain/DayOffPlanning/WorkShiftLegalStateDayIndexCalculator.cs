using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class WorkShiftLegalStateDayIndexCalculator
    {
		public double?[] CalculateIndexForRaising(IEnumerable<double?> relativeDeficits)
		{
			var max = relativeDeficits.Any() ? relativeDeficits.Max(d => d ?? 0d) : 0d;

			return relativeDeficits.Select(d => d.HasValue ? Math.Abs(d.Value - max - 1) : (double?)null).ToArray();
		}

		public double?[] CalculateIndexForReducing(IEnumerable<double?> relativeDeficits)
        {
			var min = relativeDeficits.Any() ? Math.Abs(relativeDeficits.Min(d => d ?? 0d)) : 0d;

			return relativeDeficits.Select(d => d.HasValue ? Math.Abs(d.Value + min + 1) : (double?)null).ToArray();
        }
    }
}
