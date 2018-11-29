using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
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
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using IWorkShiftCalculator = Teleopti.Ccc.Domain.Scheduling.IWorkShiftCalculator;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class SchedulingCommonModule : Module
	{
		private readonly IocConfiguration _configuration;

		public SchedulingCommonModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<HandleOneAnalyticsAvailabilityDay>().SingleInstance().ApplyAspects();
			builder.RegisterType<HandleMultipleAnalyticsAvailabilityDays>().SingleInstance();

			builder.RegisterType<ScheduleStorage>()
				.As<IScheduleStorage>()
				.As<IFindSchedulesForPersons>()
				.AsSelf()
				.SingleInstance();
			builder.RegisterType<ReplaceLayerInSchedule>().As<IReplaceLayerInSchedule>().SingleInstance();
			builder.RegisterType<AddReducedSkillDaysToStateHolder>().SingleInstance();
			builder.RegisterType<AddBpoResourcesToContext>().SingleInstance();
			builder.RegisterType<SkillCombinationToBpoResourceMapper>().SingleInstance();
			builder.RegisterType<ExternalStaffProvider>().SingleInstance();
			builder.RegisterType<MaxSeatPeak>().SingleInstance();
			builder.RegisterType<IsOverMaxSeat>().SingleInstance();
			builder.RegisterType<LockDaysOnTeamBlockInfos>().SingleInstance();
			builder.RegisterType<SkillRoutingPriorityModel>().SingleInstance();
			builder.RegisterType<SkillRoutingPriorityPersister>().SingleInstance();

			builder.RegisterType<ShovelResources>().SingleInstance();
			builder.RegisterType<ReducePrimarySkillResources>().SingleInstance();
			builder.RegisterType<SkillSetPerActivityProvider>().SingleInstance();
			builder.RegisterType<PrimarySkillOverstaff>().SingleInstance();
			builder.RegisterType<AddResourcesToSubSkills>().SingleInstance();
			builder.RegisterType<SkillCombinationResourceRepository>().As<ISkillCombinationResourceReader>().SingleInstance();

			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			builder.RegisterType<CascadingResourceCalculation>().As<IResourceCalculation>().SingleInstance();
			builder.RegisterType<WeeklyRestSolverCommand>().ApplyAspects();
			builder.RegisterType<ResourceOptimizationHelper>().SingleInstance();
			builder.RegisterType<OptimizeIntradayDesktop>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationDesktopExecuter>().InstancePerLifetimeScope();
			builder.RegisterType<DesktopScheduling>().InstancePerLifetimeScope();
			builder.RegisterType<BackToLegalShiftService>();
			builder.RegisterType<ScheduleOvertime>();
			builder.RegisterType<TeamBlockDesktopOptimization>();
			builder.RegisterType<ScheduleOptimizerHelper>().InstancePerLifetimeScope();
			builder.RegisterType<CascadingResourceCalculationContextFactory>().SingleInstance();
			builder.RegisterType<CascadingPersonSkillProvider>().SingleInstance();
			builder.RegisterType<PersonalSkillsProvider>().SingleInstance();
			builder.RegisterType<PersonalSkills>().SingleInstance();
			builder.RegisterType<SkillsOnAgentsProvider>().SingleInstance();
			builder.RegisterType<ReducedSkillsProvider>().SingleInstance();
			builder.RegisterType<DaysOffInPeriodValidatorForBlock>().As<IDaysOffInPeriodValidatorForBlock>().SingleInstance();
			builder.RegisterType<SchedulerStateScheduleDayChangedCallback>().As<IScheduleDayChangeCallback>().InstancePerLifetimeScope();
			builder.RegisterModule<IntraIntervalOptimizationServiceModule>();
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
			builder.RegisterModule<BackToLegalShiftModule>();
			builder.RegisterModule(new ScheduleOvertimeModule(_configuration));
			builder.RegisterType<RemoveShiftCategoryToBeInLegalState>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleHourlyStaffExecutor>().InstancePerLifetimeScope();
		
			builder.RegisterType<ScheduleExecutor>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<Scheduling>().InstancePerLifetimeScope();
			builder.RegisterType<RuleSetAccordingToAccessabilityFilter>().SingleInstance();
			builder.RegisterType<WorkShiftFilterService>().InstancePerLifetimeScope();				
			builder.RegisterType<AdvanceDaysOffSchedulingService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamDayOffScheduling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockMissingDayOffHandling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduleSelected>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
			builder.RegisterType<SchedulingCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<FullScheduling>().InstancePerLifetimeScope().ApplyAspects();

			builder.RegisterType<ExtendSelectedPeriodForMonthlyScheduling>().SingleInstance();
			builder.RegisterType<CorrectAlteredBetween>().SingleInstance();
			
			builder.RegisterType<PersonListExtractorFromScheduleParts>().As<IPersonListExtractorFromScheduleParts>().SingleInstance();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>().InstancePerLifetimeScope();
			builder.RegisterType<FlexibelDayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>().SingleInstance();
			builder.RegisterType<IntradayOptimizationExecutor>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<IntradayOptimizationOnStardust>().SingleInstance().ApplyAspects();

			
			//change to scope? 
			builder.RegisterType<TeamBlockMoveTimeBetweenDaysCommand>().As<ITeamBlockMoveTimeBetweenDaysCommand>();
			builder.RegisterType<MatrixListFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockIntradayOptimizationService>();				
			builder.RegisterType<TeamBlockDaysOffSameDaysOffLockSyncronizer>().SingleInstance();
			builder.RegisterType<BackToLegalShiftCommand>();
			builder.RegisterType<IntraIntervalOptimizationCommand>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPersonBuilderWrapper>().As<IGroupPersonBuilderWrapper>().InstancePerLifetimeScope();
			builder.RegisterType<TeamInfoFactoryFactory>().InstancePerLifetimeScope();
			builder.RegisterType<BlockPreferencesMapper>().SingleInstance();

			builder.RegisterType<DisableDeletedFilter>().As<IDisableDeletedFilter>().SingleInstance();
			builder.RegisterType<InnerOptimizerHelperHelper>().As<IOptimizerHelperHelper>().SingleInstance();
			builder.RegisterType<PeriodValueCalculatorProvider>().As<IPeriodValueCalculatorProvider>().SingleInstance();
			builder.RegisterType<ResourceOptimizationHelperExtended>().As<IResourceOptimizationHelperExtended>().InstancePerLifetimeScope();
			builder.RegisterType<RequiredScheduleHelper>().InstancePerLifetimeScope();
			builder.RegisterType<CommonStateHolder>().As<ICommonStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodExtractorFromScheduleParts>().SingleInstance();

			builder.RegisterModule<WeeklyRestSolverModule>();
			builder.RegisterModule<EqualNumberOfCategoryFairnessModule>();

			builder.RegisterType<OptimizerActivitiesPreferencesFactory>().SingleInstance();

			builder.RegisterType<SchedulerStateHolder>()
				.As<ISchedulerStateHolder>()
				.As<IClearReferredShiftTradeRequests>()
				.AsSelf()
				.InstancePerLifetimeScope();
			builder.RegisterType<TimeZoneGuard>().As<ITimeZoneGuard>().SingleInstance();
			builder.RegisterType<OverriddenBusinessRulesHolder>().As<IOverriddenBusinessRulesHolder>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<BudgetGroupState>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingResultStateHolderProvider>().As<ISchedulingResultStateHolderProvider>().SingleInstance();
			builder.RegisterType<ScheduleDayForPerson>().As<IScheduleDayForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRangeForPerson>().As<IScheduleRangeForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<PreSchedulingStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<NightRestWhiteSpotSolverServiceFactory>().As<INightRestWhiteSpotSolverServiceFactory>().InstancePerLifetimeScope();
			builder.RegisterType<NumberOfAgentsKnowingSkill>().SingleInstance();
			builder.RegisterType<ReduceSkillSets>().SingleInstance();
			builder.RegisterType<CreateIslands>().SingleInstance();
			builder.RegisterType<MergeIslandsSizeLimit>().SingleInstance();		
			builder.RegisterType<MergeIslands>().SingleInstance();
			builder.RegisterType<MoveSkillSetToCorrectIsland>().SingleInstance();
			builder.RegisterType<IslandModelFactory>().SingleInstance();
			builder.RegisterType<CreateSkillSets>().SingleInstance();
			builder.RegisterType<ReduceIslandsLimits>().SingleInstance();
			builder.RegisterType<LongestPeriodForAssignmentCalculator>().As<ILongestPeriodForAssignmentCalculator>().SingleInstance();
			builder.RegisterType<ShiftProjectionCacheFetcher>().SingleInstance();
			builder.RegisterType<ShiftsFromMasterActivityBaseActivityService>().As<IShiftFromMasterActivityService>().SingleInstance();			
			builder.RegisterType<RuleSetDeletedActivityChecker>().As<IRuleSetDeletedActivityChecker>().SingleInstance();
			builder.RegisterType<RuleSetDeletedShiftCategoryChecker>()
				.As<IRuleSetDeletedShiftCategoryChecker>()
				.SingleInstance();
			builder.RegisterType<WorkShiftCalculatorsManager>().SingleInstance();
			builder.RegisterType<FairnessAndMaxSeatCalculatorsManager28317>().SingleInstance();
			builder.RegisterType<ScheduleChangesAffectedDates>().SingleInstance();
			builder.RegisterType<AffectedDates>().SingleInstance();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().SingleInstance();
			builder.RegisterGeneric(typeof(PairMatrixService<>)).As(typeof(IPairMatrixService<>)).SingleInstance();
			builder.RegisterGeneric(typeof(PairDictionaryFactory<>)).As(typeof(IPairDictionaryFactory<>)).SingleInstance();
			builder.RegisterType<OptimizationPreferences>().As<IOptimizationPreferences>().InstancePerLifetimeScope();
			builder.RegisterType<DaysOffPreferences>().As<IDaysOffPreferences>().InstancePerLifetimeScope();
			builder.RegisterType<NonBlendWorkShiftCalculator>().As<INonBlendWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>()
				.As<INonBlendSkillImpactOnPeriodForProjection>()
				.SingleInstance();
			builder.RegisterType<GroupPagePerDateHolder>().As<IGroupPagePerDateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<DesiredShiftLengthCalculator>().As<IDesiredShiftLengthCalculator>().SingleInstance();
			builder.RegisterType<ShiftLengthDecider>().As<IShiftLengthDecider>().SingleInstance();
			builder.RegisterType<PersonSkillDayCreator>().SingleInstance();

			builder.RegisterType<ShiftProjectionCacheManager>().InstancePerLifetimeScope();
			builder.RegisterType<TeamScheduling>().SingleInstance();
			builder.RegisterType<WorkShiftFinderService>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftProjectionCacheFactory>().SingleInstance();
			builder.RegisterType<WorkShiftSelector>().As<IWorkShiftSelector>().As<IWorkShiftSelectorForIntraInterval>().SingleInstance();	
			builder.RegisterType<TeamBlockRoleModelSelector>().InstancePerLifetimeScope();			
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().SingleInstance();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().SingleInstance();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().SingleInstance();
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().SingleInstance();
			builder.RegisterType<ShiftCategoryLimitationChecker>().As<IShiftCategoryLimitationChecker>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulePartModifyAndRollbackService>().As<ISchedulePartModifyAndRollbackService>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulePartModifyAndRollbackServiceWithoutStateHolder>();
			builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceFullDayLayerCreator>().As<IAbsencePreferenceFullDayLayerCreator>().SingleInstance();
			builder.RegisterType<ScheduleDayAvailableForDayOffSpecification>().As<IScheduleDayAvailableForDayOffSpecification>().SingleInstance();
			builder.RegisterType<ScheduleOvertimeOnNonScheduleDays>().InstancePerLifetimeScope();

			builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().SingleInstance();
			builder.Register(c => new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().InstancePerLifetimeScope();

			builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftProjectionCacheFilter>().SingleInstance();

			builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().SingleInstance();
			builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().SingleInstance();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().SingleInstance();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().SingleInstance();
			builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().SingleInstance();
			builder.RegisterType<DeleteAndResourceCalculateService>().InstancePerLifetimeScope();
			builder.RegisterType<IntervalDataMedianCalculator>().As<IIntervalDataCalculator>().SingleInstance();
			builder.RegisterType<ScheduleDayEquator>().As<IScheduleDayEquator>().SingleInstance();

			builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().SingleInstance();
			builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerDependency();
			builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().SingleInstance();

			builder.RegisterType<SchedulingStateHolderAllSkillExtractor>().InstancePerDependency();
			builder.RegisterType<DailySkillForecastAndScheduledValueCalculator>().As<IDailySkillForecastAndScheduledValueCalculator>().InstancePerDependency();

			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().SingleInstance();
			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().SingleInstance();
			builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixUserLockLocker>().InstancePerLifetimeScope();

			if (_configuration.Toggle(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348))
			{
				builder.RegisterType<MatrixClosedDayLocker>().As<IMatrixClosedDayLocker>().InstancePerLifetimeScope();
			}
			else
			{
				builder.RegisterType<MatrixClosedDaysLockerDoNothing>().As<IMatrixClosedDayLocker>().InstancePerLifetimeScope();
			}

			if (_configuration.Toggle(Toggles.ResourcePlanner_DoNotRemoveShiftsDayOffOptimization_77941))
			{
				builder.RegisterType<ScheduleAllRemovedDaysOrRollback>().As<IScheduleAllRemovedDaysOrRollback>().InstancePerLifetimeScope();
			}
			else
			{
				builder.RegisterType<ScheduleAllRemovedDaysOrRollbackDoNothing>().As<IScheduleAllRemovedDaysOrRollback>().InstancePerLifetimeScope();
			}

			if (_configuration.Toggle(Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118))
			{
				builder.RegisterType<OpenHoursSkillExtractor>().As<IOpenHoursSkillExtractor>().SingleInstance();
			}
			else
			{
				builder.RegisterType<OpenHoursSkillExtractorDoNothing>().As<IOpenHoursSkillExtractor>().SingleInstance();
			}

			builder.RegisterType<MatrixNotPermittedLocker>().SingleInstance();
			builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().SingleInstance();
			builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().SingleInstance();

			builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().SingleInstance();
			builder.RegisterType<DayOffOptimizerValidator>().InstancePerLifetimeScope();
			builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();

			builder.RegisterType<SchedulerGroupPagesProvider>().InstancePerLifetimeScope();

			builder.RegisterType<GroupScheduleGroupPageDataProvider>().AsSelf().As<IGroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageCreator>().As<IGroupPageCreator>().SingleInstance();
			builder.RegisterType<GroupPageFactory>().As<IGroupPageFactory>().SingleInstance();
			builder.RegisterType<GroupCreator>().As<IGroupCreator>().SingleInstance();
			builder.RegisterType<SwapServiceNew>().As<ISwapServiceNew>().SingleInstance();
			builder.RegisterType<GroupPersonBuilderForOptimization>().As<IGroupPersonBuilderForOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();
			builder.RegisterType<EditableShiftMapper>().As<IEditableShiftMapper>().SingleInstance();
			builder.RegisterType<MaxMovedDaysOverLimitValidator>().As<IMaxMovedDaysOverLimitValidator>().SingleInstance();
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().SingleInstance();
			builder.RegisterType<TeamBlockOptimizationLimits>().As<ITeamBlockOptimizationLimits>().SingleInstance();
			builder.RegisterType<RestrictionOverLimitValidator>().SingleInstance();
			builder.RegisterType<DayOffOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffOptimizationCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<TeamBlockDayOffOptimizer>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<DayOffOptimizerStandard>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffOptimizerPreMoveResultPredictor>().SingleInstance();

			builder.RegisterType<AffectedDayOffs>().SingleInstance();

			builder.RegisterType<TeamBlockRemoveShiftCategoryOnBestDateService>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockRetryRemoveShiftCategoryBackToLegalService>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<RemoveScheduleDayProsBasedOnShiftCategoryLimitation>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftCategoryWeekRemover>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryPeriodRemover>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryLimitCounter>().SingleInstance();

			builder.RegisterType<TeamBlockDayOffsInPeriodValidator>().SingleInstance();

			//ITeamBlockRestrictionOverLimitValidator
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().SingleInstance();
			builder.RegisterType<HasContractDayOffDefinition>().As<IHasContractDayOffDefinition>().SingleInstance();
			builder.RegisterType<ScheduleDayDataMapper>().As<IScheduleDayDataMapper>().SingleInstance();
			builder.RegisterType<MatrixDataListCreator>().SingleInstance();
			builder.RegisterType<MatrixDataWithToFewDaysOff>().As<IMatrixDataWithToFewDaysOff>().InstancePerLifetimeScope();

			builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().SingleInstance();
			builder.RegisterType<CreatePersonalSkillDataExtractor>().SingleInstance();
			builder.RegisterType<ForecastAndScheduleSumForDay>().SingleInstance();
	
			builder.RegisterType<TrueFalseRandomizer>().As<ITrueFalseRandomizer>().SingleInstance();
			builder.RegisterType<OfficialWeekendDays>().As<IOfficialWeekendDays>().SingleInstance();
			builder.RegisterType<CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker>().As<IDayOffDecisionMaker>().SingleInstance();
			builder.RegisterType<SchedulingOptionsCreator>().As<ISchedulingOptionsCreator>().SingleInstance();
			builder.RegisterType<LockableBitArrayChangesTracker>().As<ILockableBitArrayChangesTracker>().SingleInstance();
			builder.RegisterType<SmartDayOffBackToLegalStateService>().As<ISmartDayOffBackToLegalStateService>().SingleInstance();
			builder.RegisterType<TeamBlockDaysOffMoveFinder>().As<ITeamBlockDaysOffMoveFinder>().SingleInstance();
			builder.RegisterType<DaysOffLegalStateValidatorsFactory>().As<IDaysOffLegalStateValidatorsFactory>().SingleInstance();
			//IDaysOffLegalStateValidatorsFactory

			registerWorkShiftFilters(builder);
			registerWorkShiftSelector(builder);
			registerTeamBlockCommon(builder);
			registerTeamBlockDayOffOptimizerService(builder);
			builder.RegisterType<TeamBlockIntradayDecisionMaker>().InstancePerLifetimeScope();
			registerTeamBlockSchedulingService(builder);
			registerMaxSeatSkillCreator(builder);

			builder.RegisterType<AgentRestrictionsNoWorkShiftfFinder>().As<IAgentRestrictionsNoWorkShiftfFinder>().InstancePerLifetimeScope();
			registerFairnessOptimizationService(builder);
			registerDayOffFairnessOptimizationService(builder);
			registerMoveTimeOptimizationClasses(builder);

			builder.RegisterType<AnalyzePersonAccordingToAvailability>().SingleInstance();
			builder.RegisterType<AdjustOvertimeLengthBasedOnAvailability>().SingleInstance();
			builder.RegisterType<OvertimeSkillIntervalDataAggregator>().As<IOvertimeSkillIntervalDataAggregator>().SingleInstance();
			builder.RegisterType<OvertimePeriodValueMapper>().SingleInstance();
			builder.RegisterType<MergeOvertimeSkillIntervalData>().As<IMergeOvertimeSkillIntervalData>().SingleInstance();

			builder.RegisterType<OvertimeLengthDecider>();
			builder.RegisterType<OvertimeSkillIntervalData>().As<IOvertimeSkillIntervalData>();
			builder.RegisterType<OvertimeSkillIntervalDataDivider>().As<IOvertimeSkillIntervalDataDivider>().SingleInstance();
			builder.RegisterType<OvertimeSkillStaffPeriodToSkillIntervalDataMapper>().As<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>().SingleInstance();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>().SingleInstance();
			builder.RegisterType<NightlyRestRule>().As<IAssignmentPeriodRule>().SingleInstance();
			builder.RegisterType<ScheduleMatrixLockableBitArrayConverterEx>().As<IScheduleMatrixLockableBitArrayConverterEx>().SingleInstance();
			builder.RegisterType<MoveTimeDecisionMaker2>().SingleInstance();
			builder.RegisterType<MoveTimeOptimizerCreator>().InstancePerLifetimeScope();
			builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().SingleInstance();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>().SingleInstance();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>().SingleInstance();
			builder.RegisterType<ExtendReduceDaysOffHelper>().InstancePerLifetimeScope();
			builder.RegisterType<ExtendReduceTimeHelper>().InstancePerLifetimeScope();
			builder.RegisterType<ReplaceActivityService>().SingleInstance();

			builder.RegisterType<NonSecretWorkShiftCalculatorClassic>().As<IWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>().SingleInstance();

			builder.RegisterType<DayOffOptimizationDesktop>().InstancePerLifetimeScope(); 

			builder.RegisterType<WorkShiftBackToLegalStateServiceProFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleBlankSpots>().InstancePerLifetimeScope();
			builder.RegisterType<DaysOffBackToLegalState>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingInformationProvider>().SingleInstance().ApplyAspects();
			builder.RegisterType<IntradayOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<FullSchedulingResult>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<FixedStaffLoader>().As<IFixedStaffLoader>().SingleInstance();
			builder.RegisterType<PlanningGroupStaffLoader>().As<IPlanningGroupStaffLoader>().SingleInstance();
			builder.RegisterType<FetchPlanningGroupSettingsModel>().As<IFetchPlanningGroupSettingsModel>().SingleInstance();
			builder.RegisterType<FetchPlanningGroupModel>().As<IFetchPlanningGroupModel>().SingleInstance();
			builder.RegisterType<PlanningGroupSettingsMapper>().SingleInstance();
			builder.RegisterType<PlanningGroupMapper>().SingleInstance();
			builder.RegisterType<FilterMapper>().SingleInstance();
			builder.RegisterType<PlanningGroupSettingsModelPersister>().As<IPlanningGroupSettingsModelPersister>().SingleInstance();
			builder.RegisterType<PlanningGroupModelPersister>().As<IPlanningGroupModelPersister>().SingleInstance();
			builder.RegisterType<FindFilter>().SingleInstance();
			builder.RegisterType<ViolatedSchedulePeriodBusinessRule>().SingleInstance();
			builder.RegisterType<DayOffBusinessRuleValidation>().SingleInstance();
			builder.RegisterType<NoSchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<IntradayOptimizationCommandHandler>().InstancePerLifetimeScope().ApplyAspects(); //cannot be single due to gridlockmanager dep
			builder.RegisterType<SchedulePlanningPeriodCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ClearPlanningPeriodSchedule>().InstancePerLifetimeScope();
			builder.RegisterType<ClearPlanningPeriodScheduleCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<IntradayOptimizationFromWeb>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<CrossAgentsAndSkills>().SingleInstance();
			builder.RegisterType<RestrictionsAbleToBeScheduled>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftMinMaxCalculatorSkipWeekCheck>().InstancePerLifetimeScope();
			builder.RegisterType<RestrictionNotAbleToBeScheduledReport>().InstancePerLifetimeScope();
			builder.RegisterType<AdvancedAgentsFilter>().SingleInstance();
			builder.RegisterType<StaffingDataAvailablePeriodProvider>().As<IStaffingDataAvailablePeriodProvider>().SingleInstance();
			builder.RegisterType<PlanningGroupGlobalSettingSetter>().SingleInstance();

			if (_configuration.Args().IsFatClient)
			{
				builder.RegisterType<DesktopPeopleInOrganization>().As<IAllStaff>().SingleInstance();
				builder.RegisterType<DesktopContextState>()
					.As<IOptimizationPreferencesProvider>()
					.As<ICurrentOptimizationCallback>()
					.As<IBlockPreferenceProviderForPlanningPeriod>()
					.As<IDayOffOptimizationPreferenceProviderForPlanningPeriod>()
					.As<ISchedulingOptionsProvider>()
					.As<ICurrentSchedulingCallback>()
					.As<IPlanningGroupSettingsProvider>()
					.AsSelf()
					.ApplyAspects()
					.SingleInstance();
				builder.RegisterType<FillSchedulerStateHolderForDesktop>()
					.As<FillSchedulerStateHolder>()
					.ApplyAspects()
					.SingleInstance();					

				builder.RegisterType<DesktopContext>().SingleInstance();
				builder.RegisterType<MoveSchedulesToOriginalStateHolderAfterIsland>()
					.As<ISynchronizeSchedulesAfterIsland>()
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<BlockPreferenceProviderForPlanningPeriod>().As<IBlockPreferenceProviderForPlanningPeriod>().SingleInstance();
				builder.RegisterType<DayOffOptimizationPreferenceProviderForPlanningPeriod>().As<IDayOffOptimizationPreferenceProviderForPlanningPeriod>().SingleInstance();
				builder.RegisterType<PersistSchedulesAfterIsland>().As<ISynchronizeSchedulesAfterIsland>().SingleInstance();
				if (_configuration.Toggle(Toggles.ResourcePlanner_RunPerfTestAsTeam_43537))
				{
					builder.RegisterType<OptimizationPreferencesPerfTestProvider>().As<IOptimizationPreferencesProvider>().SingleInstance();
					builder.RegisterType<SchedulingOptionsProviderForTeamPerfTest>().As<ISchedulingOptionsProvider>().SingleInstance();
				}
				else
				{
					builder.RegisterType<OptimizationPreferencesDefaultValueProvider>().As<IOptimizationPreferencesProvider>().SingleInstance();
					builder.RegisterType<SchedulingOptionsProvider>().As<ISchedulingOptionsProvider>().SingleInstance();
				}
				builder.RegisterType<PeopleInOrganization>().As<IAllStaff>().SingleInstance();
				builder.RegisterType<CurrentOptimizationCallback>().As<ICurrentOptimizationCallback>().AsSelf().SingleInstance();
				builder.RegisterType<CurrentSchedulingCallback>().As<ICurrentSchedulingCallback>().AsSelf().SingleInstance();
				builder.RegisterType<FillSchedulerStateHolderFromDatabase>().As<FillSchedulerStateHolder>().ApplyAspects().SingleInstance();
				builder.RegisterType<PlanningGroupSettingsProvider>().As<IPlanningGroupSettingsProvider>().SingleInstance();
			}


			builder.RegisterType<LoaderForResourceCalculation>().InstancePerLifetimeScope();

			if (_configuration.Toggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312))
			{
				builder.RegisterType<SkillPriorityProviderForToggle41312>().As<ISkillPriorityProvider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<SkillPriorityProvider>().As<ISkillPriorityProvider>().SingleInstance();
			}
			builder.RegisterType<AnalyticsScheduleChangeForAllReportableScenariosFilter>().As<IAnalyticsScheduleChangeUpdaterFilter>().SingleInstance();
			builder.RegisterType<ThrowExceptionOnSkillMapError>().As<IAnalyticsPersonPeriodMapNotDefined>().SingleInstance();
			builder.RegisterType<AssignScheduledLayers>().SingleInstance();

			registerForJobs(builder);
			registerValidators(builder);
		}

		private void registerValidators(ContainerBuilder builder)
		{
			builder.RegisterType<NextPlanningPeriodProvider>().SingleInstance().As<INextPlanningPeriodProvider>();
			builder.RegisterType<CheckScheduleHints>().SingleInstance();
			builder.RegisterType<GetValidations>().SingleInstance();
			
			builder.RegisterType<AlreadyScheduledAgents>().SingleInstance();
			builder.RegisterType<AgentsWithPreferences>().SingleInstance();
			if (!_configuration.Args().IsFatClient)
			{
				builder.RegisterType<PreferenceHint>().As<ISchedulePostHint>().SingleInstance();
			}

			if (_configuration.Toggle(Toggles.ResourcePlanner_HintShiftBagCannotFulFillContractTime_78717))
			{
				builder.RegisterType<PersonContractShiftBagHint>().As<ISchedulePreHint>().SingleInstance();
			}
		
			builder.RegisterType<AgentsWithWhiteSpots>().SingleInstance();
			builder.RegisterType<RemoveNonPreferenceDaysOffs>().SingleInstance();
			builder.RegisterType<BlockSchedulingPreferenceHint>().As<ISchedulePostHint>().SingleInstance();
			builder.RegisterType<BlockSchedulingExistingShiftNotMatchingEachOtherHint>().As<ISchedulePostHint>().SingleInstance();
			builder.RegisterType<BlockSchedulingPreviousShiftNotMatchingEachOtherHint>().As<ISchedulePostHint>().SingleInstance();
			builder.RegisterType<BlockSchedulingNotMatchShiftBagHint>().As<ISchedulePostHint>().SingleInstance();

			builder.RegisterType<PersonContractScheduleHint>().As<ISchedulePreHint>().SingleInstance();
			builder.RegisterType<PersonContractHint>().As<ISchedulePreHint>().SingleInstance();
			builder.RegisterType<PersonPartTimePercentageHint>().As<ISchedulePreHint>().SingleInstance();
			builder.RegisterType<PersonShiftBagHint>().As<ISchedulePreHint>().SingleInstance();
			builder.RegisterType<PersonSkillHint>().As<ISchedulePreHint>().SingleInstance();
			builder.RegisterType<ScheduleStartOnWrongDateHint>().As<ISchedulePostHint>().SingleInstance();
			if (!_configuration.Args().IsFatClient)
			{
				builder.RegisterType<MissingForecastHint>().AsSelf().As<ISchedulePreHint>().SingleInstance();
				builder.RegisterType<PersonSchedulePeriodHint>().As<ISchedulePreHint>().SingleInstance();
			}
			builder.RegisterType<BusinessRulesHint>().As<ISchedulePostHint>().SingleInstance();
		}

		private static void registerMoveTimeOptimizationClasses(ContainerBuilder builder)
		{
			builder.RegisterType<ValidateFoundMovedDaysSpecification>().As<IValidateFoundMovedDaysSpecification>().SingleInstance();
			builder.RegisterType<DayValueUnlockedIndexSorter>().SingleInstance();
			builder.RegisterType<TeamBlockMoveTimeDescisionMaker>().As<ITeamBlockMoveTimeDescisionMaker>().SingleInstance();

			builder.RegisterType<TeamBlockMoveTimeBetweenDaysService>().As<ITeamBlockMoveTimeBetweenDaysService>();
			builder.RegisterType<TeamBlockMoveTimeOptimizer>().As<ITeamBlockMoveTimeOptimizer>();
		}

		private static void registerDayOffFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockDayOffFairnessOptimizationServiceFacade>().InstancePerLifetimeScope();
			builder.RegisterType<WeekDayPoints>().As<IWeekDayPoints>();
			builder.RegisterType<DayOffStep1>().As<IDayOffStep1>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffStep2>().As<IDayOffStep2>().InstancePerLifetimeScope();
			builder.RegisterType<SeniorTeamBlockLocator>().As<ISeniorTeamBlockLocator>().SingleInstance();
			builder.RegisterType<SeniorityCalculatorForTeamBlock>().As<ISeniorityCalculatorForTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockLocatorWithHighestPoints>().As<ITeamBlockLocatorWithHighestPoints>().SingleInstance();
			builder.RegisterType<WeekDayPointCalculatorForTeamBlock>().As<IWeekDayPointCalculatorForTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockDayOffDaySwapper>().As<ITeamBlockDayOffDaySwapper>().InstancePerLifetimeScope();
			builder.RegisterType<JuniorTeamBlockExtractor>().As<IJuniorTeamBlockExtractor>().SingleInstance();
			builder.RegisterType<SuitableDayOffSpotDetector>().As<ISuitableDayOffSpotDetector>().SingleInstance();
			builder.RegisterType<TeamBlockDayOffDaySwapDecisionMaker>().As<ITeamBlockDayOffDaySwapDecisionMaker>();
			builder.RegisterType<RankedPersonBasedOnStartDate>().As<IRankedPersonBasedOnStartDate>().SingleInstance();
			builder.RegisterType<PersonStartDateFromPersonPeriod>().As<IPersonStartDateFromPersonPeriod>().SingleInstance();
			builder.RegisterType<SuitableDayOffsToGiveAway>().As<ISuitableDayOffsToGiveAway>().SingleInstance();
			builder.RegisterType<PostSwapValidationForTeamBlock>().As<IPostSwapValidationForTeamBlock>().InstancePerLifetimeScope();
			builder.RegisterType<PersonDayOffPointsCalculator>().As<IPersonDayOffPointsCalculator>();
			builder.RegisterType<PersonShiftCategoryPointCalculator>().As<IPersonShiftCategoryPointCalculator>().SingleInstance();
		}

		private static void registerFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockSeniorityValidator>().As<ITeamBlockSeniorityValidator>().SingleInstance();
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSeniorityFairnessOptimizationService>().As<ITeamBlockSeniorityFairnessOptimizationService>().InstancePerLifetimeScope();
			builder.RegisterType<ConstructTeamBlock>().As<IConstructTeamBlock>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryPoints>().As<IShiftCategoryPoints>().SingleInstance();
			builder.RegisterType<ShiftCategoryPointExtractor>().As<IShiftCategoryPointExtractor>().SingleInstance();
			builder.RegisterType<SeniorityExtractor>().As<ISeniorityExtractor>().SingleInstance();
			builder.RegisterType<DetermineTeamBlockPriority>().SingleInstance();
			builder.RegisterType<TeamBlockSwapValidator>().As<ITeamBlockSwapValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSwapDayValidator>().As<ITeamBlockSwapDayValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSwap>().As<ITeamBlockSwap>();
			builder.RegisterType<TeamBlockLockValidator>().As<ITeamBlockLockValidator>().SingleInstance();
			builder.RegisterType<SeniorityTeamBlockSwapValidator>().As<ISeniorityTeamBlockSwapValidator>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffRulesValidator>().As<IDayOffRulesValidator>().SingleInstance();
			builder.RegisterType<SeniorityTeamBlockSwapper>().As<ISeniorityTeamBlockSwapper>().InstancePerLifetimeScope();

			//ITeamBlockSameTimeZoneValidator

			//common
			builder.RegisterType<TeamMemberCountValidator>().As<ITeamMemberCountValidator>().SingleInstance();
			builder.RegisterType<TeamBlockContractTimeValidator>().As<ITeamBlockContractTimeValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSameSkillValidator>().As<ITeamBlockSameSkillValidator>().SingleInstance();
			builder.RegisterType<TeamBlockPersonsSkillChecker>().As<ITeamBlockPersonsSkillChecker>().SingleInstance();
			builder.RegisterType<TeamBlockSameRuleSetBagValidator>().As<ITeamBlockSameRuleSetBagValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSameTimeZoneValidator>().As<ITeamBlockSameTimeZoneValidator>().SingleInstance();
		}

		private void registerTeamBlockCommon(ContainerBuilder builder)
		{
			builder.RegisterType<LocateMissingIntervalsIfMidNightBreak>().As<ILocateMissingIntervalsIfMidNightBreak>();
			builder.RegisterType<FilterOutIntervalsAfterMidNight>().As<IFilterOutIntervalsAfterMidNight>().SingleInstance();
			builder.RegisterType<GroupPersonSkillAggregator>().As<IGroupPersonSkillAggregator>().SingleInstance();
			builder.RegisterType<DynamicBlockFinder>().As<IDynamicBlockFinder>().SingleInstance();
			builder.RegisterType<TeamBlockInfoFactory>().As<ITeamBlockInfoFactory>().SingleInstance();
			builder.RegisterType<TeamInfoFactory>().As<ITeamInfoFactory>().InstancePerLifetimeScope();
			builder.RegisterType<SafeRollbackAndResourceCalculation>().As<ISafeRollbackAndResourceCalculation>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockClearer>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSteadyStateValidator>().As<ITeamBlockSteadyStateValidator>().InstancePerLifetimeScope();
			builder.RegisterType<ValidatedTeamBlockInfoExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<RestrictionOverLimitDecider>().As<IRestrictionOverLimitDecider>().SingleInstance();
			builder.RegisterType<RestrictionChecker>().As<ICheckerRestriction>().SingleInstance();
			builder.RegisterType<DailyTargetValueCalculatorForTeamBlock>().As<IDailyTargetValueCalculatorForTeamBlock>();
			builder.RegisterType<MedianCalculatorForDays>().SingleInstance();
			builder.RegisterType<CreateSkillIntervalDataPerDateAndActivity>().SingleInstance();
			builder.RegisterType<TwoDaysIntervalGenerator>().As<ITwoDaysIntervalGenerator>().SingleInstance();
			builder.RegisterType<MedianCalculatorForSkillInterval>().As<IMedianCalculatorForSkillInterval>().SingleInstance();
			builder.RegisterType<SkillIntervalDataOpenHour>().As<ISkillIntervalDataOpenHour>().SingleInstance();
			builder.RegisterType<SameOpenHoursInTeamBlock>().SingleInstance();
			builder.RegisterType<SameEndTimeTeamSpecification>().As<ISameEndTimeTeamSpecification>().SingleInstance();
			builder.RegisterType<SameShiftCategoryBlockSpecification>().As<ISameShiftCategoryBlockSpecification>().SingleInstance();
			builder.RegisterType<SameShiftCategoryTeamSpecification>().As<ISameShiftCategoryTeamSpecification>().SingleInstance();
			builder.RegisterType<SameStartTimeBlockSpecification>().As<ISameStartTimeBlockSpecification>().SingleInstance();
			builder.RegisterType<SameStartTimeTeamSpecification>().As<ISameStartTimeTeamSpecification>().SingleInstance();
			builder.RegisterType<SameShiftBlockSpecification>().As<ISameShiftBlockSpecification>().SingleInstance();
			builder.RegisterType<ValidSampleDayPickerFromTeamBlock>().As<IValidSampleDayPickerFromTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockSchedulingOptions>().As<ITeamBlockSchedulingOptions>().SingleInstance();
			builder.RegisterType<TeamBlockSchedulingCompletionChecker>().As<ITeamBlockSchedulingCompletionChecker>().SingleInstance();
			builder.RegisterType<ProposedRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamBlockRestrictionAggregator>().As<ITeamBlockRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamRestrictionAggregator>().As<ITeamRestrictionAggregator>().SingleInstance();
			builder.RegisterType<BlockRestrictionAggregator>().As<IBlockRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamMatrixChecker>().SingleInstance();

			builder.RegisterType<TeamMemberTerminationOnBlockSpecification>().As<ITeamMemberTerminationOnBlockSpecification>().SingleInstance();
			builder.RegisterType<SplitSchedulePeriodToWeekPeriod>().As<ISplitSchedulePeriodToWeekPeriod>().SingleInstance();
			builder.RegisterType<TeamBlockSingleDayScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftProjectionCachesForIntraInterval>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockGenerator>().As<ITeamBlockGenerator>().InstancePerLifetimeScope();
			builder.RegisterType<MissingDayOffBestSpotDecider>().As<IMissingDayOffBestSpotDecider>().SingleInstance();
		}

		private static void registerTeamBlockSchedulingService(ContainerBuilder builder)
		{
			builder.RegisterType<CreateSkillIntervalDatasPerActivtyForDate>().As<ICreateSkillIntervalDatasPerActivtyForDate>().SingleInstance();
			builder.RegisterType<CalculateAggregatedDataForActivtyAndDate>().As<ICalculateAggregatedDataForActivtyAndDate>().SingleInstance();
			builder.RegisterType<OpenHourForDate>().As<IOpenHourForDate>().SingleInstance();
			builder.RegisterType<ActivityIntervalDataCreator>().SingleInstance();
			builder.RegisterType<WorkShiftFromEditableShift>().As<IWorkShiftFromEditableShift>().SingleInstance();
			builder.RegisterType<FirstShiftInTeamBlockFinder>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockOpenHoursValidator>().As<ITeamBlockOpenHoursValidator>().SingleInstance();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>().SingleInstance();
			builder.RegisterType<TeamDayOffModifier>().InstancePerLifetimeScope();
		}

		private void registerWorkShiftSelector(ContainerBuilder builder)
		{
			builder.RegisterType<WorkShiftLengthValueCalculator>().As<IWorkShiftLengthValueCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftValueCalculator>().SingleInstance();
			builder.RegisterType<EqualWorkShiftValueDecider>().As<IEqualWorkShiftValueDecider>().SingleInstance();
			builder.RegisterType<PullTargetValueFromSkillIntervalData>().SingleInstance();
			builder.RegisterType<WorkShiftSelectorForMaxSeat>().SingleInstance();
			builder.RegisterType<IsAnySkillOpen>().SingleInstance();
		}

		private void registerMaxSeatSkillCreator(ContainerBuilder builder)
		{
			builder.RegisterType<WorkloadDayHelper>().As<IWorkloadDayHelper>().SingleInstance();
			builder.RegisterType<MaxSeatSitesExtractor>().SingleInstance();
			builder.RegisterType<ScheduledTeamBlockInfoFactory>().InstancePerLifetimeScope();
			builder.RegisterType<InitMaxSeatForStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<UsedSeatsFromResourceCalculationContext>().As<IUsedSeats>().SingleInstance();
			builder.RegisterType<MaxSeatOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationLimitsForAgentFactory>().SingleInstance();
			builder.RegisterType<SetMainShiftOptimizeActivitySpecificationForTeamBlock>().SingleInstance();
			builder.RegisterType<PersonSkillPeriodsDataHolderManager>().As<IPersonSkillPeriodsDataHolderManager>().InstancePerLifetimeScope();
			builder.RegisterType<MaxSeatSkillDataFactory>().SingleInstance();
			builder.RegisterType<SkillsFromMaxSeatSite>().SingleInstance();
		}

		private void registerWorkShiftFilters(ContainerBuilder builder)
		{
			builder.RegisterType<ActivityRestrictionsShiftFilter>().SingleInstance();
			builder.RegisterType<BusinessRulesShiftFilter>().SingleInstance();
			builder.RegisterType<CommonMainShiftFilter>().SingleInstance();
			builder.RegisterType<ContractTimeShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<DisallowedShiftCategoriesShiftFilter>().SingleInstance();
			builder.RegisterType<EarliestEndTimeLimitationShiftFilter>().As<IEarliestEndTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionShiftFilter>().SingleInstance();
			builder.RegisterType<LatestStartTimeLimitationShiftFilter>().As<ILatestStartTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<MainShiftOptimizeActivitiesSpecificationShiftFilter>().As<IMainShiftOptimizeActivitiesSpecificationShiftFilter>().SingleInstance();
			builder.RegisterType<NotOverWritableActivitiesShiftFilter>().As<INotOverWritableActivitiesShiftFilter>().SingleInstance();
			builder.RegisterType<PersonalShiftsShiftFilter>().SingleInstance();
			builder.RegisterType<RuleSetPersonalSkillsActivityFilter>().As<IRuleSetPersonalSkillsActivityFilter>().SingleInstance();
			builder.RegisterType<ActivityRequiresSkillProjectionFilter>().SingleInstance();
			builder.RegisterType<ShiftCategoryRestrictionShiftFilter>().SingleInstance();
			builder.RegisterType<ValidDateTimePeriodShiftFilter>().SingleInstance();
			builder.RegisterType<TimeLimitsRestrictionShiftFilter>().SingleInstance();
			builder.RegisterType<WorkTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<CommonActivityFilter>().SingleInstance();
			builder.RegisterType<RuleSetBagExtractorProvider>().SingleInstance();
			builder.RegisterType<TeamBlockIncludedWorkShiftRuleFilter>().SingleInstance();
			builder.RegisterType<RuleSetSkillActivityChecker>().As<IRuleSetSkillActivityChecker>().SingleInstance();
			builder.RegisterType<PersonalShiftAndMeetingFilter>().As<IPersonalShiftAndMeetingFilter>().SingleInstance();
			builder.RegisterType<PersonalShiftMeetingTimeChecker>().As<IPersonalShiftMeetingTimeChecker>().SingleInstance();
			builder.RegisterType<DisallowedShiftProjectionCachesFilter>().SingleInstance();

			builder.RegisterType<OutboundSkillCreator>().As<IOutboundSkillCreator>().SingleInstance();
			builder.RegisterType<SkillTypeProvider>().As<ISkillTypeProvider>().SingleInstance();
			builder.RegisterType<OutboundSkillPersister>().As<IOutboundSkillPersister>().SingleInstance();
			builder.RegisterType<IncomingTaskFactory>().As<IncomingTaskFactory>().SingleInstance();
			builder.RegisterType<FlatDistributionSetter>().As<FlatDistributionSetter>().SingleInstance();
			builder.RegisterType<OutboundProductionPlanFactory>().As<OutboundProductionPlanFactory>().SingleInstance();
			builder.RegisterType<CreateOrUpdateSkillDays>().As<ICreateOrUpdateSkillDays>().SingleInstance();
			builder.RegisterType<OutboundPeriodMover>().As<IOutboundPeriodMover>().SingleInstance();
			builder.RegisterType<OutboundCampaignRepository>().As<IOutboundCampaignRepository>().SingleInstance();

			builder.RegisterType<OutboundScheduledResourcesProvider>().As<IOutboundScheduledResourcesProvider>().SingleInstance();
			builder.RegisterType<OpenHoursFilter>().SingleInstance();
		}

		private static void registerForJobs(ContainerBuilder builder)
		{
			builder.RegisterType<PersonAbsenceAccountProvider>().As<IPersonAbsenceAccountProvider>();
			builder.RegisterType<ResourceCalculationPrerequisitesLoader>().As<IResourceCalculationPrerequisitesLoader>();
			builder.RegisterType<LoadSchedulingStateHolderForResourceCalculation>().As<ILoadSchedulingStateHolderForResourceCalculation>().SingleInstance();
			builder.RegisterType<LoadSchedulesForRequestWithoutResourceCalculation>().As<ILoadSchedulesForRequestWithoutResourceCalculation>().SingleInstance();
			builder.RegisterType<BudgetGroupHeadCountSpecification>().As<IBudgetGroupHeadCountSpecification>();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>();
			builder.RegisterType<SwapService>().As<ISwapService>();
		}
	}
}