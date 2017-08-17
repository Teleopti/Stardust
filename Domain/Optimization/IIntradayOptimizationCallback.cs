namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizationCallback
	{
		void Optimizing(IntradayOptimizationCallbackInfo callbackInfo);
		bool IsCancelled();
	}
}