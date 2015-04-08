using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
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
		private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ScheduleController(IScenarioRepository scenarioRepository, ISkillDayLoadHelper skillDayLoadHelper, ISkillRepository skillRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IDayOffTemplateRepository dayOffTemplateRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, IPeopleAndSkillLoaderDecider decider, ICurrentTeleoptiPrincipal principal, IDisableDeletedFilter disableDeletedFilter, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillRepository = skillRepository;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_decider = decider;
			_principal = principal;
			_disableDeletedFilter = disableDeletedFilter;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff"), Authorize, UnitOfWork]
		public virtual IHttpActionResult FixedStaff([FromBody]FixedStaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var timeZone = _principal.Current().Regional.TimeZone;
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

			var stateHolder = new SchedulingResultStateHolder(allPeople,_scheduleRepository.FindSchedulesForPersons(new ScheduleDateTimePeriod(dateTimePeriod,selectedPeople,new SchedulerRangeToLoadCalculator(dateTimePeriod)), scenario,new PersonsInOrganizationProvider(allPeople), new ScheduleDictionaryLoadOptions(true,false,false), selectedPeople), forecast);
			var personSkillProvider = new PersonSkillProvider();
			var restrictionExtractor = new RestrictionExtractor(stateHolder);
			var effectiveRestrictionCreator = new EffectiveRestrictionCreator(restrictionExtractor);
			var resourceCalculationOnlyScheduleDayChangeCallback = new ResourceCalculationOnlyScheduleDayChangeCallback();
			var scheduleTagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);
			var schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(stateHolder, resourceCalculationOnlyScheduleDayChangeCallback,
				scheduleTagSetter);
			var workShiftCalculator = new WorkShiftCalculator();
			var dayOffsInPeriodCalculator = new DayOffsInPeriodCalculator(stateHolder);
			var hasContractDayOffDefinition = new HasContractDayOffDefinition();
			var resourceOptimizationHelper = new ResourceOptimizationHelper(stateHolder,new OccupiedSeatCalculator(), new NonBlendSkillCalculator(), personSkillProvider, new PeriodDistributionService(), _principal, new IntraIntervalFinderService(new SkillDayIntraIntervalFinder(new IntraIntervalFinder(), new SkillActivityCountCollector(new SkillActivityCounter()), new FullIntervalFinder())));
			var shiftCreatorService = new ShiftCreatorService(new CreateWorkShiftsFromTemplate());
			var workShiftWorkTime = new WorkShiftWorkTime(
				new RuleSetProjectionService(shiftCreatorService));
			var workShiftMinMaxCalculator = new WorkShiftMinMaxCalculator(
				new PossibleMinMaxWorkShiftLengthExtractor(restrictionExtractor,
					workShiftWorkTime),
				new SchedulePeriodTargetTimeCalculator(), new WorkShiftWeekMinMaxCalculator());
			var groupPagePerDateHolder = new GroupPagePerDateHolder();
			var ruleSetProjectionEntityService = new RuleSetProjectionEntityService(shiftCreatorService);
			var workShiftFromEditableShift = new WorkShiftFromEditableShift();
			var shiftProjectionCacheManager = new ShiftProjectionCacheManager(new ShiftFromMasterActivityService(),
				new RuleSetDeletedActivityChecker(), new RuleSetDeletedShiftCategoryChecker(),
				ruleSetProjectionEntityService,
				workShiftFromEditableShift);
			var schedulingService = new FixedStaffSchedulingService(stateHolder, dayOffsInPeriodCalculator,
				effectiveRestrictionCreator,
				new ScheduleService(
					new WorkShiftFinderService(stateHolder, new PreSchedulingStatusChecker(),
						new ShiftProjectionCacheFilter(new LongestPeriodForAssignmentCalculator(), new PersonalShiftAndMeetingFilter(), new NotOverWritableActivitiesShiftFilter(stateHolder)),
						new PersonSkillPeriodsDataHolderManager(stateHolder),
						shiftProjectionCacheManager,
						new WorkShiftCalculatorsManager(workShiftCalculator,
							new NonBlendWorkShiftCalculator(new NonBlendSkillImpactOnPeriodForProjection(), workShiftCalculator,
								personSkillProvider)),
						workShiftMinMaxCalculator,
						new FairnessAndMaxSeatCalculatorsManager(stateHolder,
							new ShiftCategoryFairnessManager(stateHolder,
								new GroupShiftCategoryFairnessCreator(groupPagePerDateHolder, stateHolder),
								new ShiftCategoryFairnessCalculator()), new ShiftCategoryFairnessShiftValueCalculator(),
							new FairnessValueCalculator(), new SeatLimitationWorkShiftCalculator2(new SeatImpactOnPeriodForProjection())),
						new ShiftLengthDecider(new DesiredShiftLengthCalculator(new SchedulePeriodTargetTimeCalculator()))),
					new ScheduleMatrixListCreator(stateHolder), new ShiftCategoryLimitationChecker(stateHolder),
					effectiveRestrictionCreator),
				new DaysOffSchedulingService(
					new AbsencePreferenceScheduler(effectiveRestrictionCreator,
						schedulePartModifyAndRollbackService, new AbsencePreferenceFullDayLayerCreator()),
					new DayOffScheduler(dayOffsInPeriodCalculator,
						effectiveRestrictionCreator,
						schedulePartModifyAndRollbackService, new ScheduleDayAvailableForDayOffSpecification(),
						hasContractDayOffDefinition),
					new MissingDaysOffScheduler(new BestSpotForAddingDayOffFinder(), new MatrixDataListInSteadyState(),
						new MatrixDataListCreator(
							new ScheduleDayDataMapper(effectiveRestrictionCreator,
								hasContractDayOffDefinition)), new MatrixDataWithToFewDaysOff(dayOffsInPeriodCalculator))),
				resourceOptimizationHelper);

			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (selectedPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}

			var schedulerStateHolder = new SchedulerStateHolder(scenario,new DateOnlyPeriodAsDateTimePeriod(period,timeZone),allPeople,_disableDeletedFilter,stateHolder);
			schedulerStateHolder.LoadCommonState(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CurrentUnitOfWork(), new RepositoryFactory());
			stateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(selectedPeople);

			var groupPageDataProvider = new GroupScheduleGroupPageDataProvider(schedulerStateHolder,new RepositoryFactory(), _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory(), _disableDeletedFilter);
			var scheduleDayEquator = new ScheduleDayEquator(new EditableShiftMapper());
			var groupPersonSkillAggregator = new GroupPersonSkillAggregator();
			var workShiftFilterService = new WorkShiftFilterService(new ActivityRestrictionsShiftFilter(),
				new BusinessRulesShiftFilter(stateHolder, new ValidDateTimePeriodShiftFilter(),
					new LongestPeriodForAssignmentCalculator()),
				new CommonMainShiftFilter(scheduleDayEquator),
				new ContractTimeShiftFilter(
					workShiftMinMaxCalculator),
				new DisallowedShiftCategoriesShiftFilter(), new EffectiveRestrictionShiftFilter(),
				new MainShiftOptimizeActivitiesSpecificationShiftFilter(), new NotOverWritableActivitiesShiftFilter(stateHolder),
				new PersonalShiftsShiftFilter(stateHolder, new PersonalShiftMeetingTimeChecker()),
				new ShiftCategoryRestrictionShiftFilter(),
				new TimeLimitsRestrictionShiftFilter(new ValidDateTimePeriodShiftFilter(),
					new LatestStartTimeLimitationShiftFilter(), new EarliestEndTimeLimitationShiftFilter()),
				new WorkTimeLimitationShiftFilter(),
				new ShiftLengthDecider(new DesiredShiftLengthCalculator(new SchedulePeriodTargetTimeCalculator())),
				workShiftMinMaxCalculator, new CommonActivityFilter(),
				new RuleSetAccordingToAccessabilityFilter(new TeamBlockRuleSetBagExtractor(),
					new TeamBlockIncludedWorkShiftRuleFilter(), new RuleSetSkillActivityChecker(), groupPersonSkillAggregator),
				shiftProjectionCacheManager, new RuleSetPersonalSkillsActivityFilter(new RuleSetSkillActivityChecker()),
				new DisallowedShiftProjectionCashesFilter());
			var deleteSchedulePartService = new DeleteSchedulePartService(stateHolder);
			var skillResolutionProvider = new SkillResolutionProvider();
			var matrixListFactory = new MatrixListFactory(schedulerStateHolder,
				new MatrixUserLockLocker(new GridlockManager()), new MatrixNotPermittedLocker());
			var scheduleDayDataMapper = new ScheduleDayDataMapper(effectiveRestrictionCreator, new HasContractDayOffDefinition());
			var teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			var maxSeatSkillAggregator = new MaxSeatSkillAggregator();
			var skillIntervalDataAggregator = new SkillIntervalDataAggregator();
			var createSkillIntervalDataPerDateAndActivity = new CreateSkillIntervalDataPerDateAndActivity(groupPersonSkillAggregator,
				new CreateSkillIntervalDatasPerActivtyForDate(
					new CalculateAggregatedDataForActivtyAndDate(new SkillStaffPeriodToSkillIntervalDataMapper(),
						new SkillIntervalDataSkillFactorApplier(), skillIntervalDataAggregator, new SkillIntervalDataDivider()),
					skillResolutionProvider), maxSeatSkillAggregator, new SkillIntervalDataDivider());
			var teamBlockSteadyStateValidator = new TeamBlockSteadyStateValidator(teamBlockSchedulingOptions, new SameStartTimeBlockSpecification(),
				new SameStartTimeTeamSpecification(), new SameEndTimeTeamSpecification(),
				new SameShiftCategoryBlockSpecification(), new SameShiftCategoryTeamSpecification(),
				new SameShiftBlockSpecification(new ValidSampleDayPickerFromTeamBlock(),
					scheduleDayEquator),
				new TeamBlockOpenHoursValidator(
					createSkillIntervalDataPerDateAndActivity,
					new SkillIntervalDataOpenHour()), stateHolder);
			var equalWorkShiftValueDecider = new EqualWorkShiftValueDecider(new TrueFalseRandomizer());
			var workShiftSelector = new WorkShiftSelector(
				new WorkShiftValueCalculator(new WorkShiftPeriodValueCalculator(), new WorkShiftLengthValueCalculator(),
					new MaxSeatsCalculationForTeamBlock()), equalWorkShiftValueDecider);
			var medianCalculatorForDays = new MedianCalculatorForDays(new MedianCalculatorForSkillInterval(new IntervalDataMedianCalculator()));
			var activityIntervalDataCreator = new ActivityIntervalDataCreator(
				createSkillIntervalDataPerDateAndActivity,
				new DayIntervalDataCalculator(
					medianCalculatorForDays,
					new TwoDaysIntervalGenerator()));
			var nightlyRestRule = new NightlyRestRule();
			var teamBlockClearer = new TeamBlockClearer(new DeleteAndResourceCalculateService(deleteSchedulePartService,
				resourceOptimizationHelper));
			var teamScheduling = new TeamScheduling();
			var teamBlockRestrictionAggregator = new TeamBlockRestrictionAggregator(effectiveRestrictionCreator, stateHolder, scheduleDayEquator, nightlyRestRule, teamBlockSchedulingOptions);
			var maxSeatsSpecificationDictionaryExtractor = new MaxSeatsSpecificationDictionaryExtractor(new IsMaxSeatsReachedOnSkillStaffPeriodSpecification(),
				new MaxSeatBoostingFactorCalculator());
			var teamBlockScheduler = new TeamBlockScheduler(
				new TeamBlockSingleDayScheduler(new TeamBlockSchedulingCompletionChecker(),
					new ProposedRestrictionAggregator(
						new TeamRestrictionAggregator(effectiveRestrictionCreator, stateHolder, teamBlockSchedulingOptions),
						new BlockRestrictionAggregator(effectiveRestrictionCreator, stateHolder,
							scheduleDayEquator, nightlyRestRule, teamBlockSchedulingOptions),
						teamBlockRestrictionAggregator,
						teamBlockSchedulingOptions), workShiftFilterService,
					workShiftSelector,
					teamScheduling,
					activityIntervalDataCreator,
					new MaxSeatInformationGeneratorBasedOnIntervals(maxSeatSkillAggregator,
						maxSeatsSpecificationDictionaryExtractor), maxSeatSkillAggregator),
				new TeamBlockRoleModelSelector(
					teamBlockRestrictionAggregator,
					workShiftFilterService,
					new SameOpenHoursInTeamBlockSpecification(new OpenHourForDate(new SkillIntervalDataOpenHour()),
						createSkillIntervalDataPerDateAndActivity, stateHolder),
					workShiftSelector,
					stateHolder,
					activityIntervalDataCreator,
					new MaxSeatInformationGeneratorBasedOnIntervals(maxSeatSkillAggregator,
						maxSeatsSpecificationDictionaryExtractor), maxSeatSkillAggregator,
					new FirstShiftInTeamBlockFinder(shiftProjectionCacheManager)),
				teamBlockClearer, teamBlockSchedulingOptions, false);
			var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
			var contractWeeklyRestForPersonWeek = new ContractWeeklyRestForPersonWeek();
			var groupPageCreator = new GroupPageCreator(new GroupPageFactory());
			var groupPersonBuilderForOptimizationFactory = new GroupPersonBuilderForOptimizationFactory(groupPageDataProvider, groupPagePerDateHolder,
				schedulerStateHolder, groupPageCreator);
			var teamBlockInfoFactory = new TeamBlockInfoFactory(new DynamicBlockFinder(), new TeamMemberTerminationOnBlockSpecification());
			var dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
			var weeklyRestSolverCommand = new WeeklyRestSolverCommand(
				teamBlockInfoFactory,
				teamBlockSchedulingOptions,
				new WeeklyRestSolverService(new WeeksFromScheduleDaysExtractor(),
					new EnsureWeeklyRestRule(workTimeStartEndExtractor,
						dayOffMaxFlexCalculator), contractWeeklyRestForPersonWeek,
					new DayOffToTimeSpanExtractor(new ExtractDayOffFromGivenWeek(),
						new ScheduleDayWorkShiftTimeExtractor(workTimeStartEndExtractor),
						new VerifyWeeklyRestAroundDayOffSpecification()),
					new ShiftNudgeManager(
						new ShiftNudgeEarlier(
							teamBlockClearer,
							teamBlockRestrictionAggregator,
							teamBlockScheduler),
						new ShiftNudgeLater(teamBlockClearer,
							teamBlockRestrictionAggregator,
							teamBlockScheduler),
						new EnsureWeeklyRestRule(workTimeStartEndExtractor,
							dayOffMaxFlexCalculator), contractWeeklyRestForPersonWeek,
						new TeamBlockScheduleCloner(), new FilterForTeamBlockInSelection(),
						new TeamBlockOptimizationLimits(
							new TeamBlockRestrictionOverLimitValidator(new RestrictionOverLimitDecider(new RestrictionChecker()),
								new MaxMovedDaysOverLimitValidator(scheduleDayEquator)),
							new MinWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor())), new SchedulingOptionsCreator(),
						teamBlockSteadyStateValidator, new ScheduleDayIsLockedSpecification()),
					new IdentifyDayOffWithHighestSpan(),
					new DeleteScheduleDayFromUnsolvedPersonWeek(deleteSchedulePartService,
						new ScheduleDayIsLockedSpecification()), new AllTeamMembersInSelectionSpecification(),
					new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(),
						new VerifyWeeklyRestAroundDayOffSpecification(),
						new EnsureWeeklyRestRule(workTimeStartEndExtractor,
							dayOffMaxFlexCalculator)),
					new BrokenWeekCounterForAPerson(new WeeksFromScheduleDaysExtractor(),
						new EnsureWeeklyRestRule(workTimeStartEndExtractor,
							dayOffMaxFlexCalculator), contractWeeklyRestForPersonWeek)),
				schedulerStateHolder,
				groupPersonBuilderForOptimizationFactory,
				new ScheduleCommandToggle(new TrueToggleManager()));
			var innerOptimizerHelperHelper = new InnerOptimizerHelperHelper();
			var teamMatrixChecker = new TeamMatrixChecker();
			var matrixDataListCreator = new MatrixDataListCreator(scheduleDayDataMapper);
			var command = new ScheduleCommand(personSkillProvider, groupPageCreator,
				groupPageDataProvider, resourceOptimizationHelper, resourceCalculationOnlyScheduleDayChangeCallback,
				new TeamBlockScheduleCommand(schedulingService, schedulerStateHolder,
					resourceCalculationOnlyScheduleDayChangeCallback,
					groupPersonBuilderForOptimizationFactory,
					new AdvanceDaysOffSchedulingService(
						new AbsencePreferenceScheduler(effectiveRestrictionCreator, schedulePartModifyAndRollbackService,
							new AbsencePreferenceFullDayLayerCreator()),
						new TeamDayOffScheduler(new DayOffsInPeriodCalculator(stateHolder), effectiveRestrictionCreator,
							new HasContractDayOffDefinition(),
							matrixDataListCreator, stateHolder, new ScheduleDayAvailableForDayOffSpecification()),
						new TeamBlockMissingDayOffHandler(new BestSpotForAddingDayOffFinder(),
							matrixDataListCreator, new MatrixDataWithToFewDaysOff(new DayOffsInPeriodCalculator(stateHolder)),
							new SplitSchedulePeriodToWeekPeriod(), new ValidNumberOfDayOffInAWeekSpecification())),
					matrixListFactory,
					teamBlockInfoFactory,
					new SafeRollbackAndResourceCalculation(resourceOptimizationHelper),
					workShiftMinMaxCalculator,
					teamBlockSteadyStateValidator, new TeamBlockMaxSeatChecker(stateHolder),
					teamBlockSchedulingOptions, new TeamBlockSchedulingCompletionChecker(),teamBlockScheduler,weeklyRestSolverCommand, teamMatrixChecker,innerOptimizerHelperHelper),
				new ClassicScheduleCommand(matrixListFactory, weeklyRestSolverCommand,
					resourceCalculationOnlyScheduleDayChangeCallback, resourceOptimizationHelper, innerOptimizerHelperHelper),
				matrixListFactory,
				new IntraIntervalFinderService(new SkillDayIntraIntervalFinder(new IntraIntervalFinder(),
					new SkillActivityCountCollector(new SkillActivityCounter()), new FullIntervalFinder())), innerOptimizerHelperHelper);

			var daysScheduled = 0;
			EventHandler<SchedulingServiceBaseEventArgs> schedulingServiceOnDayScheduled = (sender, args) => daysScheduled++;
			schedulingService.DayScheduled += schedulingServiceOnDayScheduled;

			var scheduleService =
				new ScheduleService(
					new WorkShiftFinderService(stateHolder, new PreSchedulingStatusChecker(),
						new ShiftProjectionCacheFilter(new LongestPeriodForAssignmentCalculator(), new PersonalShiftAndMeetingFilter(),
							new NotOverWritableActivitiesShiftFilter(stateHolder)), new PersonSkillPeriodsDataHolderManager(stateHolder),
						shiftProjectionCacheManager,
						new WorkShiftCalculatorsManager(new WorkShiftCalculator(),
							new NonBlendWorkShiftCalculator(new NonBlendSkillImpactOnPeriodForProjection(), new WorkShiftCalculator(),
								personSkillProvider)), workShiftMinMaxCalculator,
						new FairnessAndMaxSeatCalculatorsManager(stateHolder,
							new ShiftCategoryFairnessManager(stateHolder,
								new GroupShiftCategoryFairnessCreator(groupPagePerDateHolder, stateHolder),
								new ShiftCategoryFairnessCalculator()), new ShiftCategoryFairnessShiftValueCalculator(),
							new FairnessValueCalculator(), new SeatLimitationWorkShiftCalculator2(new SeatImpactOnPeriodForProjection())),
						new ShiftLengthDecider(new DesiredShiftLengthCalculator(new SchedulePeriodTargetTimeCalculator()))),
					new ScheduleMatrixListCreator(stateHolder), new ShiftCategoryLimitationChecker(stateHolder),
					effectiveRestrictionCreator);
			command.Execute(new OptimizerOriginalPreferences(new SchedulingOptions
			{
				UseAvailability = true,
				UsePreferences = true,
				UseRotations = true,
				UseStudentAvailability = true,
				DayOffTemplate = _dayOffTemplateRepository.FindAllDayOffsSortByDescription()[0],
				ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
				GroupPageForShiftCategoryFairness = new GroupPageLight {Key = "Main", Name = UserTexts.Resources.Main},
				TagToUseOnScheduling = NullScheduleTag.Instance
			}), new FakeBackgroundWorker(), schedulerStateHolder, allSchedules, groupPagePerDateHolder,
				new RequiredScheduleHelper(
					new SchedulePeriodListShiftCategoryBackToLegalStateService(stateHolder,
						new ScheduleMatrixValueCalculatorProFactory(), new ScheduleFairnessCalculator(stateHolder),
						new ScheduleDayService(scheduleService, deleteSchedulePartService, resourceOptimizationHelper,
							effectiveRestrictionCreator, schedulePartModifyAndRollbackService),
						resourceCalculationOnlyScheduleDayChangeCallback),
					new RuleSetBagsOfGroupOfPeopleCanHaveShortBreak(new RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader()),
					stateHolder, schedulingService, null, new OptimizationPreferences(), scheduleService, resourceOptimizationHelper,
					new GridlockManager(),
					new DaysOffSchedulingService(
						new AbsencePreferenceScheduler(effectiveRestrictionCreator, schedulePartModifyAndRollbackService,
							new AbsencePreferenceFullDayLayerCreator()),
						new DayOffScheduler(new DayOffsInPeriodCalculator(stateHolder), effectiveRestrictionCreator,
							schedulePartModifyAndRollbackService, new ScheduleDayAvailableForDayOffSpecification(),
							hasContractDayOffDefinition),
						new MissingDaysOffScheduler(new BestSpotForAddingDayOffFinder(), new MatrixDataListInSteadyState(),
							new MatrixDataListCreator(scheduleDayDataMapper),
							new MatrixDataWithToFewDaysOff(new DayOffsInPeriodCalculator(stateHolder)))), new WorkShiftFinderResultHolder(),
					resourceCalculationOnlyScheduleDayChangeCallback, scheduleDayEquator, matrixListFactory),
				new OptimizationPreferences());
			schedulingService.DayScheduled -= schedulingServiceOnDayScheduled;

			return Ok(new SchedulingResultModel{DaysScheduled = daysScheduled});
		}
	}

	class FakeBackgroundWorker : IBackgroundWorkerWrapper
	{
		public bool CancellationPending { get; private set; }
		public void ReportProgress(int percentProgress, object userState = null)
		{
		}
	}

	public class FixedStaffSchedulingInput
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}