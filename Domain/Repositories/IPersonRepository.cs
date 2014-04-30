using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	///<summary>
	/// Interface for Person Repository
	///</summary>
	public interface IPersonRepository : IRepository<IPerson>
	{
		/// <summary>
		/// Tries to find a basic authenticated user.
		/// </summary>
		/// <param name="logOnName">The logOnName.</param>
		/// <returns></returns>
		IPerson TryFindBasicAuthenticatedPerson(string logOnName);

		/// <summary>
		/// Tries to find a windows authenticated person.
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="foundPerson">The found person.</param>
		/// <returns></returns>
		bool TryFindIdentityAuthenticatedPerson(string identity, out IPerson foundPerson);


		/// <summary>
		/// Loads all person with hierarchy data sort by name. Skip persons with teminal date earlier than specifyed
		/// </summary>
		/// <param name="earliestTerminalDate">The earliest terminal date.</param>
		/// <returns></returns>
		ICollection<IPerson> LoadAllPeopleWithHierarchyDataSortByName(DateOnly earliestTerminalDate);

		/// <summary>
		/// Finds the people belong team.
		/// </summary>
		/// <param name="team">The team.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-03-13
		/// </remarks>
		ICollection<IPerson> FindPeopleBelongTeam(ITeam team, DateOnlyPeriod period);

		ICollection<IPerson> FindPeopleBelongTeamWithSchedulePeriod(ITeam team, DateOnlyPeriod period);

		/// <summary>
		/// Finds all persons with name sorted.
		/// </summary>
		/// <returns></returns>
		ICollection<IPerson> FindAllSortByName();

		/// <summary>
		/// Finds the persons in organization.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="includeRuleSetData">if set to <c>true</c> [include rule set data].</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-22
		/// </remarks>
		ICollection<IPerson> FindPeopleInOrganization(DateOnlyPeriod period, bool includeRuleSetData);

		/// <summary>
		/// Finds the persons with given user credentials.
		/// </summary>
		/// <param name="persons">The person collection.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by:VirajS
		/// Created date: 2008-10-29
		/// </remarks>
		IList<IPerson> FindPersonsWithGivenUserCredentials(IList<IPerson> persons);

		/// <summary>
		/// Finds the people by employment number
		/// </summary>
		/// <param name="employmentNumber">The employment number.</param>
		/// <returns></returns>
		ICollection<IPerson> FindPeopleByEmploymentNumber(string employmentNumber);

		/// <summary>
		/// Gets the number of active agents. Used for checking against license limits
		/// </summary>
		/// <value>The number of active agents.</value>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-12-02
		/// </remarks>
		int NumberOfActiveAgents();

		/// <summary>
		/// Gets the people and its depended skills.
		/// </summary>
		/// <param name="scenario">The scenario.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-12-08
		/// </remarks>
		IEnumerable<Tuple<Guid, Guid>> PeopleSkillMatrix(IScenario scenario, DateTimePeriod period);

		IEnumerable<Guid> PeopleSiteMatrix(DateTimePeriod period);

		ICollection<IPerson> FindPeopleInOrganizationLight(DateOnlyPeriod period);
		ICollection<IPerson> FindPeople(IEnumerable<Guid> peopleId);
		ICollection<IPerson> FindPeople(IEnumerable<IPerson> people);
		bool DoesIdentityExists(string identity);
		int SaveLoginAttempt(LoginAttemptModel model);
	    bool DoesPersonHaveExternalLogOn(DateOnly dateTime, Guid personId);
	    ICollection<IPerson> FindAllSortByName(bool includeSuperUserThatAlsoIsAgent);
	}
}
