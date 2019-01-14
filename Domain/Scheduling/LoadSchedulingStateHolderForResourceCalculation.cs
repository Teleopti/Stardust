using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class LoadSchedulingStateHolderForResourceCalculation : ILoadSchedulingStateHolderForResourceCalculation
	{
		private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
		private readonly IPersonRepository _personRepository;
		private readonly SkillDayLoadHelper _skillDayLoadHelper;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public LoadSchedulingStateHolderForResourceCalculation(IPersonRepository personRepository,
			IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository,
			IWorkloadRepository workloadRepository, IScheduleStorage scheduleStorage,
			IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
			SkillDayLoadHelper skillDayLoadHelper)
		{
			_peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
			_personRepository = personRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_skillRepository = skillRepository;
			_workloadRepository = workloadRepository;
			_scheduleStorage = scheduleStorage;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder, Func<DateOnlyPeriod,ICollection<IPerson>> optionalLoadOrganizationFunc = null, bool loadLight = false)
		{
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);

			var skills = _skillRepository.FindAllWithSkillDays(dateOnlyPeriod).ToArray();
			_workloadRepository.LoadAll();
			
			schedulingResultStateHolder.LoadedAgents = optionalLoadOrganizationFunc != null ?
				optionalLoadOrganizationFunc(dateOnlyPeriod) : _personRepository.FindAllAgents(dateOnlyPeriod, false);

			var result = _peopleAndSkillLoaderDecider.Execute(scenario, period, requestedPersons);
			result.FilterPeople(schedulingResultStateHolder.LoadedAgents);
			schedulingResultStateHolder.SetSkills(result, skills);

			var personsToAdd = from p in requestedPersons
							   where !schedulingResultStateHolder.LoadedAgents.Contains(p)
			                   select p;
			personsToAdd.ForEach(p => schedulingResultStateHolder.LoadedAgents.Add(p));

			var personsProvider = schedulingResultStateHolder.LoadedAgents;
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

			schedulingResultStateHolder.Schedules =
				_scheduleStorage.FindSchedulesForPersons(
					scenario, 
					personsProvider,
                    scheduleDictionaryLoadOptions,
					new DateTimePeriod(period.StartDateTime.AddDays(-9), period.EndDateTime.AddDays(2)),
					requestedPersons, false);

			schedulingResultStateHolder.AllPersonAccounts = loadLight
				? new Dictionary<IPerson, IPersonAccountCollection>()
				: _personAbsenceAccountRepository.FindByUsers(requestedPersons);

			schedulingResultStateHolder.SkillDays =
		        _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, scenario);
		}
	}
}
