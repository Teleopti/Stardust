using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillRepository _skillRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRequiredScheduleHelper _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly Func<IScheduleTagSetter> _scheduleTagSetter;

		public ScheduleController(IScenarioRepository scenarioRepository, ISkillDayLoadHelper skillDayLoadHelper, ISkillRepository skillRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IDayOffTemplateRepository dayOffTemplateRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IActivityRepository activityRepository, IPeopleAndSkillLoaderDecider decider, ICurrentTeleoptiPrincipal principal, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, Func<IFixedStaffSchedulingService> fixedStaffSchedulingService, Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder, IRequiredScheduleHelper requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder, Func<IScheduleTagSetter> scheduleTagSetter)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_activityRepository = activityRepository;
			_decider = decider;
			_principal = principal;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleTagSetter = scheduleTagSetter;
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff"), Authorize, UnitOfWork]
		public virtual IHttpActionResult FixedStaff([FromBody]FixedStaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;

			makeSureActivitiesAreLoaded();
			var allPeople = _personRepository.FindPeopleInOrganizationLight(period).ToList();
			var selectedPeople =
				allPeople.Where(
					p =>
						p.PersonPeriods(period)
							.Any(
								pp =>
									pp.PersonContract != null && pp.PersonContract.Contract != null &&
									pp.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)).ToList();
			var allSkills = _skillRepository.FindAllWithSkillDays(period);
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);

			_decider.Execute(scenario, dateTimePeriod, selectedPeople);

			_decider.FilterSkills(allSkills);
			_decider.FilterPeople(allPeople);

			var forecast = _skillDayLoadHelper.LoadSchedulerSkillDays(period, allSkills, scenario);

			var schedulerStateHolder = _schedulerStateHolder();
			var stateHolder = schedulerStateHolder.SchedulingResultState;
			stateHolder.PersonsInOrganization = allPeople;
			stateHolder.Schedules =
				_scheduleRepository.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(dateTimePeriod, selectedPeople, new SchedulerRangeToLoadCalculator(dateTimePeriod)),
					scenario, new PersonsInOrganizationProvider(allPeople), new ScheduleDictionaryLoadOptions(true, false, false),
					selectedPeople);
			stateHolder.SkillDays = forecast;
			allSkills.ForEach(stateHolder.Skills.Add);
			schedulerStateHolder.SetRequestedScenario(scenario);
			schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			allPeople.ForEach(schedulerStateHolder.AllPermittedPersons.Add);
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork(), new RepositoryFactory());
			stateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(selectedPeople);
			schedulerStateHolder.ResetFilteredPersons();

			_scheduleTagSetter().ChangeTagToSet(NullScheduleTag.Instance);
			
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (selectedPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}

			var daysScheduled = 0;
			EventHandler<SchedulingServiceBaseEventArgs> schedulingServiceOnDayScheduled = (sender, args) => daysScheduled++;
			_fixedStaffSchedulingService().DayScheduled += schedulingServiceOnDayScheduled;

			_scheduleCommand().Execute(new OptimizerOriginalPreferences(new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = false,
				DayOffTemplate = _dayOffTemplateRepository.FindAllDayOffsSortByDescription()[0],
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupPageForShiftCategoryFairness = new GroupPageLight {Key = "Main", Name = UserTexts.Resources.Main},
				TagToUseOnScheduling = NullScheduleTag.Instance
			}), new NoBackgroundWorker(), schedulerStateHolder, allSchedules, _groupPagePerDateHolder(),
				_requiredScheduleHelper,
				new OptimizationPreferences());
			_fixedStaffSchedulingService().DayScheduled -= schedulingServiceOnDayScheduled;

			return Ok(new SchedulingResultModel{DaysScheduled = daysScheduled});
		}

		private void makeSureActivitiesAreLoaded()
		{
			_activityRepository.LoadAll();
		}
	}

	public class FixedStaffSchedulingInput
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}