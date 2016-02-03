using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[UseOnToggle(Toggles.ETL_SpeedUpETL_30791)]
	public class AnalyticsScheduleChangeUpdater : 
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof(AnalyticsScheduleChangeUpdater));
		private readonly IAnalyticsFactScheduleHandler _factScheduleHandler;
		private readonly IAnalyticsFactSchedulePersonHandler _factSchedulePersonHandler;
		private readonly IAnalyticsFactScheduleDateHandler _factScheduleDateHandler;
		private readonly IAnalyticsFactScheduleDayCountHandler _factScheduleDayCountHandler;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IDelayedMessageSender _serviceBus;

		public AnalyticsScheduleChangeUpdater(
			IAnalyticsFactScheduleHandler factScheduleHandler,
			IAnalyticsFactScheduleDateHandler factScheduleDateHandler,
			IAnalyticsFactSchedulePersonHandler factSchedulePersonHandler,
			IAnalyticsFactScheduleDayCountHandler factScheduleDayCountHandler,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IDelayedMessageSender serviceBus)

		{
			_factScheduleHandler = factScheduleHandler;
			_factScheduleDateHandler = factScheduleDateHandler;
			_factSchedulePersonHandler = factSchedulePersonHandler;
			_factScheduleDayCountHandler = factScheduleDayCountHandler;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_serviceBus = serviceBus;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			if (!@event.IsDefaultScenario) return;
			var scenarioId = getScenario(@event.ScenarioId);

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				int dateId;
				if (!_factScheduleDateHandler.MapDateId(new DateOnly(scheduleDay.Date), out dateId))
				{
					//Log that schedule id could not be mapped = Schedule changes is not saved in analytics db.
					Logger.DebugFormat(
						"Date {0} could not be mapped to Analytics date_id. Schedule changes for agent {1} is not saved into Analytics database.",
						scheduleDay.Date,
						@event.PersonId);
					continue;
				}

				var personPart = _factSchedulePersonHandler.Handle(scheduleDay.PersonPeriodId);
				if (personPart.PersonId == -1)
				{
					Logger.DebugFormat(
						"PersonPeriodId {0} could not be found. Schedule changes for agent {1} is not saved into Analytics database.",
						scheduleDay.PersonPeriodId,
						@event.PersonId);
					continue;
				}
				try
				{
					_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);

					if (!scheduleDay.NotScheduled)
					{
						var shiftCategoryId = -1;
						if (scheduleDay.Shift != null)
							shiftCategoryId = getCategory(scheduleDay.ShiftCategoryId);

						var dayCount = _factScheduleDayCountHandler.Handle(scheduleDay, personPart, scenarioId, shiftCategoryId);
						if (dayCount != null)
							_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);

						var agentDaySchedule = _factScheduleHandler.AgentDaySchedule(scheduleDay, personPart, @event.Timestamp, shiftCategoryId, scenarioId);
						if (agentDaySchedule == null)
						{
							_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);
							continue;
						}
						_analyticsScheduleRepository.PersistFactScheduleBatch(agentDaySchedule);
					}

					if (scheduleDay.Date < DateTime.Now.AddDays(1))
					{
						_analyticsScheduleRepository.InsertStageScheduleChangedServicebus(new DateOnly(scheduleDay.Date), @event.PersonId,
						@event.ScenarioId, @event.LogOnBusinessUnitId, DateTime.Now);
					}
				}
				catch (SqlException ex)
				{
					//debug warn
					//timeout = -2
					if (ex.Number != -2)
					{
						Logger.Error(ex.Message);
                  throw;
					}
					var numOfRetry = @event.RetriesCount += 1;
					if (numOfRetry > 5)
					{
						Logger.ErrorFormat("Timeout when handling ProjectionChangedEvent on day {0}. Maximim number of retries reached, giving up.", scheduleDay.Date);
						return;
					}
               Logger.WarnFormat("Timeout when handling ProjectionChangedEvent on day {0}. Resending the event for processing later. Retry number {1}", scheduleDay.Date, numOfRetry);
					var processTime = DateTime.Now.AddSeconds(30*numOfRetry);
					var @newEvent = new ProjectionChangedEvent
					{
						ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
						ScenarioId = @event.ScenarioId,
						LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
						InitiatorId = @event.InitiatorId,
						LogOnDatasource = @event.LogOnDatasource,
						IsDefaultScenario = @event.IsDefaultScenario,
						IsInitialLoad = @event.IsInitialLoad,
						PersonId = @event.PersonId,
						Timestamp = DateTime.UtcNow,
						TrackId = @event.TrackId,
						RetriesCount = numOfRetry
					};
					_serviceBus.DelaySend(processTime, @newEvent);
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
