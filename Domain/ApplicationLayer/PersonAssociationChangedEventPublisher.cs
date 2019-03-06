using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

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
			_distributedLock.TryLockForTypeOfAnd(this, "trigger", () =>
			{
				if (!_keyValueStore.Get("PersonAssociationChangedPublishTrigger", false))
					return;
				if (publishForAllPersons())
					_keyValueStore.Update("PersonAssociationChangedPublishTrigger", false);
			});
		}

		public void Handle(TenantHourTickEvent @event) => publishForAllPersons();

		public void Handle(PersonTeamChangedEvent @event) => publishForOnePerson(@event.PersonId);
		public void Handle(PersonPeriodChangedEvent @event) => publishForOnePerson(@event.PersonId);
		public void Handle(PersonTerminalDateChangedEvent @event) => publishForOnePerson(@event.PersonId);
		public void Handle(PersonDeletedEvent @event) => publishForOnePerson(@event.PersonId);

		private bool publishForAllPersons()
		{
			var ran = false;
			_distributedLock.TryLockForTypeOfAnd(this, "run", () =>
			{
				ran = true;
				var now = _now.UtcDateTime();
				var loadedCheckSums = LoadAllCheckSums();

				LoadAllPersons()
					.Batch(100)
					.ForEach(batch =>
						{
							var checkSums = publishForPersons(
								now,
								batch.Select(x => (Id: x.Id.Value, Person: x)),
								id => loadedCheckSums[id].SingleOrDefault()?.CheckSum ?? 0
							);
							UpdateCheckSums(checkSums);
						}
					);
			});
			return ran;
		}

		private void publishForOnePerson(Guid personId)
		{
			var checkSums = PublishForPerson(personId);
			UpdateCheckSums(checkSums);
		}

		[UnitOfWork]
		protected virtual IEnumerable<IPerson> LoadAllPersons()
		{
			using (_deletedFilter.Disable())
				return _personsWithAssociation.LoadAll();
		}

		[UnitOfWork]
		protected virtual ILookup<Guid, PersonAssociationCheckSum> LoadAllCheckSums() =>
			_checkSums.Get().ToLookup(c => c.PersonId);

		[UnitOfWork]
		protected virtual IEnumerable<PersonAssociationCheckSum> PublishForPerson(Guid personId)
		{
			IPerson person;
			using (_deletedFilter.Disable())
				person = _persons.Get(personId);
			return publishForPersons(
				_now.UtcDateTime(),
				person.AsArray().Select(x => (Id: personId, Person: x)),
				id => _checkSums.Get(id)?.CheckSum ?? 0
			);
		}

		private IEnumerable<PersonAssociationCheckSum> publishForPersons(DateTime now, IEnumerable<(Guid Id, IPerson Person)> persons, Func<Guid, int> checkSum)
		{
			var checkSums = persons
				.Select(x => makeEvent(x.Id, x.Person, now))
				.Select(x => new
				{
					Event = x,
					CheckSum = calculateCheckSum(x)
				})
				.Where(x => x.CheckSum != checkSum(x.Event.PersonId))
				.Select(x => new
				{
					x.Event,
					CheckSum = new PersonAssociationCheckSum()
					{
						CheckSum = x.CheckSum,
						PersonId = x.Event.PersonId
					}
				})
				.ToArray();

			_eventPublisher.Current().Publish(checkSums.Select(x => x.Event).Cast<IEvent>().ToArray());

			return checkSums.Select(x => x.CheckSum);
		}

		[UnitOfWork]
		protected virtual void UpdateCheckSums(IEnumerable<PersonAssociationCheckSum> checkSums) =>
			_checkSums.Persist(checkSums);

		private PersonAssociationChangedEvent makeEvent(Guid personId, IPerson person, DateTime now)
		{
			var timeZone = person?.PermissionInformation.DefaultTimeZone();
			var agentDate = person == null ? (DateOnly?) null : new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(now, timeZone));
			var currentPeriod = person?.Period(agentDate.Value);
			var team = currentPeriod?.Team;
			var teamId = team?.Id;
			var siteId = team?.Site.Id;
			var siteName = team?.Site.Description.Name;
			var teamName = team?.Description.Name;
			var businessUnitId = team?.Site.GetOrFillWithBusinessUnit_DONTUSE().Id;
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

			return new PersonAssociationChangedEvent
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
				EmploymentNumber = person?.EmploymentNumber,
				TimeZone = person?.PermissionInformation.DefaultTimeZone().Id,
			};
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
				hashCode = (hashCode * 397) ^ (@event.TimeZone?.GetHashCode() ?? 0);

				return hashCode;
			}
		}
	}
}