using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public class TrackOptimizationCallback : IOptimizationCallback
	{
		private readonly IList<OptimizationCallbackInfo> callbacks = new List<OptimizationCallbackInfo>();
		private bool canceled; 
		
		public void Optimizing(OptimizationCallbackInfo callbackInfo)
		{
			callbacks.Add(callbackInfo);
		}

		public bool IsCancelled()
		{
			return canceled;
		}

		public void SetCancelled()
		{
			canceled = true;
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