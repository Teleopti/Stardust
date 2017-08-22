using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		IHandleEvent<PersonDeletedEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _persons;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly INow _now;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IPersonAssociationPublisherCheckSumPersister _checkSums;

		public PersonAssociationChangedEventPublisher(
			IPersonRepository persons,
			ICurrentEventPublisher eventPublisher,
			INow now,
			IDistributedLockAcquirer distributedLock,
			IKeyValueStorePersister keyValueStore,
			IPersonAssociationPublisherCheckSumPersister checkSums)
		{
			_persons = persons;
			_eventPublisher = eventPublisher;
			_now = now;
			_distributedLock = distributedLock;
			_keyValueStore = keyValueStore;
			_checkSums = checkSums;
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
					publishForAllPersons();
				}
			});
		}

		[UnitOfWork]
		public virtual void Handle(TenantHourTickEvent @event)
		{
			publishForAllPersons();
		}

		[UnitOfWork]
		public virtual void Handle(PersonTeamChangedEvent @event)
		{
			publishForPerson(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonPeriodChangedEvent @event)
		{
			publishForPerson(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonTerminalDateChangedEvent @event)
		{
			publishForPerson(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			publishForPerson(@event.PersonId);
		}

		private void publishForAllPersons()
		{
			var now = _now.UtcDateTime();
			var checkSums = _checkSums.Get().ToLookup(c => c.PersonId);

			_persons
				.LoadAll()
				.ForEach(person =>
				{
					var personId = person.Id.Value;
					publishForPerson(personId, person, now, checkSums[personId].SingleOrDefault()?.CheckSum ?? 0);
				});
		}

		private void publishForPerson(Guid personId)
		{
			publishForPerson(
				personId,
				 _persons.Get(personId),
				_now.UtcDateTime(),
				_checkSums.Get(personId)?.CheckSum ?? 0
			);
		}

		private void publishForPerson(Guid personId, IPerson person, DateTime now, int checkSum)
		{
			var timeZone = person?.PermissionInformation.DefaultTimeZone();
			var agentDate = person != null ? new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone)) : (DateOnly?) null;
			var currentPeriod = person?.Period(agentDate.Value);
			var team = currentPeriod?.Team;
			var teamId = team?.Id;
			var siteId = team?.Site.Id;
			var siteName = team?.Site.Description.Name;
			var teamName = team?.Description.Name;
			var businessUnitId = team?.Site.BusinessUnit.Id;

			publishIfChanged(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				SiteName = siteName,
				TeamId = teamId,
				TeamName = teamName,
				Timestamp = now,
				ExternalLogons = (currentPeriod?.ExternalLogOnCollection ?? Enumerable.Empty<IExternalLogOn>())
					.Select(x => new ExternalLogon
					{
						UserCode = x.AcdLogOnOriginalId,
						DataSourceId = x.DataSourceId
					}).ToArray()
			}, checkSum);
		}

		private void publishIfChanged(PersonAssociationChangedEvent @event, int lastCheckSum)
		{
			var currentCheckSum = calculateCheckSum(@event);

			if (currentCheckSum == lastCheckSum)
				return;

			_checkSums.Persist(new PersonAssociationCheckSum
			{
				PersonId = @event.PersonId,
				CheckSum = currentCheckSum
			});

			_eventPublisher.Current().Publish(@event);
		}

		private int calculateCheckSum(PersonAssociationChangedEvent @event)
		{
			unchecked
			{
				var teamId = @event.TeamId;
				var hashCode = @event
					.ExternalLogons
					.EmptyIfNull()
					.Aggregate(0, (acc, el) =>
					{
						var hc = acc;
						hc = (hc*397) ^ el.DataSourceId.GetHashCode();
						hc = (hc*397) ^ el.UserCode.GetHashCode();
						return hc;
					});
				hashCode = (hashCode*397) ^ teamId.GetHashCode();

				return hashCode;
			}
		}

	}
}