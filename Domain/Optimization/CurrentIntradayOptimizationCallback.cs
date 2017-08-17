namespace Teleopti.Ccc.Domain.Optimization
{
	public class CurrentIntradayOptimizationCallback : ICurrentIntradayOptimizationCallback
	{
		public IIntradayOptimizationCallback Current()
		{
			return new NoIntradayOptimizationCallback();
		}
	}
}