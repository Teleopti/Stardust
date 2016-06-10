using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsHourlyAvailabilityRepository : IAnalyticsHourlyAvailabilityRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;

		public AnalyticsHourlyAvailabilityRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
		}

		public void Delete(int personId, int dateId, int scenarioId)
		{
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec [mart].[etl_fact_hourly_availability_delete] 
				   @date_id=:{nameof(dateId)}
				  ,@person_id=:{nameof(personId)}
				  ,@scenario_id=:{nameof(scenarioId)}
			")
			.SetParameter(nameof(dateId), dateId)
			.SetParameter(nameof(personId), personId)
			.SetParameter(nameof(scenarioId), scenarioId)
			.ExecuteUpdate();
		}

		public void AddOrUpdate(AnalyticsHourlyAvailability analyticsHourlyAvailability)
		{
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"
				exec [mart].[etl_fact_hourly_availability_add_or_update] 
				   @date_id=:{nameof(analyticsHourlyAvailability.DateId)}
				  ,@person_id=:{nameof(analyticsHourlyAvailability.PersonId)}
				  ,@scenario_id=:{nameof(analyticsHourlyAvailability.ScenarioId)}
				  ,@available_time_m=:{nameof(analyticsHourlyAvailability.AvailableTimeMinutes)}
				  ,@available_days=:{nameof(analyticsHourlyAvailability.AvailableDays)}
				  ,@scheduled_time_m=:{nameof(analyticsHourlyAvailability.ScheduledTimeMinutes)}
				  ,@scheduled_days=:{nameof(analyticsHourlyAvailability.ScheduledDays)}
				  ,@business_unit_id=:{nameof(analyticsHourlyAvailability.BusinessUnitId)}
			")
			.SetParameter(nameof(analyticsHourlyAvailability.DateId), analyticsHourlyAvailability.DateId)
			.SetParameter(nameof(analyticsHourlyAvailability.PersonId), analyticsHourlyAvailability.PersonId)
			.SetParameter(nameof(analyticsHourlyAvailability.ScenarioId), analyticsHourlyAvailability.ScenarioId)
			.SetParameter(nameof(analyticsHourlyAvailability.AvailableTimeMinutes), analyticsHourlyAvailability.AvailableTimeMinutes)
			.SetParameter(nameof(analyticsHourlyAvailability.AvailableDays), analyticsHourlyAvailability.AvailableDays)
			.SetParameter(nameof(analyticsHourlyAvailability.ScheduledTimeMinutes), analyticsHourlyAvailability.ScheduledTimeMinutes)
			.SetParameter(nameof(analyticsHourlyAvailability.ScheduledDays), analyticsHourlyAvailability.ScheduledDays)
			.SetParameter(nameof(analyticsHourlyAvailability.BusinessUnitId), analyticsHourlyAvailability.BusinessUnitId)
			.ExecuteUpdate();
		}
	}
}