using System;
using System.Linq;
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
		private readonly IAnalyticsFactScheduleDayCountHandler _analyticsFactScheduleDayCountHandler;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;

		public AnalyticsScheduleChangeUpdater(
			IIntervalLengthFetcher intervalLengthFetcher, 
			IAnalyticsFactScheduleTimeHandler analyticsFactScheduleTimeHandler, 
			IAnalyticsFactScheduleDateHandler analyticsFactScheduleDateHandler, 
			IAnalyticsFactSchedulePersonHandler analyticsFactSchedulePersonHandler, 
			IAnalyticsFactScheduleDayCountHandler analyticsFactScheduleDayCountHandler, 
			IAnalyticsScheduleRepository analyticsScheduleRepository)

		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_analyticsFactScheduleTimeHandler = analyticsFactScheduleTimeHandler;
			_analyticsFactScheduleDateHandler = analyticsFactScheduleDateHandler;
			_analyticsFactSchedulePersonHandler = analyticsFactSchedulePersonHandler;
			_analyticsFactScheduleDayCountHandler = analyticsFactScheduleDayCountHandler;
			_analyticsScheduleRepository = analyticsScheduleRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			if(!@event.IsDefaultScenario) return;
			var intervalLength = _intervalLengthFetcher.IntervalLength;
			//var minutesPerInterval = 60/intervalLength;
			var scenarioId = getScenario(@event.ScenarioId);
			
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				int dateId;
				if (!_analyticsFactScheduleDateHandler.MapDateId(new DateOnly(scheduleDay.Date), out dateId))
				{
					//Log that schedule id could not be mapped = Schedule changes is not saved in analytics db.
					continue;
				}

				var personPart = _analyticsFactSchedulePersonHandler.Handle(scheduleDay.PersonPeriodId);
				_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);

				if (scheduleDay.NotScheduled)
					continue;

				
				var shiftCategoryId = -1;
				var shiftStart = new DateTime();
				var intervalStart = new DateTime();
				var shiftEnd = new DateTime();
				var localStartDate = new DateOnly(scheduleDay.Date);

				if (scheduleDay.Shift != null)
				{
					shiftCategoryId = getCategory(scheduleDay.ShiftCategoryId);
					shiftStart = scheduleDay.Shift.StartDateTime;
					intervalStart = shiftStart;
					shiftEnd = scheduleDay.Shift.EndDateTime;
				}
				var dayCount = _analyticsFactScheduleDayCountHandler.Handle(scheduleDay, personPart, scenarioId, shiftCategoryId);
				_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);

				while (intervalStart < shiftEnd)
				{
					var intervalLayers = scheduleDay.Shift.FilterLayers(new DateTimePeriod(intervalStart, intervalStart.AddMinutes(intervalLength)));
					foreach (var intervalLayer in intervalLayers)
					{
						var datePart = _analyticsFactScheduleDateHandler.Handle(shiftStart, shiftEnd, localStartDate, intervalLayer, @event.Timestamp, intervalLength);
						if (datePart == null)
						{
							intervalStart = shiftEnd;
							_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);
							break;
						}
						var timePart = _analyticsFactScheduleTimeHandler.Handle(intervalLayer, shiftCategoryId, scenarioId );
						timePart.ShiftLength = (int)(shiftEnd - shiftStart).TotalMinutes;

						_analyticsScheduleRepository.PersistFactScheduleRow(timePart, datePart, personPart);
					}
					
					intervalStart = intervalStart.AddMinutes(intervalLength);
				}
			}
		}

		private int getScenario(Guid scenarioCode)
		{
			var scenarios = _analyticsScheduleRepository.Scenarios();
			var scen = scenarios.FirstOrDefault(x => x.Code.Equals(scenarioCode));
			if (scen == null)
				return -1;
			return scen.Id;
		}

		private int getCategory(Guid shiftCategoryCode)
		{
			var cats = _analyticsScheduleRepository.ShiftCategories();
			var cat = cats.FirstOrDefault(x => x.Code.Equals(shiftCategoryCode));
			if (cat == null)
				return -1;
			return cat.Id;
		}
	}

	

	
}
