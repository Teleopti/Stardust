using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Common
{
	public class CalculatePersonAccount
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenario;
		
		
		public CalculatePersonAccount(IPersonAbsenceAccountRepository personAbsenceAccountRepository, ScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenario)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenario = scenario;
		}

		public void Calculate(PersonEmploymentChangedEvent @event)
		{
			var person =_personRepository.Load(@event.PersonId);
		    var personAccountCollection =	_personAbsenceAccountRepository.Find(person);
			var allPersonAccounts = personAccountCollection.AllPersonAccounts().Where(x =>
				(x.StartDate >= @event.FromDate) || x.Period().Intersection(@event.FromDate.ToDateOnlyPeriod()).HasValue);
			allPersonAccounts.ForEach(personAccount =>
			{
				personAccount.CalculateUsed(_scheduleStorage,_scenario.LoadDefaultScenario());
			});
		}
	}
}
