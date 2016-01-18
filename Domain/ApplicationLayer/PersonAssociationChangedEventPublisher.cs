using System;
using System.Linq;
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
		IHandleEvent<HourlyTickEvent>,
		IHandleEvent<PersonTerminalDateChangedEvent>,
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
		public virtual void Handle(HourlyTickEvent @event)
		{
			_persons.LoadAll()
				.ForEach(person =>
				{
					var terminatedAt = terminationTime(person.TerminalDate, person.PermissionInformation.DefaultTimeZone());
					var now = _now.UtcDateTime();

					if (terminatedAt > now)
						return;
					if (terminatedAt < now.AddDays(-1))
						return;

					_eventPublisher.Publish(new PersonAssociationChangedEvent
					{
						PersonId = person.Id.Value,
						Timestamp = now
					});
				});
		}

		public void Handle(PersonTerminalDateChangedEvent @event)
		{
			var terminalDate = @event.TerminationDate.HasValue ? new DateOnly(@event.TerminationDate.Value) : null as DateOnly?;
			var timeZoneId = @event.TimeZoneInfoId ?? TimeZoneInfo.Utc.Id;
			var terminatedAt = terminationTime(terminalDate, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

			var now = _now.UtcDateTime();

			if (@event.PreviousTerminationDate < now && terminatedAt < now)
				return;
			if (@event.PreviousTerminationDate > now && terminatedAt > now)
				return;

			var teamId = @event.PersonPeriodsAfter
				.EmptyIfNull()
				.Where(x => x.StartDate <= now && x.EndDate > now)
				.Select(x => x.TeamId as Guid?)
				.SingleOrDefault();
			
			_eventPublisher.Publish(new PersonAssociationChangedEvent
			{
				PersonId = @event.PersonId,
				Timestamp = now,
				TeamId = teamId
			});
		}

		private static DateTime terminationTime(DateOnly? terminationDate, TimeZoneInfo timeZone)
		{
			return terminationDate.HasValue
				? TimeZoneInfo.ConvertTimeToUtc(terminationDate.Value.Date.AddDays(1), timeZone)
				: DateTime.MaxValue;
		}
	}
}