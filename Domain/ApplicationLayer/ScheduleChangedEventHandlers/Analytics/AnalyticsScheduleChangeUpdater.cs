using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[UseOnToggle(Toggles.ETL_SpeedUpETL_30791)]
	public class AnalyticsScheduleChangeUpdater : IHandleEvent<ProjectionChangedEvent>
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IAnalyticsFactScheduleTimeHandler _analyticsFactScheduleTimeHandler;
		private readonly IAnalyticsFactScheduleDateHandler _analyticsFactScheduleDateHandler;
		private readonly IAnalyticsFactSchedulePersonHandler _analyticsFactSchedulePersonHandler;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;

		public AnalyticsScheduleChangeUpdater(
			IIntervalLengthFetcher intervalLengthFetcher,
			IAnalyticsFactScheduleTimeHandler analyticsFactScheduleTimeHandler,
			IAnalyticsFactScheduleDateHandler analyticsFactScheduleDateHandler,
			IAnalyticsFactSchedulePersonHandler analyticsFactSchedulePersonHandler,
			IAnalyticsScheduleRepository analyticsScheduleRepository)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_analyticsFactScheduleTimeHandler = analyticsFactScheduleTimeHandler;
			_analyticsFactScheduleDateHandler = analyticsFactScheduleDateHandler;
			_analyticsFactSchedulePersonHandler = analyticsFactSchedulePersonHandler;
			_analyticsScheduleRepository = analyticsScheduleRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			var intervalLength = _intervalLengthFetcher.IntervalLength;

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				
				if(scheduleDay.Shift == null) 
					break;
				//var dayCount = new AnalyticsFactScheduleDayCount();
				var intervalStart = scheduleDay.Shift.StartDateTime;
				var shiftEnd = scheduleDay.Shift.EndDateTime;
				while (intervalStart < shiftEnd)
				{
					var intervalLayers = scheduleDay.Shift.FilterLayers(new DateTimePeriod(intervalStart, intervalStart.AddMinutes(intervalLength)));
					foreach (var intervalLayer in intervalLayers)
					{
						var timePart = _analyticsFactScheduleTimeHandler.Handle(intervalLayer);
						var datePart = _analyticsFactScheduleDateHandler.Handle(intervalLayer);
						var personPart = _analyticsFactSchedulePersonHandler.Handle(intervalLayer);

						_analyticsScheduleRepository.PersistFactScheduleRow(timePart, datePart, personPart);
					}

					intervalStart = intervalStart.AddMinutes(intervalLength);
				}
				
				//_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);
			}
		}
	}

	

	
}
