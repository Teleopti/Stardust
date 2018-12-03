using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class AnalyticsAvailabilityUpdater : 
		IHandleEvent<AvailabilityChangedEvent>,
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnHangfire
	{
		private readonly HandleMultipleAnalyticsAvailabilityDays _handleMultipleAnalyticsAvailabilityDays;
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsAvailabilityUpdater));

		public AnalyticsAvailabilityUpdater(HandleMultipleAnalyticsAvailabilityDays handleMultipleAnalyticsAvailabilityDays)
		{
			_handleMultipleAnalyticsAvailabilityDays = handleMultipleAnalyticsAvailabilityDays;
			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			var period = new DateOnlyPeriod(new DateOnly(@event.StartDateTime.Date), new DateOnly(@event.EndDateTime.Date));
			_handleMultipleAnalyticsAvailabilityDays.Execute(@event.PersonId, period.DayCollection());
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AvailabilityChangedEvent @event)
		{
			_handleMultipleAnalyticsAvailabilityDays.Execute(@event.PersonId, @event.Dates);
		}
	}
}