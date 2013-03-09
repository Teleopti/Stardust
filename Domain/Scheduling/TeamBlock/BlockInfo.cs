using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IBlockInfo
	{
		DateOnlyPeriod BlockPeriod { get; }
		IList<double?> StandardDeviations { get; set; }
		double Sum { get; }
		double Average { get; }
	}

	public class BlockInfo : IBlockInfo
	{
		private readonly DateOnlyPeriod _blockPeriod;

		public BlockInfo(DateOnlyPeriod blockPeriod)
		{
			_blockPeriod = blockPeriod;
		}

		public DateOnlyPeriod BlockPeriod
		{
			get { return _blockPeriod; }
		}

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