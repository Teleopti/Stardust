using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	// feel free to continue implementing as see fit
	// im all for keeping it stupid (in the same manner as an .IgnoreArguments()) until smartness is required
	public class FakePersonRepository : IPersonRepository, IEnumerable<IPerson>
	{
		private readonly IPerson _person;

		public FakePersonRepository()
		{
			_person = PersonFactory.CreatePersonWithId();
		}

		public FakePersonRepository(IPerson person)
		{
			_person = person;
		}

		public void Add(IPerson entity)
		{
		}

		public void Remove(IPerson entity)
		{
			throw new NotImplementedException();
		}

		public IPerson Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPerson> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPerson Load(Guid id)
		{
			return _person;
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
			throw new NotImplementedException();
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
			return new[] {_person};
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
			return new List<IPerson> {_person}.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new List<IPerson> { _person }.GetEnumerator();
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
	}
}