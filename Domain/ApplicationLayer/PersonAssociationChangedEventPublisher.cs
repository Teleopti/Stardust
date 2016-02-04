using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[UseOnToggle(Toggles.RTA_TerminatedPersons_36042)]
	public class PersonAssociationChangedEventPublisher : 
		IHandleEvent<TenantHearbeatEvent>,
		IHandleEvent<PersonTerminalDateChangedEvent>,
		IHandleEvent<PersonTeamChangedEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _persons;
		private readonly IEventPublisher _eventPublisher;
		private readonly INow _now;

		public PersonAssociationChangedEventPublisher(
			IPersonRepository persons,
			IEventPublisher eventPublisher,
			INow now)
		{
			_persons = persons;
			_eventPublisher = eventPublisher;
			_now = now;
		}

		[UnitOfWork]
		[RecurringId("PersonAssociationChangedEventPublisher:::HourlyTickEvent")]
		public virtual void Handle(TenantHearbeatEvent @event)
		{
			_persons.LoadAll()
				.ForEach(person =>
				{
					var now = _now.UtcDateTime();
					var timeZone = person.PermissionInformation.DefaultTimeZone();
					var agentDate = new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));
					var currentPeriod = person.Period(agentDate);
					
					var time = timeOfChange(person.TerminalDate, currentPeriod, timeZone);

					if (time == null)
						return;
					if (time > now)
						return;
					if (time < now.AddDays(-1))
						return;

					_eventPublisher.Publish(new PersonAssociationChangedEvent
					{
						PersonId = person.Id.Value,
						Timestamp = now,
						TeamId = currentPeriod != null ? currentPeriod.Team.Id.Value : null as Guid?,
						SiteId = currentPeriod != null ? currentPeriod.Team.Site.Id.Value : null as Guid?,
						BusinessUnitId = currentPeriod != null ? currentPeriod.Team.Site.BusinessUnit.Id.Value : null as Guid?
					});
				});
		}

		public void Handle(PersonTerminalDateChangedEvent @event)
		{
			var timeZone = @event.TimeZoneInfoId != null ? TimeZoneInfo.FindSystemTimeZoneById(@event.TimeZoneInfoId) : TimeZoneInfo.Utc;
			var previousTerminatedAt = terminationTime(@event.PreviousTerminationDate, timeZone);
			var terminatedAt = terminationTime(@event.TerminationDate, timeZone);

			var now = _now.UtcDateTime();
			
			if (previousTerminatedAt < now && terminatedAt < now)
				return;
			if (previousTerminatedAt > now && terminatedAt > now)
				return;
		
			_eventPublisher.Publish(new PersonAssociationChangedEvent
			{
				PersonId = @event.PersonId,
				Timestamp = now,
				TeamId = @event.TeamId,
				SiteId = @event.SiteId,
				BusinessUnitId = @event.BusinessUnitId
			});
		}
		
		private DateTime? timeOfChange(DateOnly? terminalDate, IPersonPeriod currentPeriod, TimeZoneInfo timeZone)
		{
			var terminatedAt = terminationTime(terminalDate, timeZone);
			if (terminatedAt <= _now.UtcDateTime())
				return terminatedAt;
			if (currentPeriod == null)
				return null;
			return TimeZoneInfo.ConvertTimeToUtc(currentPeriod.StartDate.Date, timeZone);
		}

		private static DateTime terminationTime(DateTime? terminationDate, TimeZoneInfo timeZone)
		{
			var date = terminationDate.HasValue ? new DateOnly(terminationDate.Value) : null as DateOnly?;
			return terminationTime(date, timeZone);
		}

		private static DateTime terminationTime(DateOnly? terminationDate, TimeZoneInfo timeZone)
		{
			return terminationDate.HasValue
				? TimeZoneInfo.ConvertTimeToUtc(terminationDate.Value.Date.AddDays(1), timeZone)
				: DateTime.MaxValue;
		}

		[UseOnToggle(Toggles.RTA_TeamChanges_36043)]
		public virtual void Handle(PersonTeamChangedEvent @event)
		{
			_eventPublisher.Publish(new PersonAssociationChangedEvent());
		}
	}
}