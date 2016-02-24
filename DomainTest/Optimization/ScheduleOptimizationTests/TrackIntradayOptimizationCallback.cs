using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	public class TrackIntradayOptimizationCallback : IIntradayOptimizationCallback
	{
		private readonly IList<IntradayOptimizationCallbackInfo> callbacks = new List<IntradayOptimizationCallbackInfo>();

		public void Optimizing(IntradayOptimizationCallbackInfo callbackInfo)
		{
			callbacks.Add(callbackInfo);
		}

		public bool IsCancelled()
		{
			return false;
		}

		public int SuccessfulOptimizations()
		{
			return callbacks.Count(x => x.WasSuccessful);
		}

		public int UnSuccessfulOptimizations()
		{
			return callbacks.Count(x => !x.WasSuccessful);
		}
	}
}