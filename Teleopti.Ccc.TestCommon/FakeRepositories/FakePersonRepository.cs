using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Auditing;
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

		public void Has(IPerson person)
		{
			_persons.Add(person);
		}

		public void Add(IPerson person)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPerson person)
		{
			throw new NotImplementedException();
		}

		public IPerson Get(Guid id)
		{
			return _persons.SingleOrDefault(x => x.Id.Equals(id));
		}

		public IList<IPerson> LoadAll()
		{
			throw new NotImplementedException();
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
			return new List<IPerson> {_persons.FirstOrDefault()};
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
			throw new NotImplementedException();
		}

		public IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleInOrganizationLight(DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId)
		{
			return _persons.Where(x => peopleId.Any(id => id.Equals(x.Id.Value))).ToArray();
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

		public int SaveLoginAttempt(LoginAttemptModel model)
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

		public IPerson LoadOne(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}