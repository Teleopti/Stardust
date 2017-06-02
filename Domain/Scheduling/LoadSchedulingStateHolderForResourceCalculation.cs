using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class LoadSchedulingStateHolderForResourceCalculation : ILoadSchedulingStateHolderForResourceCalculation
	{
		private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly Func<IEnumerable<IPerson>, IPersonProvider> _personProviderMaker;

		public LoadSchedulingStateHolderForResourceCalculation(IPersonRepository personRepository,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository,
			IWorkloadRepository workloadRepository, IScheduleStorage scheduleStorage,
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
			ISkillDayLoadHelper skillDayLoadHelper)
			: this(personRepository, personAbsenceAccountRepository, skillRepository, workloadRepository, scheduleStorage,
				peopleAndSkillLoaderDecider, skillDayLoadHelper,
				p => new PersonsInOrganizationProvider(p))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")
		]
		public LoadSchedulingStateHolderForResourceCalculation(IPersonRepository personRepository,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository,
			IWorkloadRepository workloadRepository, IScheduleStorage scheduleStorage,
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
			ISkillDayLoadHelper skillDayLoadHelper, Func<IEnumerable<IPerson>, IPersonProvider> personProviderMaker)
		{
			_peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
			_personRepository = personRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_scheduleStorage = scheduleStorage;
			_personProviderMaker = personProviderMaker;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder, Func<DateOnlyPeriod,ICollection<IPerson>> optionalLoadOrganizationFunc = null, bool loadLight = false)
		{
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);

			var skills = _skillRepository.FindAllWithSkillDays(dateOnlyPeriod).ToArray();
			_workloadRepository.LoadAll();
			schedulingResultStateHolder.AddSkills(skills);

			schedulingResultStateHolder.PersonsInOrganization = optionalLoadOrganizationFunc != null ?
				optionalLoadOrganizationFunc(dateOnlyPeriod) : _personRepository.FindPeopleInOrganization(dateOnlyPeriod, false);

			var result = _peopleAndSkillLoaderDecider.Execute(scenario, period, requestedPersons);
			result.FilterPeople(schedulingResultStateHolder.PersonsInOrganization);
			result.FilterSkills(skills,schedulingResultStateHolder.RemoveSkill,s => schedulingResultStateHolder.AddSkills(s));

			var personsToAdd = from p in requestedPersons
							   where !schedulingResultStateHolder.PersonsInOrganization.Contains(p)
			                   select p;
			personsToAdd.ForEach(p => schedulingResultStateHolder.PersonsInOrganization.Add(p));

			var personsProvider = _personProviderMaker.Invoke(schedulingResultStateHolder.PersonsInOrganization);
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

			 var scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod(period.StartDateTime.AddDays(-9), period.EndDateTime.AddDays(2)));
			schedulingResultStateHolder.Schedules =
#pragma warning disable 618
				_scheduleStorage.FindSchedulesForPersons(
#pragma warning restore 618
					scheduleDateTimePeriod,
					scenario, 
					personsProvider,
                    scheduleDictionaryLoadOptions,
					requestedPersons); //rk - fattar inte, för rörigt. lägger till detta av nån anledning här

			schedulingResultStateHolder.AllPersonAccounts = loadLight
				? new Dictionary<IPerson, IPersonAccountCollection>()
				: _personAbsenceAccountRepository.FindByUsers(requestedPersons);

			schedulingResultStateHolder.SkillDays =
		        _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, scenario);
		}
	}
}
