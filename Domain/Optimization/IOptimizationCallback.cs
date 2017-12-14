namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationCallback
	{
		void Optimizing(OptimizationCallbackInfo callbackInfo);
		bool IsCancelled();
	}
}