using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayerFactoryWithoutCascading : IResourceCalculateDelayerFactory
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public ResourceCalculateDelayerFactoryWithoutCascading(IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public IResourceCalculateDelayer Create(int calculationFrequency, bool considerShortBreaks)
		{
			return new ResourceCalculateDelayer(_resourceOptimizationHelper, calculationFrequency, considerShortBreaks);
		}
	}
}