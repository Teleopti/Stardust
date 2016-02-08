using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ILoadSchedulesForRequestWithoutResourceCalculation
    {
        void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons);
    }

    public class LoadSchedulesForRequestWithoutResourceCalculation : ILoadSchedulesForRequestWithoutResourceCalculation
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly IScheduleStorage _scheduleStorage;
        private readonly Func<IEnumerable<IPerson>, IPersonProvider> _personProviderMaker;

        public LoadSchedulesForRequestWithoutResourceCalculation(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleStorage scheduleStorage) : this(schedulingResultStateHolder, personAbsenceAccountRepository,scheduleStorage, p => new PersonProvider(p))
        {
        }

        public LoadSchedulesForRequestWithoutResourceCalculation(ISchedulingResultStateHolder schedulingResultStateHolder, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleStorage scheduleStorage, Func<IEnumerable<IPerson>, IPersonProvider> personProviderMaker)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _personAbsenceAccountRepository = personAbsenceAccountRepository;
            _scheduleStorage = scheduleStorage;
            _personProviderMaker = personProviderMaker;
        }

        public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons)
        {
            _schedulingResultStateHolder.PersonsInOrganization = requestedPersons.ToList();

            var personsProvider = _personProviderMaker.Invoke(_schedulingResultStateHolder.PersonsInOrganization);
            personsProvider.DoLoadByPerson = true;

            var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

            var scheduleDateTimePeriod = new ScheduleDateTimePeriod(period, requestedPersons);
            _schedulingResultStateHolder.Schedules =
                _scheduleStorage.FindSchedulesForPersons(
                    scheduleDateTimePeriod,
                    scenario,
                    personsProvider,
                    scheduleDictionaryLoadOptions,
                    requestedPersons); //rk - fattar inte, för rörigt. lägger till detta av nån anledning här

            _schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(requestedPersons);
            _schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
        }
    }
}