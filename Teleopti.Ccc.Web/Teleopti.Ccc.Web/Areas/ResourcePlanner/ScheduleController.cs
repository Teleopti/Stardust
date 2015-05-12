using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.WebTest.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly FixedStaffLoader _fixedStaffLoader;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IRequiredScheduleHelper> _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly Func<IScheduleTagSetter> _scheduleTagSetter;
		private readonly IScheduleRangePersister _persister;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly VoilatedSchedulePeriodBusinessRule _voilatedSchedulePeriodBusinessRule;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public ScheduleController(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling, FixedStaffLoader fixedStaffLoader, IDayOffTemplateRepository dayOffTemplateRepository, IActivityRepository activityRepository, Func<IFixedStaffSchedulingService> fixedStaffSchedulingService, Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder, Func<IRequiredScheduleHelper> requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder, Func<IScheduleTagSetter> scheduleTagSetter, Func<IPersonSkillProvider> personSkillProvider, IScheduleRangePersister persister, IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_activityRepository = activityRepository;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleTagSetter = scheduleTagSetter;
			_personSkillProvider = personSkillProvider;
			_persister = persister;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_voilatedSchedulePeriodBusinessRule = new VoilatedSchedulePeriodBusinessRule();
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff"), Authorize, UnitOfWork]
		public virtual IHttpActionResult FixedStaff([FromBody]FixedStaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));

			makeSurePrereqsAreLoaded();

			var people = _fixedStaffLoader.Load(period);

			_setupStateHolderForWebScheduling.Setup(period, people);

			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);

			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);
			_scheduleTagSetter().ChangeTagToSet(NullScheduleTag.Instance);
			
			var daysScheduled = 0;
			if (allSchedules.Any())
			{
				EventHandler<SchedulingServiceBaseEventArgs> schedulingServiceOnDayScheduled = (sender, args) => daysScheduled++;
				var fixedStaffSchedulingService = _fixedStaffSchedulingService();
				fixedStaffSchedulingService.DayScheduled += schedulingServiceOnDayScheduled;

				_scheduleCommand().Execute(new OptimizerOriginalPreferences(new SchedulingOptions
				{
					UseAvailability = true,
					UsePreferences = true,
					UseRotations = true,
					UseStudentAvailability = false,
					DayOffTemplate = _dayOffTemplateRepository.FindAllDayOffsSortByDescription()[0],
					ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
					GroupPageForShiftCategoryFairness = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
					TagToUseOnScheduling = NullScheduleTag.Instance
				}), new NoBackgroundWorker(), _schedulerStateHolder(), allSchedules, _groupPagePerDateHolder(),
					_requiredScheduleHelper(),
					new OptimizationPreferences());
				fixedStaffSchedulingService.DayScheduled -= schedulingServiceOnDayScheduled;
			}

			var conflicts = new List<PersistConflict>();
			foreach (var schedule in _schedulerStateHolder().Schedules)
			{
				conflicts.AddRange(_persister.Persist(schedule.Value));
			}

			var voilatedBusinessRules = getBusinessRulesValidationResults(people.SelectedPeople, period);
			return Ok(new SchedulingResultModel { DaysScheduled = daysScheduled, ConflictCount = conflicts.Count(), BusinessRulesValidationResults = voilatedBusinessRules });
		}

		private IEnumerable<BusinessRulesValidationResult> getBusinessRulesValidationResults(IEnumerable<IPerson> selectedPeople, DateOnlyPeriod period)
		{
			var result = new List<BusinessRulesValidationResult>();
			var schedulePeriodNotInRange = _voilatedSchedulePeriodBusinessRule.GetResult(selectedPeople, period).ToList();
			foreach (var person in selectedPeople)
			{
				int targetDaysOff;
				IList<IScheduleDay> dayOffsNow;
				var virtualSchedulePeriod = person.VirtualSchedulePeriod(period.StartDate);
				if(virtualSchedulePeriod== null) continue;
			   var hasCorrectNumberOfDayOff =_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(virtualSchedulePeriod, out targetDaysOff, out dayOffsNow);
				if (!hasCorrectNumberOfDayOff && isNotAInvalidScheduleRange(schedulePeriodNotInRange, person))
				{
						result.Add(new BusinessRulesValidationResult(){BusinessRuleCategory = BusinessRuleCategory.DayOff,Message = "Target days off are not achieved",Name = person.Name.FirstName});
				} 
			}
			result.AddRange(schedulePeriodNotInRange);
			return result;
		}

		private static bool isNotAInvalidScheduleRange(List<BusinessRulesValidationResult> schedulePeriodNotInRange, IPerson person)
		{
			return !schedulePeriodNotInRange.Contains(new BusinessRulesValidationResult()
			{
				BusinessRuleCategory = BusinessRuleCategory.SchedulePeriod,
				Name = person.Name.FirstName
			});
		}

		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder, PeopleSelection people,
			DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (people.SelectedPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}
			return allSchedules;
		}

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period, IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}

		private void makeSurePrereqsAreLoaded()
		{
			_activityRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
		}
	}
}