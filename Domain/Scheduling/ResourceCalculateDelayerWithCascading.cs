using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayerWithCascading : ResourceCalculateDelayer
	{
		private readonly CascadingResourceCalculation _cascadingResourceCalculation;

		public ResourceCalculateDelayerWithCascading(CascadingResourceCalculation cascadingResourceCalculation, int calculationFrequency, bool considerShortBreaks) 
			: base(null, calculationFrequency, considerShortBreaks)
		{
			_cascadingResourceCalculation = cascadingResourceCalculation;
		}

		protected override void ResourceCalculateDate(DateOnly date, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			//need to consider short breaks and intra later probably
			_cascadingResourceCalculation.ForDay(date);
		}
	}
}