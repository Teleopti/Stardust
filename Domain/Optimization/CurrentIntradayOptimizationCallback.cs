namespace Teleopti.Ccc.Domain.Optimization
{
	public class CurrentIntradayOptimizationCallback : ICurrentIntradayOptimizationCallback
	{
		public IOptimizationCallback Current()
		{
			return new NoOptimizationCallback();
		}
	}
}