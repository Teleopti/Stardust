using System;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntraDayOptimization : IIntradayOptimization
	{
		public OptimizationResultModel Optimize(Guid planningPeriodId)
		{
			return new OptimizationResultModel();
		}
	}
}
