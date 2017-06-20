namespace Teleopti.Ccc.Domain.Optimization
{
	public class CurrentIntradayOptimizationCallback : ICurrentIntradayOptimizationCallback
	{
		public IIntradayOptimizationCallback Current()
		{
			return new nullIntradayOptimizationCallback();
		}

		private class nullIntradayOptimizationCallback : IIntradayOptimizationCallback
		{
			public void Optimizing(IntradayOptimizationCallbackInfo callbackInfo)
			{
			}

			public bool IsCancelled()
			{
				return false;
			}
		}
	}
}