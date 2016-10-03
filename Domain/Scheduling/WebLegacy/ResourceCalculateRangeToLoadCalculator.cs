using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	//dont mess with the period on calculation only
	public class ResourceCalculateRangeToLoadCalculator : ISchedulerRangeToLoadCalculator
	{
		private readonly DateTimePeriod _requestedDateTimePeriod;

		public ResourceCalculateRangeToLoadCalculator(DateTimePeriod requestedDateTimePeriod)
		{
			_requestedDateTimePeriod = requestedDateTimePeriod;
			
		}
		public DateTimePeriod SchedulerRangeToLoad(IPerson person)
		{
			return _requestedDateTimePeriod;
		}

		public DateTimePeriod RequestedPeriod {
			get
			{
				return _requestedDateTimePeriod;
			}
		}
	}
}