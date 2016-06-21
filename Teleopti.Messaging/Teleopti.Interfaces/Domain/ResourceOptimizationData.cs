namespace Teleopti.Interfaces.Domain
{
	public class ResourceOptimizationData
	{
		public ResourceOptimizationData(ISchedulingResultStateHolder schedulingResultStateHolder, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = schedulingResultStateHolder.Schedules;
		}

		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
	}
}