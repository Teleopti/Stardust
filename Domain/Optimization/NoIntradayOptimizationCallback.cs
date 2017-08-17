namespace Teleopti.Ccc.Domain.Optimization
{
	public class NoIntradayOptimizationCallback : IIntradayOptimizationCallback
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