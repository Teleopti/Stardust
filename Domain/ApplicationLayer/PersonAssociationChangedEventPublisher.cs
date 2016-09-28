using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class PersonAssociationChangedEventPublisher : 
		IHandleEvent<TenantHourTickEvent>,
		IHandleEvent<TenantMinuteTickEvent>,
		IHandleEvent<PersonTerminalDateChangedEvent>,
		IHandleEvent<PersonTeamChangedEvent>,
		IHandleEvent<PersonPeriodChangedEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _persons;
		private readonly IEventPublisher _eventPublisher;
		private readonly INow _now;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly IKeyValueStorePersister _keyValueStore;

		public PersonAssociationChangedEventPublisher(
			IPersonRepository persons,
			IEventPublisher eventPublisher,
			INow now,
			IDistributedLockAcquirer distributedLock,
			IKeyValueStorePersister keyValueStore)
		{
			_persons = persons;
			_eventPublisher = eventPublisher;
			_now = now;
			_distributedLock = distributedLock;
			_keyValueStore = keyValueStore;
		}

		[UnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				if (_keyValueStore.Get("PersonAssociationChangedPublishTrigger", false))
				{
					_keyValueStore.Update("PersonAssociationChangedPublishTrigger", false);
					publishEventsFor(d => true);
				}
			});
		}

		[UnitOfWork]
		public virtual void Handle(TenantHourTickEvent @event)
		{
			publishEventsFor(data =>
			{
				var previousTeam = data.previousPeriod?.Team.Id;
				var teamId = data.currentPeriod?.Team.Id;

				if (data.timeOfChange == null)
					return false;
				if (data.timeOfChange > data.now)
					return false;
				if (data.timeOfChange < data.now.AddDays(-1))
					return false;

				var sameTeam = previousTeam.HasValue &&
							   teamId.HasValue &&
							   previousTeam == teamId;

				var sameExternalLogons =
					(data.previousPeriod?.ExternalLogOnCollection.Select(x => x.AcdLogOnOriginalId).OrderBy(x => x))
						.SequenceEqualNullSafe(
							data.currentPeriod?.ExternalLogOnCollection.Select(x => x.AcdLogOnOriginalId).OrderBy(x => x)
						);

				return !(sameTeam && sameExternalLogons);

			});
		}

		private class data
		{
			public IPerson person;
			public IPersonPeriod previousPeriod;
			public IPersonPeriod currentPeriod;	
			public DateOnly agentDate;	
			public DateTime? timeOfChange;
			public DateTime now;
		}

		private void publishEventsFor(Func<data, bool> predicate)
		{
			var now = _now.UtcDateTime();

			_persons.LoadAll()
				.Select(person =>
				{
					var timeZone = person.PermissionInformation.DefaultTimeZone();
					var agentDate = new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));
					var currentPeriod = person.Period(agentDate);
					var previousPeriod = this.previousPeriod(person, currentPeriod);
					var time = timeOfChange(person.TerminalDate, currentPeriod, timeZone);

					return new data
					{
						person = person,
						previousPeriod = previousPeriod,
						currentPeriod = currentPeriod,
						agentDate = agentDate,
						timeOfChange = time,
						now = now
					};

				})
				.Where(predicate)
				.ForEach(data =>
				{

					var teamId = data.currentPeriod?.Team.Id;
					var siteId = data.currentPeriod?.Team.Site.Id;
					var businessUnitId = data.currentPeriod?.Team.Site.BusinessUnit.Id;

					var previousAssociation = (
						from p in data.person.PersonPeriodCollection
						where p != null &&
							  p.StartDate < data.agentDate
						select new Association
						{
							TeamId = p.Team.Id.Value,
							SiteId = p.Team.Site.Id.Value,
							BusinessUnitId = p.Team.Site.BusinessUnit.Id.Value
						}).ToArray();

					_eventPublisher.Publish(new PersonAssociationChangedEvent
					{
						Version = 2,
						PersonId = data.person.Id.Value,
						Timestamp = now,
						TeamId = teamId,
						SiteId = siteId,
						BusinessUnitId = businessUnitId,
						PreviousAssociation = previousAssociation,
						ExternalLogons = (data.currentPeriod?.ExternalLogOnCollection ?? Enumerable.Empty<IExternalLogOn>())
							.Select(x => new ExternalLogon
							{
								UserCode = x.AcdLogOnOriginalId,
								DataSourceId = x.DataSourceId
							}).ToArray()
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
			var timeZone = @event.TimeZoneInfoId != null
				? TimeZoneInfo.FindSystemTimeZoneById(@event.TimeZoneInfoId)
				: TimeZoneInfo.Utc;
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
				PreviousAssociation = @event.PreviousAssociations,
				ExternalLogons = @event.ExternalLogons
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
				PreviousAssociation = @event.PreviousAssociations,
				ExternalLogons = @event.ExternalLogons
			});
		}

		public void Handle(PersonPeriodChangedEvent @event)
		{
			_eventPublisher.Publish(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = @event.PersonId,
				Timestamp = @event.Timestamp,
				BusinessUnitId = @event.CurrentBusinessUnitId,
				SiteId = @event.CurrentSiteId,
				TeamId = @event.CurrentTeamId,
				PreviousAssociation = @event.PreviousAssociation,
				ExternalLogons = @event.ExternalLogons
			});
		}

	}
}