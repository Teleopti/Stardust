using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayerFactoryWithCascading : IResourceCalculateDelayerFactory
	{
		private readonly CascadingResourceCalculation _cascadingResourceCalculation;

		public ResourceCalculateDelayerFactoryWithCascading(CascadingResourceCalculation cascadingResourceCalculation)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
		}

		public IResourceCalculateDelayer Create(int calculationFrequency, bool considerShortBreaks)
		{
			return new ResourceCalculateDelayerWithCascading(_cascadingResourceCalculation, calculationFrequency, considerShortBreaks);
		}
	}
}