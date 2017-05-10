using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonRepository : IRepository<IPerson>
	{
		ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate);
		ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period);
		ICollection<IPerson> FindPeopleBelongTeams(ITeam[] teams, DateOnlyPeriod period);
		ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period);
		ICollection<IPerson> FindAllSortByName();
		ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData);
		ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber);
		ICollection<IPerson> FindPeopleByEmploymentNumbers(IEnumerable<string> employmentNumbers);

		int NumberOfActiveAgents();
		IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period);

		IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period);

		ICollection<IPerson> FindPeopleInOrganizationLight(DateOnlyPeriod period);
		ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId);
		ICollection<IPerson> FindPeople(IEnumerable<IPerson> people);
		ICollection<IPerson> FindPeopleSimplify(IEnumerable<Guid> people);
		ICollection<IPerson> FindAllWithRolesSortByName();
		ICollection<IPerson> FindPeopleByEmail(string email);

		IPerson LoadPersonAndPermissions(Guid id);

		ICollection<IPerson> FindPeopleInOrganizationQuiteLight(DateOnlyPeriod period);

		IList<IPerson> FindUsers(DateOnly date);
		IList<IPerson> FindPeopleInAgentGroup(IAgentGroup agentGroup, DateOnlyPeriod period);

		void HardRemove(IPerson person);

		int CountPeopleInAgentGroup(IAgentGroup agentGroup, DateOnlyPeriod period);
		IList<Guid> FindPeopleIdsInAgentGroup(IAgentGroup agentGroup, DateOnlyPeriod period);
	}
}
