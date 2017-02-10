using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationLimitsForAgent
	{
		private readonly IDictionary<IPerson, OptimizationLimits> _dic;

		public OptimizationLimitsForAgent(IDictionary<IPerson, OptimizationLimits> dic)
		{
			_dic = dic;
		}

		public OptimizationLimits ForAgent(IPerson agent)
		{
			return _dic==null ? 
				new OptimizationLimits(new optimizationOverLimitByRestrictionDeciderAcceptEverything()) : 
				_dic[agent];
		}

		private class optimizationOverLimitByRestrictionDeciderAcceptEverything : IOptimizationOverLimitByRestrictionDecider
		{
			public bool MoveMaxDaysOverLimit()
			{
				return false;
			}

			public OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix)
			{
				throw new System.NotImplementedException();
			}

			public bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}