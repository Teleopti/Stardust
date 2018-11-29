using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetPeopleProvider : IBudgetPeopleProvider
	{
		private readonly IPersonRepository _personRepository;

		public BudgetPeopleProvider(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public IEnumerable<IPerson> FindPeopleWithBudgetGroup(IBudgetGroup budgetGroup, DateOnly day)
		{
			var periodForQuery = new DateOnlyPeriod(day.AddDays(-1), day.AddDays(1));
			var people = _personRepository.FindAllAgentsLight(periodForQuery);

			foreach (IPerson person in people)
			{
				var personPeriod = person.Period(day);
				if (PersonPeriodBelongsToBudgetGroup(budgetGroup, personPeriod))
				{
					yield return person;
				}
			}
		}

		private static bool PersonPeriodBelongsToBudgetGroup(IBudgetGroup budgetGroup, IPersonPeriod personPeriod)
		{
			return personPeriod != null &&
			       budgetGroup.Equals(personPeriod.BudgetGroup);
		}
	}
}