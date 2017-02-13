using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRepository : IPersonRepository, IEnumerable<IPerson>
	{
		private readonly FakeStorage _storage;

		public FakePersonRepository(FakeStorage storage)
		{
			_storage = storage;
		}

		public void Has(IPerson person)
		{
			_storage.Add(person);
		}

		public Person Has(IContract contract, IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, ISchedulePeriod schedulePeriod, IRuleSetBag ruleSetBag, params ISkill[] skills)
		{
			var ppDate = new DateOnly(1950, 1, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
			var period = new PersonPeriod(ppDate, new PersonContract(contract, partTimePercentage, contractSchedule), team)
			{
				RuleSetBag = ruleSetBag
			};
			agent.AddPersonPeriod(period);
			if (schedulePeriod != null)
			{
				agent.AddSchedulePeriod(schedulePeriod);
			}
			foreach (var skill in skills)
			{
				agent.AddSkill(skill, ppDate);
			}
			_storage.Add(agent);
			return agent;
		}

		public Person Has(IContract contract, IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, ISchedulePeriod schedulePeriod, IWorkShiftRuleSet ruleSet, params ISkill[] skills)
		{
			return Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, new RuleSetBag(ruleSet), skills);
		}

		public Person Has(IContract contract, IContractSchedule contractSchedule, IPartTimePercentage partTimePercentage, ITeam team, ISchedulePeriod schedulePeriod, params ISkill[] skills)
		{
			return Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, (IRuleSetBag)null, skills);
		}

		public Person Has(IContract contract, ISchedulePeriod schedulePeriod, params ISkill[] skills)
		{
			return Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("_") }, schedulePeriod, skills);
		}

		public Person Has(params ISkill[] skills)
		{
			return Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("_") }, new SchedulePeriod(DateOnly.MinValue, SchedulePeriodType.Day, 1), skills);
		}

		public Person Has(IContract contract, ISchedulePeriod schedulePeriod, IWorkShiftRuleSet ruleSet, params ISkill[] skills)
		{
			return Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("_") }, schedulePeriod, ruleSet, skills);
		}

		public IPerson Has(IPerson person, ITeam team, DateOnly startDate)
		{
			person.AddPersonPeriod(new PersonPeriod(startDate, new PersonContract(new Contract("."), new PartTimePercentage("."), new ContractSchedule(".")), team));
			_storage.Add(person);
			return person;
		}

		public void Add(IPerson person)
		{
			_storage.Add(person);
		}

		public void Remove(IPerson person)
		{
			_storage.Remove(person);
		}

		public IPerson Get(Guid id)
		{
			return _storage.Get<IPerson>(id);
		}

		public IList<IPerson> LoadAll()
		{
			return _storage.LoadAll<IPerson>().ToList();
		}

		public IPerson Load(Guid id)
		{
			return _storage.LoadAll<IPerson>().SingleOrDefault(x => x.Id.Equals(id));
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate)
		{
			return _storage.LoadAll<IPerson>().ToList();
		}

		public ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period)
		{
			var people = from per in _storage.LoadAll<IPerson>()
						 let periods = per.PersonPeriods(period)
				where periods.Any(personPeriod => personPeriod.Team == team)
				select per;

			return people.ToList();
		}

		public ICollection<IPerson> FindPeopleBelongTeams(ITeam[] teams, DateOnlyPeriod period)
		{
			var people = from per in _storage.LoadAll<IPerson>()
						 let periods = per.PersonPeriods(period)
						 where periods.Any(personPeriod => teams.Contains(personPeriod.Team))
						 select per;

			return people.ToList();
		}

		public ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period)
		{

			var people = from per in _storage.LoadAll<IPerson>()
						 let periods = per.PersonPeriods(period)
				where periods.Any(personPeriod => personPeriod.Team == team)
				select per;

			return people.ToList();

		}

		public ICollection<IPerson> FindAllSortByName()
		{
			return _storage.LoadAll<IPerson>().OrderBy(p => p.Name.ToString()).ToArray();
		}

		public ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData)
		{
			return _storage.LoadAll<IPerson>().Where(p => p.PersonPeriods(period).Count > 0).ToList();
		}

		public ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber)
		{
			return _storage.LoadAll<IPerson>().Where(p => p.EmploymentNumber == employmentNumber).ToArray();
		}

		public ICollection<IPerson> FindPeopleByEmploymentNumbers(IEnumerable<string> employmentNumbers)
		{
			return _storage.LoadAll<IPerson>().Where(p => employmentNumbers.Contains(p.EmploymentNumber)).ToArray();
		}

		public int NumberOfActiveAgents()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period)
		{
			foreach (var agent in _storage.LoadAll<IPerson>())
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
			foreach (var agent in _storage.LoadAll<IPerson>())
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
			return _storage.LoadAll<IPerson>().Where(p => p.PersonPeriods(period).Count > 0).ToArray();
		}

		public ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId)
		{
			return _storage.LoadAll<IPerson>().Where(x => peopleId.Any(id => id == x.Id.GetValueOrDefault())).ToArray();
		}

		public ICollection<IPerson> FindPeople(IEnumerable<IPerson> people)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleSimplify(IEnumerable<Guid> people)
		{
			return _storage.LoadAll<IPerson>().Where(x => people.Any(id => id == x.Id.GetValueOrDefault())).ToArray();
		}

		public IEnumerator<IPerson> GetEnumerator()
		{
			return _storage.LoadAll<IPerson>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _storage.LoadAll<IPerson>().GetEnumerator();
		}

		public ICollection<IPerson> FindAllWithRolesSortByName()
		{
			throw new NotImplementedException();
		}

		public ICollection<IPerson> FindPeopleByEmail(string email)
		{
			return _storage.LoadAll<IPerson>().Where(p => p.Email == email).ToArray();
		}

		public IPerson LoadPersonAndPermissions(Guid id)
		{
			return Get(id);
		}

		public ICollection<IPerson> FindPeopleInOrganizationQuiteLight(DateOnlyPeriod period)
		{
			return _storage.LoadAll<IPerson>().Where(p => p.PersonPeriods(period).Count > 0).ToArray();
		}

		public IList<IPerson> FindUsers(DateOnly date)
		{
			return _storage.LoadAll<IPerson>().Where(
				p => !p.IsAgent(date) && p.TerminalDate.GetValueOrDefault(DateOnly.MaxValue) >= date).ToArray();
		}

		public IList<IPerson> FindPeopleInAgentGroup(DateOnly date, IAgentGroup agentGroup)
		{
			throw new NotImplementedException();
		}
	}
}