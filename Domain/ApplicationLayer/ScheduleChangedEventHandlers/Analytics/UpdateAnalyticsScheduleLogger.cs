using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IUpdateAnalyticsScheduleLogger
	{
		void Log(ScheduleChangedEvent scheduleChangedEvent);
	}

	public class UpdateAnalyticsScheduleLoggerDummy : IUpdateAnalyticsScheduleLogger
	{
		public void Log(ScheduleChangedEvent scheduleChangedEvent)
		{
			
		}
	}

	public class UpdateAnalyticsScheduleLogger : IUpdateAnalyticsScheduleLogger
	{
		private readonly ICurrentDataSource _currentDataSource;
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateAnalyticsScheduleLogger));

		public UpdateAnalyticsScheduleLogger(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}
		public void Log(ScheduleChangedEvent @event)
		{
			if (_currentDataSource.Current().DataSourceName != @event.LogOnDatasource)
			{
				logger.Warn($"Datasources not matching! LogOnDatasourceFromEvent={@event.LogOnDatasource}; CurrentDatasource={_currentDataSource.Current().DataSourceName}");
			}
			if (logger.IsInfoEnabled)
			{
				logger.Info(
					$"Start handling ScheduleChangedEvent: PersonId={@event.PersonId}; Date={@event.Date}; StartDateTime={@event.StartDateTime};" 
						+ $"EndDateTime={@event.EndDateTime}; ScenarioId={@event.ScenarioId}; Timestamp={@event.Timestamp}; LogOnBusinessUnitId={@event.LogOnBusinessUnitId}");
			}
		}
	}
}