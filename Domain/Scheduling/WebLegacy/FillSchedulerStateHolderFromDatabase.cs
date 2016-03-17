using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderFromDatabase : IFillSchedulerStateHolder
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFixedStaffLoader _fixedStaffLoader;

		public FillSchedulerStateHolderFromDatabase(IScenarioRepository scenarioRepository, 
					ISkillDayLoadHelper skillDayLoadHelper, 
					ISkillRepository skillRepository, 
					IScheduleStorage scheduleStorage, 
					IPersonAbsenceAccountRepository personAbsenceAccountRepository, 
					IPeopleAndSkillLoaderDecider decider, 
					ICurrentTeleoptiPrincipal principal, 
					ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
					IRepositoryFactory repositoryFactory,
					IFixedStaffLoader fixedStaffLoader)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_scheduleStorage = scheduleStorage;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_decider = decider;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_fixedStaffLoader = fixedStaffLoader;
		}

		public void Fill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			//need to filter things like skilldays and agents here based on agentIds
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.Current().CurrentUnitOfWork(), _repositoryFactory);
			var people = _fixedStaffLoader.Load(period);
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;
			
			var allSkills = _skillRepository.FindAllWithSkillDays(period).ToArray();
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);

			var deciderResult = _decider.Execute(scenario, dateTimePeriod, people.AllPeople);
			deciderResult.FilterPeople(people.AllPeople);

			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);

			var stateHolder = schedulerStateHolder.SchedulingResultState;
			stateHolder.PersonsInOrganization = people.AllPeople;
			stateHolder.SkillDays = forecast;
			stateHolder.AddSkills(allSkills);
			deciderResult.FilterSkills(allSkills,stateHolder.RemoveSkill, s => stateHolder.AddSkills(s));
			
			schedulerStateHolder.SetRequestedScenario(scenario);
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			people.AllPeople.ForEach(schedulerStateHolder.AllPermittedPersons.Add);
			stateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(people.AllPeople);
			schedulerStateHolder.ResetFilteredPersons();
			schedulerStateHolder.LoadSchedules(_scheduleStorage, new PersonsInOrganizationProvider(people.AllPeople),
				new ScheduleDictionaryLoadOptions(true, false, false),
				new ScheduleDateTimePeriod(dateTimePeriod, people.AllPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)));
		}
	}
}