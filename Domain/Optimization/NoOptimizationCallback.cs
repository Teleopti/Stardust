namespace Teleopti.Ccc.Domain.Optimization
{
	public class NoOptimizationCallback : IOptimizationCallback
	{
		public void Optimizing(OptimizationCallbackInfo callbackInfo)
		{
		}

		public bool IsCancelled()
		{
			return false;
		}
	}
}