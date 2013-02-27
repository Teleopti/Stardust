using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayBlock
	{
		IList<DateOnly> BlockDays { get; set; }
		IList<double?> StandardDeviations { get; set; }
		double Sum { get; }
		double Average { get; }
		DateOnlyPeriod CoveringPeriod { get; }
	}

	public class IntradayBlock : IIntradayBlock
	{
		public IList<DateOnly> BlockDays { get; set; }
		public DateOnlyPeriod CoveringPeriod { get { return new DateOnlyPeriod(BlockDays.First(), BlockDays.Last()); } }
		public IList<double?> StandardDeviations { get; set; }
		public double Sum
		{
			get
			{
				var sum = 0.0;
				foreach (var standardDeviation in StandardDeviations)
				{
					if (standardDeviation.HasValue)
						sum += standardDeviation.Value;
				}
				return sum;
			}
		}

		public double Average
		{
			get
			{
				var count = 0;
				var sum = 0.0;
				foreach (var standardDeviation in StandardDeviations)
				{
					if (!standardDeviation.HasValue) continue;
					
					count++;
					sum += standardDeviation.Value;
				}
				if (sum > 0)
					return sum / count;
				return 0;
			}
		}
	}
}