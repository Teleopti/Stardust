﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRepository : IPersonRepository, IEnumerable<IPerson>
	{
		private readonly IList<IPerson> _persons = new List<IPerson>();

		public FakePersonRepository()
		{
			Has(PersonFactory.CreatePersonWithId());
		}

		public FakePersonRepository(IPerson person)
		{
			Has(person);
		}

		public FakePersonRepository(params IPerson[] persons)
		{
			persons.ForEach (Add);
		}

		public void Has(IPerson person)
		{
			_persons.Add(person);
		}

		public void Add(IPerson person)
		{
			_persons.Add(person);
		}

		public void Remove(IPerson person)
		{
			_persons.Remove (person);
		}

		public IPerson Get(Guid id)
		{
			return _persons.SingleOrDefault(x => x.Id.Equals(id));
		}

		public IList<IPerson> LoadAll()
		{
			return _persons;
		}

		public IPerson Load(Guid id)
		{
			return _persons.SingleOrDefault(x => x.Id.Equals(id));
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPerson> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IPerson TryFindBasicAuthenticatedPerson(string logOnName)
		{
			throw new NotImplementedException();
		}

		public bool TryFindIdentityAuthenticatedPerson(string identity, out IPerson foundPerson)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period)
		{

			var people = from per in _persons
				let periods = per.PersonPeriods (period)
				where periods.Any(personPeriod => personPeriod.Team == team)
				select per;

			return people.ToList();

		}

		public ICollection<IPerson> FindAllSortByName()
		{
			throw new NotImplementedException();
		}

		public IPerson LoadPermissionData(IPerson person)
		{
			throw new NotImplementedException();
		}

		public IPerson LoadPermissionDataWithoutReassociate(IPerson person)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData)
		{
			throw new NotImplementedException();
		}

		public IList<IPerson> FindPersonsWithGivenUserCredentials(IList<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber)
		{
			throw new NotImplementedException();
		}

		public int NumberOfActiveAgents()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period)
		{
			return new List<Tuple<Guid, Guid>>();
		}

		public IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period)
		{
			return new List<Guid> {Guid.NewGuid()};
		}

		public ICollection<IPerson> FindPeopleInOrganizationLight(DateOnlyPeriod period)
		{
			return _persons.Where(p => p.PersonPeriods(period).Count > 0).ToArray();
		}

		public ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId)
		{
			return _persons.Where(x => peopleId.Any(id => id == x.Id.GetValueOrDefault())).ToArray();
		}

		public ICollection<IPerson> FindPeople(IEnumerable<IPerson> people)
		{
			throw new NotImplementedException();
		}

		public bool DoesIdentityExists(string identity)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPerson> FindPossibleShiftTrades(IPerson loggedOnUser)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IPerson> GetEnumerator()
		{
			return _persons.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _persons.GetEnumerator();
		}

		public IPerson LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public bool DoesPersonHaveExternalLogOn(DateOnly dateTime, Guid personId)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindAllSortByName(bool includeSuperUserThatAlsoIsAgent)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindAllWithRolesSortByName()
		{
			throw new NotImplementedException();
		}

		public IPerson LoadPersonAndPermissions(Guid id)
		{
			return _persons.SingleOrDefault(x => x.Id.Equals(id));
		}
	}
}