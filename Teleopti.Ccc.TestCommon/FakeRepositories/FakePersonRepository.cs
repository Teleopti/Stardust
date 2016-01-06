using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

		public IPerson Has(IContract contract, IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, ISchedulePeriod schedulePeriod, ISkill skill)
		{
			var ppDate = new DateOnly(1950, 1, 1);
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			agent.AddPersonPeriod(new PersonPeriod(ppDate, new PersonContract(contract, partTimePercentage, contractSchedule), team));
      agent.AddSchedulePeriod(schedulePeriod);
			agent.AddSkill(skill, ppDate);
			_persons.Add(agent);
			return agent;
		}

		public FakePersonRepository Has(IPerson person, ITeam team, DateOnly startDate)
		{
			person.AddPersonPeriod(new PersonPeriod(startDate, new PersonContract(new Contract("."), new PartTimePercentage("."), new ContractSchedule(".")), team));
			_persons.Add(person);
			return this;
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

		public ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period)
		{
			var people = from per in _persons
						 let periods = per.PersonPeriods(period)
						 where periods.Any(personPeriod => personPeriod.Team == team)
						 select per;

			return people.ToList();
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

		public ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData)
		{
			return _persons.Where(p => p.PersonPeriods(period).Count > 0).ToArray();
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
			foreach (var agent in _persons)
			{
				foreach (var personPeriod in agent.PersonPeriods(period.ToDateOnlyPeriod(agent.PermissionInformation.DefaultTimeZone())))
				{
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						yield return new Tuple<Guid, Guid>(agent.Id.Value, personSkill.Skill.Id.Value);
					}
				}
			}
		}

		public IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period)
		{
			foreach (var agent in _persons)
			{
				foreach (var personPeriod in agent.PersonPeriods(period.ToDateOnlyPeriod(agent.PermissionInformation.DefaultTimeZone())))
				{
					var site = personPeriod.Team.Site;
					if (site.MaxSeats != null)
					{
						yield return agent.Id.Value;
					}
				}
			}
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

		public ICollection<IPerson> FindPeopleSimplify(IEnumerable<Guid> people)
		{
			return _persons.Where(x => people.Any(id => id == x.Id.GetValueOrDefault())).ToArray();
		}

		public IEnumerator<IPerson> GetEnumerator()
		{
			return _persons.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _persons.GetEnumerator();
		}

		public bool DoesPersonHaveExternalLogOn(DateOnly dateTime, Guid personId)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindAllWithRolesSortByName()
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleByEmail(string email)
		{
			throw new NotImplementedException();
		}
	}
}