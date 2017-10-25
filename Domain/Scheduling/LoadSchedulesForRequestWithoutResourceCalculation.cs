using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ILoadSchedulesForRequestWithoutResourceCalculation
    {
        void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder);
    }

    public class LoadSchedulesForRequestWithoutResourceCalculation : ILoadSchedulesForRequestWithoutResourceCalculation
    {
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly Func<IEnumerable<IPerson>, IPersonProvider> _personProviderMaker;

	    public LoadSchedulesForRequestWithoutResourceCalculation(
		    IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleStorage scheduleStorage)
		    : this(personAbsenceAccountRepository, scheduleStorage, p => new PersonProvider(p))
	    {
	    }

	    public LoadSchedulesForRequestWithoutResourceCalculation(
		    IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleStorage scheduleStorage,
			    Func<IEnumerable<IPerson>, IPersonProvider> personProviderMaker)
	    {
		    _personAbsenceAccountRepository = personAbsenceAccountRepository;
		    _scheduleStorage = scheduleStorage;
		    _personProviderMaker = personProviderMaker;
	    }

	    public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons,
		    ISchedulingResultStateHolder schedulingResultStateHolder)
	    {
		    schedulingResultStateHolder.PersonsInOrganization = requestedPersons.ToList();

		    var personsProvider = _personProviderMaker.Invoke(schedulingResultStateHolder.PersonsInOrganization);
		    personsProvider.DoLoadByPerson = true;

		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

		    schedulingResultStateHolder.Schedules =
#pragma warning disable 618
			    _scheduleStorage.FindSchedulesForPersons(
#pragma warning restore 618
					scenario,
				    personsProvider,
				    scheduleDictionaryLoadOptions,
				    period,
					requestedPersons, true);

		    schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(requestedPersons);
		    schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
	    }
    }
}