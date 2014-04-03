using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class LoadSchedulingStateHolderForResourceCalculation : ILoadSchedulingStateHolderForResourceCalculation
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly Func<IEnumerable<IPerson>, IPersonProvider> _personProviderMaker;

		public LoadSchedulingStateHolderForResourceCalculation(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduleRepository scheduleRepository, ISchedulingResultStateHolder schedulingResultStateHolder, IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider, ISkillDayLoadHelper skillDayLoadHelper) : this(personRepository, personAbsenceAccountRepository, skillRepository, workloadRepository, scheduleRepository,
			 schedulingResultStateHolder, peopleAndSkillLoaderDecider, skillDayLoadHelper, p => new PersonsInOrganizationProvider(p))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public LoadSchedulingStateHolderForResourceCalculation(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduleRepository scheduleRepository, ISchedulingResultStateHolder schedulingResultStateHolder, IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider, ISkillDayLoadHelper skillDayLoadHelper, Func<IEnumerable<IPerson>, IPersonProvider> personProviderMaker)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
			_personRepository = personRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_scheduleRepository = scheduleRepository;
			_personProviderMaker = personProviderMaker;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons)
		{
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);

			_schedulingResultStateHolder.PersonsInOrganization = _personRepository.FindPeopleInOrganization(dateOnlyPeriod, false);

			var skills = _skillRepository.FindAllWithSkillDays(dateOnlyPeriod);
			_workloadRepository.LoadAll();

			_peopleAndSkillLoaderDecider.Execute(scenario, period, requestedPersons);
			_peopleAndSkillLoaderDecider.FilterPeople(_schedulingResultStateHolder.PersonsInOrganization);
			_peopleAndSkillLoaderDecider.FilterSkills(skills);

			var personsToAdd = from p in requestedPersons
							   where !_schedulingResultStateHolder.PersonsInOrganization.Contains(p)
			                   select p;
			personsToAdd.ForEach(p => _schedulingResultStateHolder.PersonsInOrganization.Add(p));

			var personsProvider = _personProviderMaker.Invoke(_schedulingResultStateHolder.PersonsInOrganization);
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

			 var scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod(period.StartDateTime.AddDays(-9), period.EndDateTime.AddDays(2)));
			_schedulingResultStateHolder.Schedules =
				_scheduleRepository.FindSchedulesForPersons(
					scheduleDateTimePeriod,
					scenario, 
					personsProvider,
                    scheduleDictionaryLoadOptions,
					requestedPersons); //rk - fattar inte, för rörigt. lägger till detta av nån anledning här

			_schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(requestedPersons);

			if (skills != null)
			{
			    skills.ForEach(_schedulingResultStateHolder.Skills.Add);
			}
			
		    _schedulingResultStateHolder.SkillDays =
		        _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, scenario);
		}
	}
}
