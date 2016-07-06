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
	public class PersonAssociationChangedEventPublisher : 
		IHandleEvent<TenantHourTickEvent>,
		IHandleEvent<PersonTerminalDateChangedEvent>,
		IHandleEvent<PersonTeamChangedEvent>,
		IHandleEvent<PersonPeriodChangedEvent>,
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
		[RecurringJob]
		public virtual void Handle(TenantHourTickEvent @event)
		{
			_persons.LoadAll()
				.ForEach(person =>
				{
					var now = _now.UtcDateTime();
					var timeZone = person.PermissionInformation.DefaultTimeZone();
					var agentDate = new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));
					var currentPeriod = person.Period(agentDate);
					var previousPeriod = this.previousPeriod(person, currentPeriod);

					var time = timeOfChange(person.TerminalDate, currentPeriod, timeZone);

					if (time == null)
						return;
					if (time > now)
						return;
					if (time < now.AddDays(-1))
						return;

					var previousTeam = previousPeriod?.Team.Id;
					var teamId = currentPeriod?.Team.Id;
					var siteId = currentPeriod?.Team.Site.Id;
					var businessUnitId = currentPeriod?.Team.Site.BusinessUnit.Id;
					
					if (previousTeam.HasValue &&
						teamId.HasValue &&
						previousTeam == teamId)
						return;

					var previousAssociation = from p in person.PersonPeriodCollection
						where p != null &&
						p.StartDate < agentDate
						select new Association
						{
							TeamId = p.Team.Id.Value,
							SiteId = p.Team.Site.Id.Value,
							BusinessUnitId = p.Team.Site.BusinessUnit.Id.Value
						};

					_eventPublisher.Publish(new PersonAssociationChangedEvent
					{
						Version = 2,
						PersonId = person.Id.Value,
						Timestamp = now,
						TeamId = teamId,
						SiteId = siteId,
						BusinessUnitId = businessUnitId,
						PreviousAssociation = previousAssociation
					});
				});
		}

		private IPersonPeriod previousPeriod(IPerson person, IPersonPeriod currentPeriod)
		{
			var currentPeriodIndex = person.PersonPeriodCollection.IndexOf(currentPeriod);
			IPersonPeriod previousPeriod = null;
			if (currentPeriodIndex > 0)
				previousPeriod = person.PersonPeriodCollection.ElementAt(currentPeriodIndex - 1);
			return previousPeriod;
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
				Version = 2,
				PersonId = @event.PersonId,
				Timestamp = now,
				TeamId = @event.TeamId,
				SiteId = @event.SiteId,
				BusinessUnitId = @event.BusinessUnitId,
				PreviousAssociation = @event.PreviousAssociations
			});
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

		public void Handle(PersonTeamChangedEvent @event)
		{
			_eventPublisher.Publish(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = @event.PersonId,
				Timestamp = @event.Timestamp,
				BusinessUnitId = @event.CurrentBusinessUnitId,
				SiteId = @event.CurrentSiteId,
				TeamId = @event.CurrentTeamId,
				PreviousAssociation = @event.PreviousAssociations
			});
		}

		public void Handle(PersonPeriodChangedEvent @event)
		{
			_eventPublisher.Publish(new PersonAssociationChangedEvent
			{
				PersonId = @event.PersonId,
				Timestamp = @event.Timestamp,
				BusinessUnitId = @event.CurrentBusinessUnitId,
				SiteId = @event.CurrentSiteId,
				TeamId = @event.CurrentTeamId
			});
		}
	}
}