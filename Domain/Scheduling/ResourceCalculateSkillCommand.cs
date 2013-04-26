using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IResourceCalculateSkillCommand
	{
		void Execute(IScenario scenario, DateTimePeriod period, ISkill skill);
	}

	public class ResourceCalculateSkillCommand : IResourceCalculateSkillCommand
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ISkillLoaderDecider _skillLoaderDecider;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly Func<IEnumerable<IPerson>, IPersonProvider> _personProviderMaker;

		public ResourceCalculateSkillCommand(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduleRepository scheduleRepository, ISchedulingResultStateHolder schedulingResultStateHolder, ISkillLoaderDecider skillLoaderDecider, ISkillDayLoadHelper skillDayLoadHelper) : this(personRepository, personAbsenceAccountRepository, skillRepository, workloadRepository, scheduleRepository,
			 schedulingResultStateHolder, skillLoaderDecider, skillDayLoadHelper, p => new PersonsInOrganizationProvider(p))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ResourceCalculateSkillCommand(IPersonRepository personRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, IScheduleRepository scheduleRepository, ISchedulingResultStateHolder schedulingResultStateHolder, ISkillLoaderDecider skillLoaderDecider, ISkillDayLoadHelper skillDayLoadHelper, Func<IEnumerable<IPerson>, IPersonProvider> personProviderMaker)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_skillLoaderDecider = skillLoaderDecider;
			_personRepository = personRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_scheduleRepository = scheduleRepository;
			_personProviderMaker = personProviderMaker;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, ISkill skill)
		{
			var dateOnlyPeriod = period.ToDateOnlyPeriod(skill.TimeZone);

			_schedulingResultStateHolder.PersonsInOrganization = _personRepository.FindPeopleInOrganization(dateOnlyPeriod, true);

			var skills = _skillRepository.FindAllWithSkillDays(dateOnlyPeriod);
			_workloadRepository.LoadAll();

			_skillLoaderDecider.Execute(scenario, period, skill);
			_skillLoaderDecider.FilterPeople(_schedulingResultStateHolder.PersonsInOrganization);
			_skillLoaderDecider.FilterSkills(skills);

			var personsProvider = _personProviderMaker.Invoke(_schedulingResultStateHolder.PersonsInOrganization);
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);

			var scheduleDateTimePeriod = new ScheduleDateTimePeriod(period, _schedulingResultStateHolder.PersonsInOrganization);
			_schedulingResultStateHolder.Schedules =
				_scheduleRepository.FindSchedulesForPersons(
					scheduleDateTimePeriod,
					scenario, 
					personsProvider,
                    scheduleDictionaryLoadOptions,
					_schedulingResultStateHolder.PersonsInOrganization); //rk - fattar inte, för rörigt. lägger till detta av nån anledning här

			_schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(_schedulingResultStateHolder.PersonsInOrganization);

			if (skills != null)
				skills.ForEach(_schedulingResultStateHolder.Skills.Add);

		    _schedulingResultStateHolder.SkillDays =
		        _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, scenario);

		}
	}
}
