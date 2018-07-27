using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
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
		private readonly IPersonLoadAllWithAssociation _personsWithAssociation;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly INow _now;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IPersonAssociationPublisherCheckSumPersister _checkSums;
		private readonly IDisableDeletedFilter _deletedFilter;

		public PersonAssociationChangedEventPublisher(
			IPersonRepository persons,
			IPersonLoadAllWithAssociation personsWithAssociation,
			ICurrentEventPublisher eventPublisher,
			INow now,
			IDistributedLockAcquirer distributedLock,
			IKeyValueStorePersister keyValueStore,
			IPersonAssociationPublisherCheckSumPersister checkSums,
			IDisableDeletedFilter deletedFilter)
		{
			_persons = persons;
			_personsWithAssociation = personsWithAssociation;
			_eventPublisher = eventPublisher;
			_now = now;
			_distributedLock = distributedLock;
			_keyValueStore = keyValueStore;
			_checkSums = checkSums;
			_deletedFilter = deletedFilter;
		}

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

		[Attempts(5)]
		public void Handle(TenantHourTickEvent @event) => publishForAllPersons();

		public void Handle(PersonTeamChangedEvent @event) => PublishForPerson(@event.PersonId);
		public void Handle(PersonPeriodChangedEvent @event) => PublishForPerson(@event.PersonId);
		public void Handle(PersonTerminalDateChangedEvent @event) => PublishForPerson(@event.PersonId);
		public void Handle(PersonDeletedEvent @event) => PublishForPerson(@event.PersonId);

		private void publishForAllPersons()
		{
			var now = _now.UtcDateTime();
			var checkSums = LoadAllCheckSums();

			LoadAllPersons()
				.Batch(100)
				.ForEach(batch =>
					PublishForPersons(now, batch, id => checkSums[id].SingleOrDefault()?.CheckSum ?? 0)
				);
		}

		[UnitOfWork]
		protected virtual IEnumerable<IPerson> LoadAllPersons()
		{
			using (_deletedFilter.Disable())
				return _personsWithAssociation.LoadAll();
		}

		[UnitOfWork]
		protected virtual ILookup<Guid, PersonAssociationCheckSum> LoadAllCheckSums() => _checkSums.Get().ToLookup(c => c.PersonId);

		[UnitOfWork]
		protected virtual void PublishForPersons(DateTime now, IEnumerable<IPerson> persons, Func<Guid, int> checkSum)
		{
			persons.ForEach(person =>
			{
				var personId = person.Id.Value;
				publishForPerson(person.Id.Value, person, now, checkSum(personId));
			});
		}

		[UnitOfWork]
		protected virtual void PublishForPerson(Guid personId)
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
			var agentDate = person == null ? (DateOnly?) null : new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));
			var currentPeriod = person?.Period(agentDate.Value);
			var team = currentPeriod?.Team;
			var teamId = team?.Id;
			var siteId = team?.Site.Id;
			var siteName = team?.Site.Description.Name;
			var teamName = team?.Description.Name;
			var businessUnitId = team?.Site.BusinessUnit.Id;
			var externalLogons = (currentPeriod?.ExternalLogOnCollection ?? Enumerable.Empty<IExternalLogOn>())
				.Select(x => new ExternalLogon
				{
					UserCode = x.AcdLogOnOriginalId,
					DataSourceId = x.DataSourceId
				}).ToArray();

			var isDeleted = (person as IDeleteTag)?.IsDeleted ?? true;
			if (isDeleted)
			{
				teamId = null;
				siteId = null;
				siteName = null;
				teamName = null;
				businessUnitId = null;
				externalLogons = new ExternalLogon[] { };
			}

			publishIfChanged(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				BusinessUnitId = businessUnitId,
				SiteId = siteId,
				SiteName = siteName,
				TeamId = teamId,
				TeamName = teamName,
				Timestamp = now,
				ExternalLogons = externalLogons,
				FirstName = person?.Name.FirstName,
				LastName = person?.Name.LastName,
				EmploymentNumber = person?.EmploymentNumber
			}, checkSum);
		}

		private void publishIfChanged(PersonAssociationChangedEvent @event, int lastCheckSum)
		{
			var currentCheckSum = calculateCheckSum(@event);

			if (currentCheckSum == lastCheckSum)
				return;

			var personId = @event.PersonId;
			_checkSums.Persist(new PersonAssociationCheckSum
			{
				PersonId = personId,
				CheckSum = currentCheckSum
			});

			_eventPublisher.Current().Publish(@event);
		}

		private int calculateCheckSum(PersonAssociationChangedEvent @event)
		{
			unchecked
			{
				var hashCode = 0;
				var externalLogonsHashCode = @event
					.ExternalLogons
					.EmptyIfNull()
					.Aggregate(0, (acc, el) =>
					{
						var hc = acc;
						hc = (hc * 397) ^ el.DataSourceId.GetHashCode();
						hc = (hc * 397) ^ el.UserCode.GetHashCode();
						return hc;
					});
				hashCode = (hashCode * 397) ^ externalLogonsHashCode;
				hashCode = (hashCode * 397) ^ @event.TeamId.GetHashCode();
				hashCode = (hashCode * 397) ^ (@event.FirstName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (@event.LastName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (@event.EmploymentNumber?.GetHashCode() ?? 0);

				return hashCode;
			}
		}
	}

	public class X
	{
		public int? something;
		public int? something2;
		public string something3;

		protected bool Equals(X other)
		{
			return something == other.something && something2 == other.something2 && string.Equals(something3, other.something3);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((X) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = something.GetHashCode();
				hashCode = (hashCode * 397) ^ something2.GetHashCode();
				hashCode = (hashCode * 397) ^ (something3 != null ? something3.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}