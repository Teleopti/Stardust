namespace Teleopti.Ccc.Domain.Optimization
{
	public class CurrentOptimizationCallback : ICurrentOptimizationCallback
	{
		public IOptimizationCallback Current()
		{
			return new NoOptimizationCallback();
		}
	}
}