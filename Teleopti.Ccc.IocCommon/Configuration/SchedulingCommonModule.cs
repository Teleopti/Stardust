using System.Diagnostics.CodeAnalysis;
using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.IslandScheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
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
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SchedulingCommonModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public SchedulingCommonModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new IntraIntervalOptimizationServiceModule(_configuration));
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
			builder.RegisterModule<BackToLegalShiftModule>();
			builder.RegisterModule<ScheduleOvertimeModule>();

			builder.RegisterType<ClassicScheduleCommand>().As<IClassicScheduleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleCommand>().As<IScheduleCommand>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<OptimizationCommand>().As<IOptimizationCommand>().InstancePerLifetimeScope();
			builder.RegisterType<ClassicDaysOffOptimizationCommand>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftBackToLegalStateServiceFactory>().As<IWorkShiftBackToLegalStateServiceFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PersonListExtractorFromScheduleParts>().As<IPersonListExtractorFromScheduleParts>().SingleInstance();

			builder.RegisterType<FlexibelDayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>().SingleInstance();

			builder.RegisterType<ScheduleOvertimeCommand>().As<IScheduleOvertimeCommand>();
			builder.RegisterType<TeamBlockMoveTimeBetweenDaysCommand>().As<ITeamBlockMoveTimeBetweenDaysCommand>();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>();
			builder.RegisterType<MatrixListFactory>().As<IMatrixListFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduleCommand>().As<ITeamBlockScheduleCommand>();
			builder.RegisterType<TeamBlockOptimizationCommand>().As<ITeamBlockOptimizationCommand>();
			builder.RegisterType<WeeklyRestSolverCommand>().As<IWeeklyRestSolverCommand>();
			builder.RegisterType<BackToLegalShiftCommand>();
			builder.RegisterType<IntraIntervalOptimizationCommand>().As<IIntraIntervalOptimizationCommand>();
			builder.RegisterType<GroupPersonBuilderWrapper>().As<IGroupPersonBuilderWrapper>().InstancePerLifetimeScope();

			builder.RegisterType<DisableDeletedFilter>().As<IDisableDeletedFilter>().SingleInstance();
			builder.RegisterType<InnerOptimizerHelperHelper>().As<IOptimizerHelperHelper>().SingleInstance();
			builder.RegisterType<ResourceOptimizationHelperExtended>().As<IResourceOptimizationHelperExtended>().InstancePerLifetimeScope();
			builder.RegisterType<RequiredScheduleHelper>().As<IRequiredScheduleHelper>().InstancePerLifetimeScope();
			builder.RegisterType<CommonStateHolder>().As<ICommonStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodExtractorFromScheduleParts>().SingleInstance();

			builder.RegisterType<IslandScheduler>().As<IslandScheduler>().InstancePerLifetimeScope();
			builder.RegisterModule<WeeklyRestSolverModule>();
			builder.RegisterModule<EqualNumberOfCategoryFairnessModule>();
			
			builder.RegisterType<SchedulerStateHolder>()
				.As<ISchedulerStateHolder>()
				.As<IClearReferredShiftTradeRequests>()
				.AsSelf()
				.InstancePerLifetimeScope();
			builder.RegisterType<TimeZoneGuardWrapper>().As<ITimeZoneGuard>().SingleInstance();
			builder.RegisterType<OverriddenBusinessRulesHolder>().As<IOverriddenBusinessRulesHolder>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayForPerson>().As<IScheduleDayForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRangeForPerson>().As<IScheduleRangeForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<PreSchedulingStatusChecker>().As<IPreSchedulingStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<VirtualSkillGroupsResultProvider>().SingleInstance();
			builder.RegisterType<VirtualSkillContext>().InstancePerLifetimeScope();
			builder.RegisterType<IntradayOptimizationContext>().InstancePerLifetimeScope();

			builder.RegisterType<VirtualSkillGroupsCreator>().SingleInstance();

			builder.RegisterType<SeatLimitationWorkShiftCalculator2>()
				.As<ISeatLimitationWorkShiftCalculator2>()
				.SingleInstance();
			builder.RegisterType<SeatImpactOnPeriodForProjection>()
				.As<ISeatImpactOnPeriodForProjection>()
				.SingleInstance();
			builder.RegisterType<LongestPeriodForAssignmentCalculator>()
				.As<ILongestPeriodForAssignmentCalculator>()
				.InstancePerLifetimeScope();
			builder.RegisterType<PersonSkillPeriodsDataHolderManager>()
				.As<IPersonSkillPeriodsDataHolderManager>()
				.InstancePerLifetimeScope();
			builder.RegisterType<ShiftProjectionCacheManager>().As<IShiftProjectionCacheManager>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftFromMasterActivityService>()
				.As<IShiftFromMasterActivityService>()
				.InstancePerLifetimeScope();
			builder.RegisterType<RuleSetDeletedActivityChecker>().As<IRuleSetDeletedActivityChecker>().SingleInstance();
			builder.RegisterType<RuleSetDeletedShiftCategoryChecker>()
				.As<IRuleSetDeletedShiftCategoryChecker>()
				.SingleInstance();
			builder.RegisterType<WorkShiftCalculatorsManager>().As<IWorkShiftCalculatorsManager>().InstancePerLifetimeScope();
			builder.RegisterType<FairnessAndMaxSeatCalculatorsManager28317>().As<IFairnessAndMaxSeatCalculatorsManager>()
				.InstancePerLifetimeScope();
			builder.RegisterType<SchedulerStateScheduleDayChangedCallback>()
				.As<IScheduleDayChangeCallback>()
				.InstancePerDependency();
			builder.RegisterType<ResourceCalculateDaysDecider>().As<IResourceCalculateDaysDecider>().SingleInstance();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().SingleInstance();
			builder.RegisterGeneric(typeof(PairMatrixService<>)).As(typeof(IPairMatrixService<>)).SingleInstance();
			builder.RegisterGeneric(typeof(PairDictionaryFactory<>)).As(typeof(IPairDictionaryFactory<>)).SingleInstance();
			builder.RegisterType<OptimizationPreferences>().As<IOptimizationPreferences>().InstancePerLifetimeScope();
			builder.RegisterType<DaysOffPreferences>().As<IDaysOffPreferences>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffPlannerRules>().As<IDayOffPlannerRules>().InstancePerLifetimeScope();
			builder.RegisterType<NonBlendWorkShiftCalculator>().As<INonBlendWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>()
				.As<INonBlendSkillImpactOnPeriodForProjection>()
				.SingleInstance();
			builder.RegisterType<GroupPagePerDateHolder>().As<IGroupPagePerDateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<DesiredShiftLengthCalculator>().As<IDesiredShiftLengthCalculator>().SingleInstance();
			builder.RegisterType<ShiftLengthDecider>().As<IShiftLengthDecider>().SingleInstance();
			builder.RegisterType<PersonSkillDayCreator>().As<IPersonSkillDayCreator>().SingleInstance();
			builder.RegisterType<WorkShiftFinderService>().As<IWorkShiftFinderService>().InstancePerLifetimeScope();

			builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>()
				.As<ISkillVisualLayerCollectionDictionaryCreator>()
				.SingleInstance();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().SingleInstance();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().SingleInstance();
			builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().SingleInstance();
			builder.RegisterType<ShiftCategoryLimitationChecker>().As<IShiftCategoryLimitationChecker>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulePartModifyAndRollbackService>().As<ISchedulePartModifyAndRollbackService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduler>().As<IAbsencePreferenceScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceFullDayLayerCreator>().As<IAbsencePreferenceFullDayLayerCreator>().SingleInstance();
			builder.RegisterType<DayOffScheduler>().As<IDayOffScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayAvailableForDayOffSpecification>().As<IScheduleDayAvailableForDayOffSpecification>().SingleInstance();
			builder.RegisterType<IntradayOptimizer2Creator>().InstancePerLifetimeScope();

			builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().SingleInstance();
			builder.Register(c => new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().InstancePerLifetimeScope();
			builder.RegisterType<FixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();

			//OptimizationCommand


			builder.RegisterType<AdvanceDaysOffSchedulingService>().As<IAdvanceDaysOffSchedulingService>();
			builder.RegisterType<SkillResolutionProvider>().As<ISkillResolutionProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().InstancePerLifetimeScope();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().InstancePerLifetimeScope();
			builder.RegisterType<DeleteAndResourceCalculateService>().As<IDeleteAndResourceCalculateService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
			builder.RegisterType<IntervalDataMedianCalculator>().As<IIntervalDataCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<RestrictionAggregator>().As<IRestrictionAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftFilterService>().As<IWorkShiftFilterService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRestrictionExtractor>().As<IScheduleRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<SuggestedShiftRestrictionExtractor>().As<ISuggestedShiftRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayEquator>().As<IScheduleDayEquator>().InstancePerLifetimeScope();

			builder.RegisterType<WorkShiftFinderResultHolder>().As<IWorkShiftFinderResultHolder>().InstancePerLifetimeScope();

			builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerDependency();
			builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
			builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().InstancePerLifetimeScope();

			builder.RegisterType<SchedulingStateHolderAllSkillExtractor>().InstancePerDependency();
			builder.RegisterType<DailySkillForecastAndScheduledValueCalculator>().As<IDailySkillForecastAndScheduledValueCalculator>().InstancePerDependency();

			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().InstancePerLifetimeScope();
			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().InstancePerLifetimeScope();
			builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixUserLockLocker>().As<IMatrixUserLockLocker>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixNotPermittedLocker>().As<IMatrixNotPermittedLocker>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().As<IScheduleMatrixValueCalculatorProFactory>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().As<IWorkShiftLegalStateDayIndexCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulePeriodListShiftCategoryBackToLegalStateService>().As<ISchedulePeriodListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();

			builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
			builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();

			builder.RegisterType<SchedulerGroupPagesProvider>().As<ISchedulerGroupPagesProvider>().InstancePerLifetimeScope();

			builder.RegisterType<GroupScheduleGroupPageDataProvider>().As<IGroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageCreator>().As<IGroupPageCreator>().SingleInstance();
			builder.RegisterType<GroupPageFactory>().As<IGroupPageFactory>().SingleInstance();
			builder.RegisterType<GroupCreator>().As<IGroupCreator>().SingleInstance();
			builder.RegisterType<SwapServiceNew>().As<ISwapServiceNew>();
			builder.RegisterType<GroupPersonBuilderForOptimization>().As<IGroupPersonBuilderForOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();
			builder.RegisterType<EditableShiftMapper>().As<IEditableShiftMapper>().InstancePerLifetimeScope();
			builder.RegisterType<MaxMovedDaysOverLimitValidator>().As<IMaxMovedDaysOverLimitValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().As<ITeamBlockRestrictionOverLimitValidator>();
			builder.RegisterType<TeamBlockOptimizationLimits>().As<ITeamBlockOptimizationLimits>();

			builder.RegisterType<TeamBlockRemoveShiftCategoryOnBestDateService>().As<ITeamBlockRemoveShiftCategoryOnBestDateService>();
			builder.RegisterType<TeamBlockRemoveShiftCategoryBackToLegalService>().As<ITeamBlockRemoveShiftCategoryBackToLegalService>();
			builder.RegisterType<ShiftCategoryWeekRemover>().As<IShiftCategoryWeekRemover>();
			builder.RegisterType<ShiftCategoryPeriodRemover>().As<IShiftCategoryPeriodRemover>();
			builder.RegisterType<ShiftCategoryLimitCounter>().As<IShiftCategoryLimitCounter>();

			builder.RegisterType<TeamBlockDayOffsInPeriodValidator>().As<TeamBlockDayOffsInPeriodValidator>().As<ITeamBlockDayOffsInPeriodValidator>().SingleInstance();

			//ITeamBlockRestrictionOverLimitValidator
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixDataListInSteadyState>().As<IMatrixDataListInSteadyState>().InstancePerLifetimeScope();
			builder.RegisterType<HasContractDayOffDefinition>().As<IHasContractDayOffDefinition>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayDataMapper>().As<IScheduleDayDataMapper>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixDataListCreator>().As<IMatrixDataListCreator>().InstancePerDependency();
			builder.RegisterType<MatrixDataWithToFewDaysOff>().As<IMatrixDataWithToFewDaysOff>().InstancePerDependency();
			builder.RegisterType<MissingDaysOffScheduler>().As<IMissingDaysOffScheduler>().InstancePerDependency();
			builder.RegisterType<TeamDayOffScheduler>().As<ITeamDayOffScheduler>().InstancePerDependency();
			builder.RegisterType<DaysOffSchedulingService>().As<IDaysOffSchedulingService>().InstancePerDependency();

			builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().SingleInstance();
			builder.RegisterType<TrueFalseRandomizer>().As<ITrueFalseRandomizer>().SingleInstance();
			builder.RegisterType<OfficialWeekendDays>().As<IOfficialWeekendDays>().SingleInstance();
			builder.RegisterType<CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker>().As<IDayOffDecisionMaker>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingOptionsCreator>().As<ISchedulingOptionsCreator>().SingleInstance();
			builder.RegisterType<LockableBitArrayChangesTracker>().As<ILockableBitArrayChangesTracker>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleMatrixOriginalStateContainerCreator>().InstancePerLifetimeScope();

			builder.RegisterType<DaysOffLegalStateValidatorsFactory>().As<IDaysOffLegalStateValidatorsFactory>().InstancePerLifetimeScope();
			//IDaysOffLegalStateValidatorsFactory

			registerWorkShiftFilters(builder);
			registerWorkShiftSelector(builder);
			registerTeamBlockCommon(builder);
			registerTeamBlockDayOffOptimizerService(builder);
			registerTeamBlockIntradayOptimizerService(builder);
			registerTeamBlockSchedulingService(builder);
			registerMaxSeatSkillCreator(builder);

			builder.RegisterType<AgentRestrictionsNoWorkShiftfFinder>().As<IAgentRestrictionsNoWorkShiftfFinder>().InstancePerLifetimeScope();
			registerFairnessOptimizationService(builder);
			registerDayOffFairnessOptimizationService(builder);
			registerMoveTimeOptimizationClasses(builder);

			builder.RegisterType<AnalyzePersonAccordingToAvailability>().As<IAnalyzePersonAccordingToAvailability>().InstancePerLifetimeScope();
			builder.RegisterType<AdjustOvertimeLengthBasedOnAvailability>().InstancePerLifetimeScope();
			builder.RegisterType<OvertimeSkillIntervalDataAggregator>().As<IOvertimeSkillIntervalDataAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<OvertimePeriodValueMapper>().InstancePerLifetimeScope();
			builder.RegisterType<MergeOvertimeSkillIntervalData>().As<IMergeOvertimeSkillIntervalData>().InstancePerLifetimeScope();
			
			builder.RegisterType<OvertimeLengthDecider>().As<IOvertimeLengthDecider>();
			builder.RegisterType<OvertimeSkillIntervalData>().As<IOvertimeSkillIntervalData>();
			builder.RegisterType<OvertimeSkillIntervalDataDivider>().As<IOvertimeSkillIntervalDataDivider>().InstancePerLifetimeScope();
			builder.RegisterType<OvertimeSkillStaffPeriodToSkillIntervalDataMapper>().As<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>().InstancePerLifetimeScope();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>().InstancePerLifetimeScope();
			builder.RegisterType<NightlyRestRule>().As<IAssignmentPeriodRule>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleMatrixLockableBitArrayConverterEx>().As<IScheduleMatrixLockableBitArrayConverterEx>().SingleInstance();
			builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().SingleInstance();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>().SingleInstance();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>().SingleInstance();
			builder.RegisterType<MinWeekWorkTimeRule>().As<IMinWeekWorkTimeRule>();
			builder.RegisterType<DailyValueByAllSkillsExtractor>().As<IDailyValueByAllSkillsExtractor>();

			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();

			builder.RegisterType<ScheduleOptimization>().InstancePerLifetimeScope().ApplyAspects(); //should be singleinstance but not yet possible
			builder.RegisterType<FullScheduling>().InstancePerLifetimeScope().ApplyAspects(); //should be singleinstance but not yet possible
			builder.RegisterType<IntradayOptimizationEventHandler>().InstancePerLifetimeScope().ApplyAspects(); //should be singleinstance but not yet possible
			builder.RegisterType<OptimizationResult>().InstancePerLifetimeScope().ApplyAspects();
			builder.RegisterType<IntradayDecisionMaker>().As<IIntradayDecisionMaker>().SingleInstance();
			builder.RegisterType<FillSchedulerStateHolderFromDatabase>().As<IFillSchedulerStateHolder>().SingleInstance();
			builder.RegisterType<FixedStaffLoader>().As<IFixedStaffLoader>().SingleInstance();
			builder.RegisterType<OptimizationPreferencesFactory>().SingleInstance();
			builder.RegisterType<FetchDayOffRulesModel>().As<IFetchDayOffRulesModel>().SingleInstance();
			builder.RegisterType<DayOffRulesMapper>().SingleInstance();
			builder.RegisterType<FilterMapper>().SingleInstance();
			builder.RegisterType<DayOffRulesModelPersister>().As<IDayOffRulesModelPersister>().SingleInstance();
			builder.RegisterType<DayOffOptimizationPreferenceProviderUsingFiltersFactory>().AsSelf().SingleInstance();
			builder.RegisterType<FindFilter>().SingleInstance();
			builder.RegisterType<ViolatedSchedulePeriodBusinessRule>().SingleInstance();
			builder.RegisterType<DayOffBusinessRuleValidation>().SingleInstance();
			if (_configuration.Toggle(Toggles.ResourcePlanner_SkillGroupDeleteAfterCalculation_37048))
			{
				builder.RegisterType<ResourceCalculateAfterDeleteDecider>().As<IResourceCalculateAfterDeleteDecider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AlwaysResourceCalculateAfterDelete>().As<IResourceCalculateAfterDeleteDecider>().SingleInstance();
			}
			builder.RegisterType<LimitForNoResourceCalculation>().As<ILimitForNoResourceCalculation>().AsSelf().SingleInstance();
			builder.RegisterType<NoSchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			if (_configuration.Toggle(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049))
			{
				builder.RegisterType<IntradayOptimizerContainerConsiderLargeGroups>().As<IIntradayOptimizerContainer>().SingleInstance();
			}
			else
			{
				builder.RegisterType<IntradayOptimizerContainer>().As<IIntradayOptimizerContainer>().SingleInstance();
			}
			builder.RegisterType<AgentsToSkillGroups>().SingleInstance();
			builder.RegisterType<IntradayOptmizerLimiter>().As<IIntradayOptimizerLimiter>().AsSelf().SingleInstance();
			builder.RegisterType<IntradayOptimizeOnDayCallBackDoNothing>().As<IIntradayOptimizeOneDayCallback>().SingleInstance();
			builder.RegisterType<IntradayOptimizationCallbackContext>().SingleInstance();
			builder.RegisterType<ResourceCalculationContextFactory>().InstancePerLifetimeScope();
			if (_configuration.Toggle(Toggles.ResourcePlanner_IntradayIslands_36939))
			{
				builder.RegisterType<IntradayOptimizationCommandHandler>().As<IIntradayOptimizationCommandHandler>().SingleInstance();
			}
			else
			{
				builder.RegisterType<IntradayOptimizationOneThreadCommandHandler>().As<IIntradayOptimizationCommandHandler>().SingleInstance();
			}
			builder.RegisterType<IntradayOptimizationFromWeb>().InstancePerLifetimeScope().ApplyAspects();
		}

		private static void registerMoveTimeOptimizationClasses(ContainerBuilder builder)
		{
			builder.RegisterType<ValidateFoundMovedDaysSpecification>().As<IValidateFoundMovedDaysSpecification>().InstancePerLifetimeScope();
			builder.RegisterType<DayValueUnlockedIndexSorter>().As<IDayValueUnlockedIndexSorter>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockMoveTimeDescisionMaker>().As<ITeamBlockMoveTimeDescisionMaker>();

			builder.RegisterType<TeamBlockMoveTimeBetweenDaysService>().As<ITeamBlockMoveTimeBetweenDaysService>();
			builder.RegisterType<TeamBlockMoveTimeOptimizer>().As<ITeamBlockMoveTimeOptimizer>();
			builder.RegisterType<LockUnSelectedInTeamBlock>().As<ILockUnSelectedInTeamBlock>().InstancePerLifetimeScope();
		}

		private static void registerDayOffFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockDayOffFairnessOptimizationServiceFacade>().As<ITeamBlockDayOffFairnessOptimizationServiceFacade>();
			builder.RegisterType<WeekDayPoints>().As<IWeekDayPoints>();
			builder.RegisterType<DayOffStep1>().As<IDayOffStep1>();
			builder.RegisterType<DayOffStep2>().As<IDayOffStep2>();
			builder.RegisterType<SeniorTeamBlockLocator>().As<ISeniorTeamBlockLocator>().SingleInstance();
			builder.RegisterType<DescisionMakerToSwapTeamBlock>().As<IDescisionMakerToSwapTeamBlock>();
			builder.RegisterType<SeniorityCalculatorForTeamBlock>().As<ISeniorityCalculatorForTeamBlock>();
			builder.RegisterType<TeamBlockLocatorWithHighestPoints>().As<ITeamBlockLocatorWithHighestPoints>().SingleInstance();
			builder.RegisterType<WeekDayPointCalculatorForTeamBlock>().As<IWeekDayPointCalculatorForTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockDayOffSwapper>().As<ITeamBlockDayOffSwapper>();
			builder.RegisterType<TeamBlockDayOffDaySwapper>().As<ITeamBlockDayOffDaySwapper>();
			builder.RegisterType<JuniorTeamBlockExtractor>().As<IJuniorTeamBlockExtractor>().SingleInstance();
			builder.RegisterType<SuitableDayOffSpotDetector>().As<ISuitableDayOffSpotDetector>().SingleInstance();
			builder.RegisterType<TeamBlockDayOffDaySwapDecisionMaker>().As<ITeamBlockDayOffDaySwapDecisionMaker>();
			builder.RegisterType<RankedPersonBasedOnStartDate>().As<IRankedPersonBasedOnStartDate>().SingleInstance();
			builder.RegisterType<PersonStartDateFromPersonPeriod>().As<IPersonStartDateFromPersonPeriod>().SingleInstance();
			builder.RegisterType<SuitableDayOffsToGiveAway>().As<ISuitableDayOffsToGiveAway>().SingleInstance();
			builder.RegisterType<PostSwapValidationForTeamBlock>().As<IPostSwapValidationForTeamBlock>();
			builder.RegisterType<PersonDayOffPointsCalculator>().As<IPersonDayOffPointsCalculator>();
			builder.RegisterType<PersonShiftCategoryPointCalculator>().As<IPersonShiftCategoryPointCalculator>().InstancePerLifetimeScope();
		}

		private static void registerFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockSeniorityValidator>().As<ITeamBlockSeniorityValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSeniorityFairnessOptimizationService>().As<ITeamBlockSeniorityFairnessOptimizationService>();
			builder.RegisterType<ConstructTeamBlock>().As<IConstructTeamBlock>();
			builder.RegisterType<ShiftCategoryPoints>().As<IShiftCategoryPoints>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryPointExtractor>().As<IShiftCategoryPointExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<SeniorityExtractor>().As<ISeniorityExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<DetermineTeamBlockPriority>().As<IDetermineTeamBlockPriority>();
			builder.RegisterType<TeamBlockSwapValidator>().As<ITeamBlockSwapValidator>();
			builder.RegisterType<TeamBlockSwapDayValidator>().As<ITeamBlockSwapDayValidator>();
			builder.RegisterType<TeamBlockSwap>().As<ITeamBlockSwap>();
			builder.RegisterType<TeamBlockLockValidator>().As<ITeamBlockLockValidator>();
			builder.RegisterType<SeniorityTeamBlockSwapValidator>().As<ISeniorityTeamBlockSwapValidator>();
			builder.RegisterType<DayOffRulesValidator>().As<IDayOffRulesValidator>().InstancePerLifetimeScope();
			builder.RegisterType<SeniorityTeamBlockSwapper>().As<ISeniorityTeamBlockSwapper>();

			//ITeamBlockSameTimeZoneValidator

			//common
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamMemberCountValidator>().As<ITeamMemberCountValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockContractTimeValidator>().As<ITeamBlockContractTimeValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSameSkillValidator>().As<ITeamBlockSameSkillValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockPersonsSkillChecker>().As<ITeamBlockPersonsSkillChecker>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSameRuleSetBagValidator>().As<ITeamBlockSameRuleSetBagValidator>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSameTimeZoneValidator>().As<ITeamBlockSameTimeZoneValidator>().InstancePerLifetimeScope();
		}

		private static void registerTeamBlockCommon(ContainerBuilder builder)
		{
			builder.RegisterType<LocateMissingIntervalsIfMidNightBreak>().As<ILocateMissingIntervalsIfMidNightBreak>();
			builder.RegisterType<FilterOutIntervalsAfterMidNight>().As<IFilterOutIntervalsAfterMidNight>().SingleInstance();
			builder.RegisterType<GroupPersonSkillAggregator>().As<IGroupPersonSkillAggregator>().SingleInstance();
			builder.RegisterType<DynamicBlockFinder>().As<IDynamicBlockFinder>().SingleInstance();
			builder.RegisterType<TeamBlockInfoFactory>().As<ITeamBlockInfoFactory>();
			builder.RegisterType<TeamInfoFactory>().As<ITeamInfoFactory>();
			builder.RegisterType<SafeRollbackAndResourceCalculation>().As<ISafeRollbackAndResourceCalculation>();
			builder.RegisterType<TeamBlockClearer>().As<ITeamBlockClearer>();
			builder.RegisterType<TeamBlockSteadyStateValidator>().As<ITeamBlockSteadyStateValidator>();
			builder.RegisterType<RestrictionOverLimitDecider>().As<IRestrictionOverLimitDecider>();
			builder.RegisterType<RestrictionChecker>().As<ICheckerRestriction>();

			builder.RegisterType<TeamBlockMaxSeatChecker>().As<ITeamBlockMaxSeatChecker>();
			builder.RegisterType<DailyTargetValueCalculatorForTeamBlock>().As<IDailyTargetValueCalculatorForTeamBlock>();
			builder.RegisterType<NightlyRestRestrictionForTeamBlock>().As<INightlyRestRestrictionForTeamBlock>();
			builder.RegisterType<MedianCalculatorForDays>().As<IMedianCalculatorForDays>().InstancePerLifetimeScope();
			builder.RegisterType<TwoDaysIntervalGenerator>().As<ITwoDaysIntervalGenerator>().InstancePerLifetimeScope();
			builder.RegisterType<MedianCalculatorForSkillInterval>().As<IMedianCalculatorForSkillInterval>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataOpenHour>().As<ISkillIntervalDataOpenHour>();
			builder.RegisterType<SameOpenHoursInTeamBlockSpecification>().As<ISameOpenHoursInTeamBlockSpecification>();
			builder.RegisterType<SameEndTimeTeamSpecification>().As<ISameEndTimeTeamSpecification>();
			builder.RegisterType<SameShiftCategoryBlockSpecification>().As<ISameShiftCategoryBlockSpecification>();
			builder.RegisterType<SameShiftCategoryTeamSpecification>().As<ISameShiftCategoryTeamSpecification>();
			builder.RegisterType<SameStartTimeBlockSpecification>().As<ISameStartTimeBlockSpecification>();
			builder.RegisterType<SameStartTimeTeamSpecification>().As<ISameStartTimeTeamSpecification>();
			builder.RegisterType<SameShiftBlockSpecification>().As<ISameShiftBlockSpecification>().InstancePerLifetimeScope();
			builder.RegisterType<ValidSampleDayPickerFromTeamBlock>().As<IValidSampleDayPickerFromTeamBlock>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockSchedulingOptions>().As<ITeamBlockSchedulingOptions>();
			builder.RegisterType<TeamBlockRoleModelSelector>().As<ITeamBlockRoleModelSelector>();
			builder.RegisterType<TeamBlockSchedulingCompletionChecker>().As<ITeamBlockSchedulingCompletionChecker>();
			builder.RegisterType<ProposedRestrictionAggregator>().As<IProposedRestrictionAggregator>();
			builder.RegisterType<TeamBlockRestrictionAggregator>().As<ITeamBlockRestrictionAggregator>();
			builder.RegisterType<TeamRestrictionAggregator>().As<ITeamRestrictionAggregator>();
			builder.RegisterType<BlockRestrictionAggregator>().As<IBlockRestrictionAggregator>();
			builder.RegisterType<TeamBlockMissingDaysOffScheduler>().As<ITeamBlockMissingDaysOffScheduler>();
			builder.RegisterType<TeamMatrixChecker>().As<ITeamMatrixChecker>().InstancePerLifetimeScope();
			//ITeamMatrixChecker

			builder.RegisterType<TeamMemberTerminationOnBlockSpecification>().As<ITeamMemberTerminationOnBlockSpecification>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockMissingDayOffHandler>().As<ITeamBlockMissingDayOffHandler>();
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().InstancePerLifetimeScope();
			builder.RegisterType<SplitSchedulePeriodToWeekPeriod>().As<ISplitSchedulePeriodToWeekPeriod>().InstancePerLifetimeScope();
			builder.RegisterType<TeamScheduling>().As<ITeamScheduling>();
			builder.RegisterType<TeamBlockSingleDayScheduler>().As<ITeamBlockSingleDayScheduler>();
			builder.RegisterType<TeamBlockScheduler>().As<ITeamBlockScheduler>();
			builder.RegisterType<TeamBlockGenerator>().As<ITeamBlockGenerator>();
			builder.RegisterType<MissingDayOffBestSpotDecider>().As<IMissingDayOffBestSpotDecider>();
		}

		private static void registerTeamBlockSchedulingService(ContainerBuilder builder)
		{

			builder.RegisterType<CreateSkillIntervalDatasPerActivtyForDate>().As<ICreateSkillIntervalDatasPerActivtyForDate>();
			builder.RegisterType<CalculateAggregatedDataForActivtyAndDate>().As<ICalculateAggregatedDataForActivtyAndDate>();
			builder.RegisterType<CreateSkillIntervalDataPerDateAndActivity>().As<ICreateSkillIntervalDataPerDateAndActivity>();
			builder.RegisterType<OpenHourForDate>().As<IOpenHourForDate>().InstancePerLifetimeScope();
			builder.RegisterType<ActivityIntervalDataCreator>().As<IActivityIntervalDataCreator>();
			builder.RegisterType<WorkShiftFromEditableShift>().As<IWorkShiftFromEditableShift>().InstancePerLifetimeScope();
			builder.RegisterType<FirstShiftInTeamBlockFinder>().As<IFirstShiftInTeamBlockFinder>();
			builder.RegisterType<TeamBlockOpenHoursValidator>().As<ITeamBlockOpenHoursValidator>();
			//IFirstShiftInTeamBlockFinder
		}

		private static void registerTeamBlockIntradayOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockIntradayDecisionMaker>().As<ITeamBlockIntradayDecisionMaker>().InstancePerLifetimeScope();
			builder.RegisterType<RelativeDailyValueCalculatorForTeamBlock>().As<IRelativeDailyValueCalculatorForTeamBlock>().SingleInstance();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeamDayOffModifier>().As<ITeamDayOffModifier>();
		}

		private static void registerWorkShiftSelector(ContainerBuilder builder)
		{
			builder.RegisterType<WorkShiftLengthValueCalculator>().As<IWorkShiftLengthValueCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftValueCalculator>().As<IWorkShiftValueCalculator>();
			builder.RegisterType<EqualWorkShiftValueDecider>().As<IEqualWorkShiftValueDecider>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftSelector>().As<IWorkShiftSelector>();
			builder.RegisterType<VisualLayerToBaseDateMapper>().As<IVisualLayerToBaseDateMapper>().InstancePerLifetimeScope();
			builder.RegisterType<MaxSeatsCalculationForTeamBlock>().As<IMaxSeatsCalculationForTeamBlock>().InstancePerLifetimeScope();
			builder.RegisterType<MaxSeatInformationGeneratorBasedOnIntervals>().As<IMaxSeatInformationGeneratorBasedOnIntervals>();
			builder.RegisterType<MaxSeatSkillAggregator>().As<IMaxSeatSkillAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<MaxSeatInformationGeneratorBasedOnIntervals>().As<IMaxSeatInformationGeneratorBasedOnIntervals>();
			builder.RegisterType<MaxSeatsSpecificationDictionaryExtractor>().As<IMaxSeatsSpecificationDictionaryExtractor>();
			builder.RegisterType<IsMaxSeatsReachedOnSkillStaffPeriodSpecification>().As<IIsMaxSeatsReachedOnSkillStaffPeriodSpecification>().InstancePerLifetimeScope();
			builder.RegisterType<IntervalLevelMaxSeatInfo>();
			builder.RegisterType<MaxSeatBoostingFactorCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<PullTargetValueFromSkillIntervalData>().InstancePerLifetimeScope();
			builder.RegisterType<ExtractIntervalsViolatingMaxSeat>().As<IExtractIntervalsViolatingMaxSeat>();
		}

		private static void registerMaxSeatSkillCreator(ContainerBuilder builder)
		{
			builder.RegisterType<WorkloadDayHelper>().As<IWorkloadDayHelper>().SingleInstance();
			builder.RegisterType<SchedulerSkillDayHelper>().As<ISchedulerSkillDayHelper>().SingleInstance();
			builder.RegisterType<MaxSeatSitesExtractor>().As<IMaxSeatSitesExtractor>().SingleInstance();
			builder.RegisterType<CreateSkillsFromMaxSeatSites>().As<ICreateSkillsFromMaxSeatSites>().SingleInstance();
			builder.RegisterType<SchedulerSkillDayHelper>().As<ISchedulerSkillDayHelper>().SingleInstance();
			builder.RegisterType<CreatePersonalSkillsFromMaxSeatSites>().As<ICreatePersonalSkillsFromMaxSeatSites>().SingleInstance();
			builder.RegisterType<MaxSeatSkillCreator>().As<MaxSeatSkillCreator>().SingleInstance();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private static void registerWorkShiftFilters(ContainerBuilder builder)
		{
			builder.RegisterType<ActivityRestrictionsShiftFilter>().As<IActivityRestrictionsShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<BusinessRulesShiftFilter>().As<IBusinessRulesShiftFilter>();
			builder.RegisterType<CommonMainShiftFilter>().As<ICommonMainShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<ContractTimeShiftFilter>().As<IContractTimeShiftFilter>();
			builder.RegisterType<DisallowedShiftCategoriesShiftFilter>().As<IDisallowedShiftCategoriesShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<EarliestEndTimeLimitationShiftFilter>().As<IEarliestEndTimeLimitationShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<EffectiveRestrictionShiftFilter>().As<IEffectiveRestrictionShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<LatestStartTimeLimitationShiftFilter>().As<ILatestStartTimeLimitationShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<MainShiftOptimizeActivitiesSpecificationShiftFilter>().As<IMainShiftOptimizeActivitiesSpecificationShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<NotOverWritableActivitiesShiftFilter>().As<INotOverWritableActivitiesShiftFilter>();
			builder.RegisterType<PersonalShiftsShiftFilter>().As<IPersonalShiftsShiftFilter>();
			builder.RegisterType<RuleSetPersonalSkillsActivityFilter>().As<IRuleSetPersonalSkillsActivityFilter>().InstancePerLifetimeScope();
			builder.RegisterType<ActivityRequiresSkillProjectionFilter>().As<IActivityRequiresSkillProjectionFilter>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryRestrictionShiftFilter>().As<IShiftCategoryRestrictionShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<ValidDateTimePeriodShiftFilter>().As<IValidDateTimePeriodShiftFilter>();
			builder.RegisterType<TimeLimitsRestrictionShiftFilter>().As<ITimeLimitsRestrictionShiftFilter>();
			builder.RegisterType<WorkTimeLimitationShiftFilter>().As<IWorkTimeLimitationShiftFilter>().InstancePerLifetimeScope();
			builder.RegisterType<CommonActivityFilter>().As<ICommonActivityFilter>().InstancePerLifetimeScope();
			builder.RegisterType<RuleSetAccordingToAccessabilityFilter>().As<IRuleSetAccordingToAccessabilityFilter>();
			builder.RegisterType<TeamBlockRuleSetBagExtractor>().As<ITeamBlockRuleSetBagExtractor>().SingleInstance();
			builder.RegisterType<TeamBlockIncludedWorkShiftRuleFilter>().As<ITeamBlockIncludedWorkShiftRuleFilter>().SingleInstance();
			builder.RegisterType<RuleSetSkillActivityChecker>().As<IRuleSetSkillActivityChecker>().SingleInstance();
			builder.RegisterType<PersonalShiftAndMeetingFilter>().As<IPersonalShiftAndMeetingFilter>();
			builder.RegisterType<PersonalShiftMeetingTimeChecker>().As<IPersonalShiftMeetingTimeChecker>().SingleInstance();
			builder.RegisterType<DisallowedShiftProjectionCashesFilter>().As<IDisallowedShiftProjectionCashesFilter>().SingleInstance();

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
		 			
		}
	}
}