using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonRepository : IRepository<IPerson>
	{
		ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate);
		ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period);
		ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period);
		ICollection<IPerson> FindAllSortByName();
		ICollection<IPerson> FindAllAgents(DateOnlyPeriod period, bool includeRuleSetData);
		ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber);
		ICollection<IPerson> FindPeopleByEmploymentNumbers(IEnumerable<string> employmentNumbers);

		int NumberOfActiveAgents();
		IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period);

		IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period);

		ICollection<IPerson> FindAllAgentsLight(DateOnlyPeriod period);
		ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId);
		ICollection<IPerson> FindPeople(IEnumerable<IPerson> people);
		ICollection<IPerson> FindPeopleSimplify(IEnumerable<Guid> people);
		ICollection<IPerson> FindAllWithRolesSortByName();
		ICollection<IPerson> FindPeopleByEmail(string email);

		IPerson LoadPersonAndPermissions(Guid id);

		ICollection<IPerson> FindAllAgentsQuiteLight(DateOnlyPeriod period);

		IList<IPerson> FindUsers(DateOnly date);
		IList<IPerson> FindPeopleInPlanningGroup(PlanningGroup planningGroup, DateOnlyPeriod period);
		IList<IPerson> FindPersonsByKeywords(IEnumerable<string> keywords);

		void HardRemove(IPerson person);

		int CountPeopleInPlanningGroup(PlanningGroup planningGroup, DateOnlyPeriod period);
		IList<Guid> FindPeopleIdsInPlanningGroup(PlanningGroup planningGroup, DateOnlyPeriod period);
		IList<PersonBudgetGroupName> FindBudgetGroupNameForPeople(IList<Guid> personIds, DateTime startDate);
	}
}
