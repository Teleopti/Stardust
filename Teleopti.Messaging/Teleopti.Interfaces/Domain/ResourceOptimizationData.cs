namespace Teleopti.Interfaces.Domain
{
	public class ResourceOptimizationData
	{
		public ResourceOptimizationData(bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
		}

		public bool ConsiderShortBreaks { get; private set; }
		public bool DoIntraIntervalCalculation { get; private set; }
	}
}