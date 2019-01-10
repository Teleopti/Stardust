using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeUpdater :
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEvent<ReloadSchedules>,
		IRunOnHangfire
	{
		private readonly UpdateFactSchedules _updateFactSchedules;
		private readonly IUpdateAnalyticsScheduleLogger _updateAnalyticsScheduleLogger;

		public AnalyticsScheduleChangeUpdater(UpdateFactSchedules updateFactSchedules, IUpdateAnalyticsScheduleLogger updateAnalyticsScheduleLogger)
		{
			_updateFactSchedules = updateFactSchedules;
			_updateAnalyticsScheduleLogger = updateAnalyticsScheduleLogger;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			_updateAnalyticsScheduleLogger.Log(@event);

			Func<IPerson, IEnumerable<DateTime>> period = p =>
			{
				var days =
					new DateTimePeriod(@event.StartDateTime, @event.EndDateTime)
						.ToDateOnlyPeriod(p.PermissionInformation.DefaultTimeZone()).DayCollection().Select(d => d.Date).ToList();
				if (@event.Date.HasValue)
					days.Add(@event.Date.Value);
				return days.Distinct();
			};

			_updateFactSchedules.Execute(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				period, @event.Timestamp);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[Attempts(2)]
		public virtual void Handle(ReloadSchedules @event)
		{
			_updateFactSchedules.Execute(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				p=>@event.Dates, @event.Timestamp);
		}
	}
}