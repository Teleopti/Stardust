﻿using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class SchedulingCommonModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public SchedulingCommonModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (_configuration.Toggle(Toggles.ResourcePlanner_FasterLoading_46307))
			{
				builder.RegisterType<ScheduleStorage46307>()
					.UsingConstructor(typeof(ICurrentUnitOfWork), typeof(IRepositoryFactory), typeof(IPersistableScheduleDataPermissionChecker), typeof(IScheduleStorageRepositoryWrapper))
					.As<IScheduleStorage>()
					.As<IFindSchedulesForPersons>()
					.AsSelf()
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<ScheduleStorage>()
					.UsingConstructor(typeof(ICurrentUnitOfWork), typeof(IRepositoryFactory), typeof(IPersistableScheduleDataPermissionChecker), typeof(IScheduleStorageRepositoryWrapper))
					.As<IScheduleStorage>()
					.As<IFindSchedulesForPersons>()
					.AsSelf()
					.SingleInstance();
			}

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
			
			//	builder.RegisterType<PrimarySkillOverstaffOLD>().As<PrimarySkillOverstaff>().SingleInstance();
			//	builder.RegisterType<AddResourcesToSubSkillsOLD>().As<AddResourcesToSubSkills>().SingleInstance();
			
			builder.RegisterType<ResourceOptimizationHelper>().SingleInstance();
			builder.RegisterType<CascadingResourceCalculation>().As<IResourceCalculation>().SingleInstance();
			builder.RegisterType<CascadingResourceCalculationContextFactory>().SingleInstance();
			builder.RegisterType<CascadingPersonSkillProvider>().SingleInstance();
			builder.RegisterType<PersonalSkillsProvider>().SingleInstance();
			if (_configuration.Toggle(Toggles.ResourcePlanner_EasierBlockScheduling_46155))
			{
				builder.RegisterType<DaysOffInPeriodValidatorForBlock>().As<IDaysOffInPeriodValidatorForBlock>().SingleInstance();
			}
			else
			{
				builder.RegisterType<DaysOffInPeriodValidatorForBlockOLD>().As<IDaysOffInPeriodValidatorForBlock>().SingleInstance();
			}

			builder.RegisterType<SharedResourceContextOldSchedulingScreenBehaviorWithoutShoveling>().InstancePerLifetimeScope();
			builder.RegisterType<SharedResourceContextOldSchedulingScreenBehavior>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulerStateScheduleDayChangedCallback>().As<IScheduleDayChangeCallback>().InstancePerLifetimeScope();
			builder.RegisterModule<IntraIntervalOptimizationServiceModule>();
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
			builder.RegisterModule<BackToLegalShiftModule>();
			builder.RegisterModule(new ScheduleOvertimeModule(_configuration));
			builder.RegisterType<DoFullResourceOptimizationOneTime>().InstancePerLifetimeScope();
			if (_configuration.Toggle(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582))
			{
				builder.RegisterType<RemoveShiftCategoryToBeInLegalStateAlwaysTeamBlock>().As<RemoveShiftCategoryToBeInLegalState>().InstancePerLifetimeScope();				
			}
			else
			{
				builder.RegisterType<RemoveShiftCategoryToBeInLegalState>().InstancePerLifetimeScope();
			}
			builder.RegisterType<ScheduleHourlyStaffExecutor>().InstancePerLifetimeScope();
		
			builder.RegisterType<ScheduleExecutor>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<Scheduling>().InstancePerLifetimeScope();
			builder.RegisterType<RuleSetAccordingToAccessabilityFilter>().SingleInstance();
			builder.RegisterType<WorkShiftFilterService>().As<IWorkShiftFilterService>().InstancePerLifetimeScope();
			builder.RegisterType<AdvanceDaysOffSchedulingService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamDayOffScheduling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockMissingDayOffHandling>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduleSelected>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
			builder.RegisterType<SchedulingCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<FullScheduling>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<DesktopScheduling>().InstancePerLifetimeScope();

			builder.RegisterType<ExtendSelectedPeriodForMonthlyScheduling>().SingleInstance();
			builder.RegisterType<LowThreadPriorityScope>().As<ILowThreadPriorityScope>().SingleInstance();
			builder.RegisterType<OptimizationDesktopExecuter>().InstancePerLifetimeScope();
			builder.RegisterType<CorrectAlteredBetween>().SingleInstance();
			builder.RegisterType<ScheduleOptimizerHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PersonListExtractorFromScheduleParts>().As<IPersonListExtractorFromScheduleParts>().SingleInstance();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>().InstancePerLifetimeScope();
			builder.RegisterType<FlexibelDayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>().SingleInstance();

			//change to scope? 
			builder.RegisterType<ScheduleOvertime>();
			builder.RegisterType<TeamBlockMoveTimeBetweenDaysCommand>().As<ITeamBlockMoveTimeBetweenDaysCommand>();
			builder.RegisterType<MatrixListFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockIntradayOptimizationService>();				
			builder.RegisterType<TeamBlockDaysOffSameDaysOffLockSyncronizer>().SingleInstance();
			builder.RegisterType<WeeklyRestSolverCommand>().As<IWeeklyRestSolverCommand>().ApplyAspects();
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
			builder.RegisterType<SchedulingResultStateHolderProvider>().As<ISchedulingResultStateHolderProvider>().SingleInstance();
			builder.RegisterType<ScheduleDayForPerson>().As<IScheduleDayForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRangeForPerson>().As<IScheduleRangeForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<PreSchedulingStatusChecker>().As<IPreSchedulingStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<IntradayOptimizationContext>().InstancePerLifetimeScope();
			builder.RegisterType<NightRestWhiteSpotSolverServiceFactory>().As<INightRestWhiteSpotSolverServiceFactory>().InstancePerLifetimeScope();
			builder.RegisterType<NumberOfAgentsKnowingSkill>().SingleInstance();
			builder.RegisterType<ReduceSkillSets>().SingleInstance();
			builder.RegisterType<CreateIslands>().SingleInstance();
			builder.RegisterType<MoveSkillSetToCorrectIsland>().SingleInstance();
			builder.RegisterType<SkillSetProvider>().SingleInstance();
			builder.RegisterType<SkillSetContext>().SingleInstance();
			builder.RegisterType<IslandModelFactory>().SingleInstance();
			builder.RegisterType<CreateSkillSets>().SingleInstance();
			builder.RegisterType<ReduceIslandsLimits>().SingleInstance();
			builder.RegisterType<LongestPeriodForAssignmentCalculator>()
				.As<ILongestPeriodForAssignmentCalculator>()
				.SingleInstance();
			builder.RegisterType<ShiftProjectionCacheManager>().As<IShiftProjectionCacheManager>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftsFromMasterActivityBaseActivityService>().As<IShiftFromMasterActivityService>().SingleInstance();			
			builder.RegisterType<RuleSetDeletedActivityChecker>().As<IRuleSetDeletedActivityChecker>().SingleInstance();
			builder.RegisterType<RuleSetDeletedShiftCategoryChecker>()
				.As<IRuleSetDeletedShiftCategoryChecker>()
				.SingleInstance();
			builder.RegisterType<WorkShiftCalculatorsManager>().As<IWorkShiftCalculatorsManager>().SingleInstance();
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
			builder.RegisterType<PersonSkillDayCreator>().As<IPersonSkillDayCreator>().SingleInstance();

			builder.RegisterType<WorkShiftFinderService>().InstancePerLifetimeScope();
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
			builder.RegisterType<IntradayOptimizerCreator>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleOvertimeOnNonScheduleDays>().InstancePerLifetimeScope();

			builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().SingleInstance();
			builder.Register(c => new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().InstancePerLifetimeScope();

			builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();

			//OptimizationCommand
			builder.RegisterType<SkillResolutionProvider>().As<ISkillResolutionProvider>().SingleInstance();
			builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().SingleInstance();
			builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().SingleInstance();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().SingleInstance();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().SingleInstance();
			builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().SingleInstance();
			builder.RegisterType<DeleteAndResourceCalculateService>().As<IDeleteAndResourceCalculateService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
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
			builder.RegisterType<MatrixNotPermittedLocker>().SingleInstance();
			builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().As<IScheduleMatrixValueCalculatorProFactory>().SingleInstance();
			builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().SingleInstance();
			builder.RegisterType<SchedulePeriodListShiftCategoryBackToLegalStateService>().As<ISchedulePeriodListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();

			builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().SingleInstance();
			builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
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
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().As<ITeamBlockRestrictionOverLimitValidator>().SingleInstance();
			builder.RegisterType<TeamBlockOptimizationLimits>().As<ITeamBlockOptimizationLimits>().SingleInstance();
			builder.RegisterType<RestrictionOverLimitValidator>().SingleInstance();
			builder.RegisterType<TeamBlockDayOffOptimizer>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<DayOffOptimizerStandard>().InstancePerLifetimeScope();
			builder.RegisterType<AffectedDayOffs>().SingleInstance();
			builder.RegisterType<DayOffOptimizerPreMoveResultPredictor>().InstancePerLifetimeScope();

			builder.RegisterType<TeamBlockRemoveShiftCategoryOnBestDateService>().As<ITeamBlockRemoveShiftCategoryOnBestDateService>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockRetryRemoveShiftCategoryBackToLegalService>().InstancePerLifetimeScope();
			builder.RegisterType<RemoveScheduleDayProsBasedOnShiftCategoryLimitation>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftCategoryWeekRemover>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryPeriodRemover>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryLimitCounter>().As<IShiftCategoryLimitCounter>().SingleInstance();

			builder.RegisterType<TeamBlockDayOffsInPeriodValidator>().As<TeamBlockDayOffsInPeriodValidator>().As<ITeamBlockDayOffsInPeriodValidator>().SingleInstance();

			//ITeamBlockRestrictionOverLimitValidator
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().SingleInstance();
			builder.RegisterType<HasContractDayOffDefinition>().As<IHasContractDayOffDefinition>().SingleInstance();
			builder.RegisterType<ScheduleDayDataMapper>().As<IScheduleDayDataMapper>().SingleInstance();
			builder.RegisterType<MatrixDataListCreator>().As<IMatrixDataListCreator>().SingleInstance();
			builder.RegisterType<MatrixDataWithToFewDaysOff>().As<IMatrixDataWithToFewDaysOff>().InstancePerLifetimeScope();

			builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().SingleInstance();
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
			builder.RegisterType<MoveTimeDecisionMaker2>().As<IMoveTimeDecisionMaker>().SingleInstance();
			builder.RegisterType<MoveTimeOptimizerCreator>().InstancePerLifetimeScope();
			builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().SingleInstance();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>().SingleInstance();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>().SingleInstance();
			builder.RegisterType<ExtendReduceDaysOffHelper>().InstancePerLifetimeScope();
			builder.RegisterType<ExtendReduceTimeHelper>().InstancePerLifetimeScope();

			if (_configuration.Toggle(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002))
			{
				builder.RegisterType<EffectiveRestrictionStartTimeDecider>().As<IEffectiveRestrictionStartTimeDecider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<EffectiveRestrictionStartTimeDeciderOff>().As<IEffectiveRestrictionStartTimeDecider>().SingleInstance();	
			}

			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>().SingleInstance();

			builder.RegisterType<DayOffOptimization>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<DayOffOptimizationDesktopTeamBlock>().InstancePerLifetimeScope(); 

			builder.RegisterType<OptimizerHelperHelper>().SingleInstance();
			builder.RegisterType<WorkShiftBackToLegalStateServiceProFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleBlankSpots>().InstancePerLifetimeScope();
			builder.RegisterType<DaysOffBackToLegalState>().InstancePerLifetimeScope();
			builder.RegisterType<FullSchedulingResult>().SingleInstance();
			builder.RegisterType<SuccessfulScheduledAgents>().SingleInstance();
			builder.RegisterType<SchedulingInformationProvider>().SingleInstance().ApplyAspects();
			if (_configuration.Toggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508))
			{
				builder.RegisterType<IntradayOptimization>().As<IIntradayOptimization>().InstancePerLifetimeScope();
				//remove As<xxxOLD> when toggle is gone
				builder.RegisterType<TeamBlockDesktopOptimization>().As<TeamBlockDesktopOptimizationOLD>(); //set scope? Just keep old behavior for now...
			}
			else
			{
				builder.RegisterType<TeamBlockDesktopOptimizationOLD>();
				builder.RegisterType<IntradayOptimizationClassic>().As<IIntradayOptimization>().InstancePerLifetimeScope();
			}
			builder.RegisterType<OptimizationResult>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<IntradayDecisionMaker>().SingleInstance();
			builder.RegisterType<FixedStaffLoader>().As<IFixedStaffLoader>().SingleInstance();
			builder.RegisterType<PlanningGroupStaffLoader>().As<IPlanningGroupStaffLoader>().SingleInstance();
			builder.RegisterType<FetchPlanningGroupSettingsModel>().As<IFetchPlanningGroupSettingsModel>().SingleInstance();
			builder.RegisterType<FetchPlanningGroupModel>().As<IFetchPlanningGroupModel>().SingleInstance();
			builder.RegisterType<PlanningGroupSettingsMapper>().SingleInstance();
			builder.RegisterType<PlanningGroupMapper>().SingleInstance();
			builder.RegisterType<FilterMapper>().SingleInstance();
			builder.RegisterType<PlanningGroupSettingsModelPersister>().As<IPlanningGroupSettingsModelPersister>().SingleInstance();
			builder.RegisterType<PlanningGroupModelPersister>().As<IPlanningGroupModelPersister>().SingleInstance();
			builder.RegisterType<BlockPreferenceProviderUsingFiltersFactory>().AsSelf().SingleInstance();
			builder.RegisterType<DayOffOptimizationPreferenceProviderUsingFiltersFactory>().AsSelf().SingleInstance();
			builder.RegisterType<FindFilter>().SingleInstance();
			builder.RegisterType<ViolatedSchedulePeriodBusinessRule>().SingleInstance();
			builder.RegisterType<DayOffBusinessRuleValidation>().SingleInstance();
			builder.RegisterType<ResourceCalculateAfterDeleteDecider>().As<IResourceCalculateAfterDeleteDecider>().SingleInstance();
			builder.RegisterType<LimitForNoResourceCalculation>().As<ILimitForNoResourceCalculation>().AsSelf().SingleInstance();
			builder.RegisterType<NoSchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<IntradayOptimizerContainerConsiderLargeSkillSets>().As<IIntradayOptimizerContainer>().SingleInstance();
			builder.RegisterType<AgentsToSkillSets>().SingleInstance();
			builder.RegisterType<IntradayOptimizerLimiter>().As<IIntradayOptimizerLimiter>().AsSelf().SingleInstance();
			builder.RegisterType<IntradayOptimizeOnDayCallBackDoNothing>().As<IIntradayOptimizeOneDayCallback>().SingleInstance();
			builder.RegisterType<IntradayOptimizationCommandHandler>().InstancePerLifetimeScope().ApplyAspects(); //cannot be single due to gridlockmanager dep
			builder.RegisterType<SchedulePlanningPeriodCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<ClearPlanningPeriodSchedulingCommandHandler>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<IntradayOptimizationFromWeb>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<OptimizeIntradayIslandsDesktop>().InstancePerLifetimeScope();
			builder.RegisterType<CrossAgentsAndSkills>().SingleInstance();

			if (_configuration.Args().IsFatClient)
			{
				builder.RegisterType<DesktopPeopleInOrganization>().As<IPeopleInOrganization>().SingleInstance();
				builder.RegisterType<DesktopOptimizationContext>()
					.As<IOptimizationPreferencesProvider>()
					.As<ICurrentIntradayOptimizationCallback>()
					.AsSelf()
					.ApplyAspects()
					.SingleInstance();
				builder.RegisterType<FillSchedulerStateHolderForDesktop>()
					.As<FillSchedulerStateHolder>()
					.ApplyAspects()
					.SingleInstance();
				builder.RegisterType<DesktopContext>().SingleInstance();
				builder.RegisterType<DesktopSchedulingContext>()
					.As<ISchedulingOptionsProvider>()
					.As<ICurrentSchedulingCallback>()
					.AsSelf()
					.SingleInstance();
				builder.RegisterType<MoveSchedulesToOriginalStateHolderAfterIsland>()
					.As<ISynchronizeSchedulesAfterIsland>()
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<PersistSchedulesAfterIsland>().As<ISynchronizeSchedulesAfterIsland>().SingleInstance();
				if (_configuration.Toggle(Toggles.ResourcePlanner_RunPerfTestAsTeam_43537))
				{
					builder.RegisterType<OptimizationPreferencesPerfTestProvider>().As<IOptimizationPreferencesProvider>().SingleInstance();
					builder.RegisterType<SchedulingOptionsProviderForTeamPerfTest>().As<ISchedulingOptionsProvider>().SingleInstance();
				}
				else
				{
					builder.RegisterType<OptimizationPreferencesDefaultValueProvider>().AsSelf().As<IOptimizationPreferencesProvider>().SingleInstance();
					builder.RegisterType<SchedulingOptionsProvider>().As<ISchedulingOptionsProvider>().AsSelf().SingleInstance();
				}
				builder.RegisterType<PeopleInOrganization>().As<IPeopleInOrganization>().SingleInstance();
				builder.RegisterType<CurrentIntradayOptimizationCallback>().As<ICurrentIntradayOptimizationCallback>().AsSelf().SingleInstance();
				builder.RegisterType<CurrentSchedulingCallback>().As<ICurrentSchedulingCallback>().AsSelf().SingleInstance();
				builder.RegisterType<FillSchedulerStateHolderFromDatabase>().As<FillSchedulerStateHolder>().ApplyAspects().SingleInstance();
			}


			builder.RegisterType<LoaderForResourceCalculation>().InstancePerLifetimeScope();
			builder.RegisterType<ExtractSkillStaffingDataForResourceCalculation>().InstancePerLifetimeScope();

			

			if (_configuration.Toggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312))
			{
				builder.RegisterType<SkillPriorityProviderForToggle41312>().As<ISkillPriorityProvider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<SkillPriorityProvider>().As<ISkillPriorityProvider>().SingleInstance();
			}

			// Analytics fact schedule updates
			if (_configuration.Toggle(Toggles.ETL_SpeedUpFactScheduleNightly_38019))
			{
				builder.RegisterType<AnalyticsScheduleChangeForAllReportableScenariosFilter>().As<IAnalyticsScheduleChangeUpdaterFilter>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AnalyticsScheduleChangeForDefaultScenarioFilter>().As<IAnalyticsScheduleChangeUpdaterFilter>().SingleInstance();
			}

			// Analytics skill updater on person when skills not exists in analytics failure
			if (_configuration.Toggle(Toggles.ETL_SpeedUpNightlySkill_37543))
			{
				builder.RegisterType<ThrowExceptionOnSkillMapError>().As<IAnalyticsPersonPeriodMapNotDefined>().SingleInstance();
			}
			else
			{
				builder.RegisterType<ReturnNotDefined>().As<IAnalyticsPersonPeriodMapNotDefined>().SingleInstance();
			}

			builder.RegisterType<AssignScheduledLayers>().SingleInstance();

			registerForJobs(builder);
			registerValidators(builder);
		}

		private void registerValidators(ContainerBuilder builder)
		{
			builder.RegisterType<NextPlanningPeriodProvider>().SingleInstance().As<INextPlanningPeriodProvider>();
			builder.RegisterType<SchedulingValidator>().SingleInstance();
			if (!_configuration.Args().IsFatClient)
			{
				builder.RegisterType<MissingForecastValidator>().AsSelf().As<IScheduleValidator>().SingleInstance();
				builder.RegisterType<PersonSchedulePeriodValidator>().As<IScheduleValidator>().SingleInstance();				
			}
			if (_configuration.Toggle(Toggles.ResourcePlanner_ShowSwitchedTimeZone_46303))
			{
				builder.RegisterType<ScheduleStartOnWrongDateValidator>().As<IScheduleValidator>().SingleInstance();
			}
			builder.RegisterType<BusinessRulesValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonSkillValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonPeriodValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonShiftBagValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonPartTimePercentageValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonContractValidator>().As<IScheduleValidator>().SingleInstance();
			builder.RegisterType<PersonContractScheduleValidator>().As<IScheduleValidator>().SingleInstance();
			
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
			builder.RegisterType<TeamBlockDayOffFairnessOptimizationServiceFacade>().As<ITeamBlockDayOffFairnessOptimizationServiceFacade>().InstancePerLifetimeScope();
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
			builder.RegisterType<DetermineTeamBlockPriority>().As<IDetermineTeamBlockPriority>().SingleInstance();
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

		private static void registerTeamBlockCommon(ContainerBuilder builder)
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
			builder.RegisterType<MedianCalculatorForDays>().As<IMedianCalculatorForDays>().SingleInstance();
			builder.RegisterType<TwoDaysIntervalGenerator>().As<ITwoDaysIntervalGenerator>().SingleInstance();
			builder.RegisterType<MedianCalculatorForSkillInterval>().As<IMedianCalculatorForSkillInterval>().SingleInstance();
			builder.RegisterType<SkillIntervalDataOpenHour>().As<ISkillIntervalDataOpenHour>().SingleInstance();
			builder.RegisterType<SameOpenHoursInTeamBlock>().As<ISameOpenHoursInTeamBlock>().SingleInstance();
			builder.RegisterType<SameEndTimeTeamSpecification>().As<ISameEndTimeTeamSpecification>().SingleInstance();
			builder.RegisterType<SameShiftCategoryBlockSpecification>().As<ISameShiftCategoryBlockSpecification>().SingleInstance();
			builder.RegisterType<SameShiftCategoryTeamSpecification>().As<ISameShiftCategoryTeamSpecification>().SingleInstance();
			builder.RegisterType<SameStartTimeBlockSpecification>().As<ISameStartTimeBlockSpecification>().SingleInstance();
			builder.RegisterType<SameStartTimeTeamSpecification>().As<ISameStartTimeTeamSpecification>().SingleInstance();
			builder.RegisterType<SameShiftBlockSpecification>().As<ISameShiftBlockSpecification>().SingleInstance();
			builder.RegisterType<ValidSampleDayPickerFromTeamBlock>().As<IValidSampleDayPickerFromTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockSchedulingOptions>().As<ITeamBlockSchedulingOptions>().SingleInstance();
			builder.RegisterType<TeamBlockRoleModelSelector>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSchedulingCompletionChecker>().As<ITeamBlockSchedulingCompletionChecker>().SingleInstance();
			builder.RegisterType<ProposedRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamBlockRestrictionAggregator>().As<ITeamBlockRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamRestrictionAggregator>().As<ITeamRestrictionAggregator>().SingleInstance();
			builder.RegisterType<BlockRestrictionAggregator>().As<IBlockRestrictionAggregator>().SingleInstance();
			builder.RegisterType<TeamMatrixChecker>().As<ITeamMatrixChecker>().SingleInstance();

			builder.RegisterType<TeamMemberTerminationOnBlockSpecification>().As<ITeamMemberTerminationOnBlockSpecification>().SingleInstance();
			builder.RegisterType<SplitSchedulePeriodToWeekPeriod>().As<ISplitSchedulePeriodToWeekPeriod>().SingleInstance();
			builder.RegisterType<TeamScheduling>().SingleInstance();
			builder.RegisterType<TeamBlockSingleDayScheduler>().As<ITeamBlockSingleDayScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftProjectionCachesForIntraInterval>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockGenerator>().As<ITeamBlockGenerator>().InstancePerLifetimeScope();
			builder.RegisterType<MissingDayOffBestSpotDecider>().As<IMissingDayOffBestSpotDecider>().SingleInstance();
		}

		private static void registerTeamBlockSchedulingService(ContainerBuilder builder)
		{

			builder.RegisterType<CreateSkillIntervalDatasPerActivtyForDate>().As<ICreateSkillIntervalDatasPerActivtyForDate>().SingleInstance();
			builder.RegisterType<CalculateAggregatedDataForActivtyAndDate>().As<ICalculateAggregatedDataForActivtyAndDate>().SingleInstance();
			builder.RegisterType<CreateSkillIntervalDataPerDateAndActivity>().As<ICreateSkillIntervalDataPerDateAndActivity>().SingleInstance();
			builder.RegisterType<OpenHourForDate>().As<IOpenHourForDate>().SingleInstance();
			builder.RegisterType<ActivityIntervalDataCreator>().As<IActivityIntervalDataCreator>().SingleInstance();
			builder.RegisterType<WorkShiftFromEditableShift>().As<IWorkShiftFromEditableShift>().SingleInstance();
			builder.RegisterType<FirstShiftInTeamBlockFinder>().As<IFirstShiftInTeamBlockFinder>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockOpenHoursValidator>().As<ITeamBlockOpenHoursValidator>().SingleInstance();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>().SingleInstance();
			builder.RegisterType<TeamDayOffModifier>().As<ITeamDayOffModifier>().InstancePerLifetimeScope();
		}

		private void registerWorkShiftSelector(ContainerBuilder builder)
		{
			builder.RegisterType<WorkShiftLengthValueCalculator>().As<IWorkShiftLengthValueCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftValueCalculator>().As<IWorkShiftValueCalculator>().SingleInstance();
			builder.RegisterType<EqualWorkShiftValueDecider>().As<IEqualWorkShiftValueDecider>().SingleInstance();
			builder.RegisterType<WorkShiftSelector>().As<IWorkShiftSelector>().As<IWorkShiftSelectorForIntraInterval>().SingleInstance();
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
			builder.RegisterType<ScheduleIsInvalidSpecification>().As<IScheduleIsInvalidSpecification>();
			builder.RegisterType<BudgetGroupHeadCountSpecification>().As<IBudgetGroupHeadCountSpecification>();
			builder.RegisterType<SwapAndModifyService>().As<ISwapAndModifyService>();
			builder.RegisterType<SwapService>().As<ISwapService>();
		}
	}
}