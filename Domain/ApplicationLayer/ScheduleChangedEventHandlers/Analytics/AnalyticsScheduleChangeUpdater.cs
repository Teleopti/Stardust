using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeUpdater :
		IHandleEvent<ScheduleChangedEvent>,
		IHandleEvent<ReloadSchedules>,
		IRunOnHangfire
	{
		private readonly IUpdateFactSchedules _updateFactSchedules;

		public AnalyticsScheduleChangeUpdater(IUpdateFactSchedules updateFactSchedules)
		{
			_updateFactSchedules = updateFactSchedules;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			Func<IPerson, IEnumerable<DateTime>> period = p =>
				new DateTimePeriod(@event.StartDateTime, @event.EndDateTime)
					.ToDateOnlyPeriod(p.PermissionInformation.DefaultTimeZone()).DayCollection().Select(d => d.Date);
			_updateFactSchedules.Execute(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				period, @event.Timestamp);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(2)]
		public virtual void Handle(ReloadSchedules @event)
		{
			_updateFactSchedules.Execute(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				p=>@event.Dates, @event.Timestamp);
		}
	}
}