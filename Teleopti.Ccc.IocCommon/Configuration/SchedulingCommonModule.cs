using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
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
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SchedulingCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule<IntraIntervalOptimizationServiceModule>();
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
			builder.RegisterModule<BackToLegalShiftModule>();
			builder.RegisterModule<ScheduleOvertimeModule>();

			builder.RegisterType<ClassicScheduleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleCommand>().As<IScheduleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationCommand>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>().SingleInstance();
			builder.RegisterType<ScheduleOvertimeCommand>().As<IScheduleOvertimeCommand>();
			builder.RegisterType<TeamBlockMoveTimeBetweenDaysCommand>().As<ITeamBlockMoveTimeBetweenDaysCommand>();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>();
			builder.RegisterType<MatrixListFactory>().As<IMatrixListFactory>().InstancePerLifetimeScope();
			builder.RegisterType<TeamBlockScheduleCommand>().As<ITeamBlockScheduleCommand>();
			builder.RegisterType<TeamBlockOptimizationCommand>().As<ITeamBlockOptimizationCommand>();
			builder.RegisterType<WeeklyRestSolverCommand>().As<IWeeklyRestSolverCommand>();
			builder.RegisterType<BackToLegalShiftCommand>();
			builder.RegisterType<IntraIntervalOptimizationCommand>().As<IIntraIntervalOptimizationCommand>();

			builder.RegisterType<DisableDeletedFilter>().As<IDisableDeletedFilter>().SingleInstance();
			builder.RegisterType<InnerOptimizerHelperHelper>().As<IOptimizerHelperHelper>().SingleInstance();
			builder.RegisterType<ResourceOptimizationHelperExtended>().As<IResourceOptimizationHelperExtended>().InstancePerLifetimeScope();
			builder.RegisterType<RequiredScheduleHelper>().As<IRequiredScheduleHelper>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleCommandToggle>().As<IScheduleCommandToggle>().InstancePerLifetimeScope();
			builder.RegisterType<CommonStateHolder>().As<ICommonStateHolder>().InstancePerLifetimeScope();

			builder.RegisterModule<WeeklyRestSolverModule>();
			builder.RegisterModule<EqualNumberOfCategoryFairnessModule>();
			
			builder.RegisterType<SchedulerStateHolder>().As<ISchedulerStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<OverriddenBusinessRulesHolder>().As<IOverriddenBusinessRulesHolder>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<PreSchedulingStatusChecker>().As<IPreSchedulingStatusChecker>().InstancePerLifetimeScope();
			
			builder.RegisterType<SeatLimitationWorkShiftCalculator2>()
				.As<ISeatLimitationWorkShiftCalculator2>()
				.SingleInstance();
			builder.RegisterType<SeatImpactOnPeriodForProjection>()
				.As<ISeatImpactOnPeriodForProjection>()
				.SingleInstance();
			builder.RegisterType<LongestPeriodForAssignmentCalculator>()
				.As<ILongestPeriodForAssignmentCalculator>()
				.SingleInstance();
			builder.RegisterType<PersonSkillPeriodsDataHolderManager>()
				.As<IPersonSkillPeriodsDataHolderManager>()
				.InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessShiftValueCalculator>()
				.As<IShiftCategoryFairnessShiftValueCalculator>()
				.SingleInstance();
			builder.RegisterType<ShiftProjectionCacheManager>().As<IShiftProjectionCacheManager>().SingleInstance();
			builder.RegisterType<ShiftFromMasterActivityService>()
				.As<IShiftFromMasterActivityService>()
				.SingleInstance();
			builder.RegisterType<RuleSetDeletedActivityChecker>().As<IRuleSetDeletedActivityChecker>().SingleInstance();
			builder.RegisterType<RuleSetDeletedShiftCategoryChecker>()
				.As<IRuleSetDeletedShiftCategoryChecker>()
				.SingleInstance();
			builder.RegisterType<FairnessValueCalculator>().As<IFairnessValueCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftCalculatorsManager>().As<IWorkShiftCalculatorsManager>().SingleInstance();

			builder.RegisterType<FairnessAndMaxSeatCalculatorsManager28317>()
				.InstancePerLifetimeScope();
			builder.RegisterType<FairnessAndMaxSeatCalculatorsManager>()
				.InstancePerLifetimeScope();
			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317)
				? (IFairnessAndMaxSeatCalculatorsManager) c.Resolve<FairnessAndMaxSeatCalculatorsManager28317>()
				: c.Resolve<FairnessAndMaxSeatCalculatorsManager>())
				.As<IFairnessAndMaxSeatCalculatorsManager>();

			builder.RegisterType<SchedulerStateScheduleDayChangedCallback>()
				.As<IScheduleDayChangeCallback>()
				.InstancePerLifetimeScope();
			builder.RegisterType<ResourceCalculateDaysDecider>().As<IResourceCalculateDaysDecider>().SingleInstance();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationPreferences>().As<IOptimizationPreferences>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffPlannerRules>().As<IDayOffPlannerRules>().InstancePerLifetimeScope();
			builder.RegisterType<NonBlendWorkShiftCalculator>().As<INonBlendWorkShiftCalculator>().SingleInstance();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>()
				.As<INonBlendSkillImpactOnPeriodForProjection>()
				.SingleInstance();
			builder.RegisterType<ShiftCategoryFairnessManager>().As<IShiftCategoryFairnessManager>().InstancePerLifetimeScope();
			builder.RegisterType<GroupShiftCategoryFairnessCreator>()
				.As<IGroupShiftCategoryFairnessCreator>()
				.InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessCalculator>()
				.As<IShiftCategoryFairnessCalculator>()
				.SingleInstance();
			builder.RegisterType<GroupPagePerDateHolder>().As<IGroupPagePerDateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<DesiredShiftLengthCalculator>().As<IDesiredShiftLengthCalculator>().SingleInstance();
			builder.RegisterType<ShiftLengthDecider>().As<IShiftLengthDecider>().SingleInstance();
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
			builder.RegisterType<ScheduleMatrixListCreator>().As<IScheduleMatrixListCreator>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryLimitationChecker>().As<IShiftCategoryLimitationChecker>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulePartModifyAndRollbackService>().As<ISchedulePartModifyAndRollbackService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduler>().As<IAbsencePreferenceScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceFullDayLayerCreator>().As<IAbsencePreferenceFullDayLayerCreator>().SingleInstance();
			builder.RegisterType<DayOffScheduler>().As<IDayOffScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayAvailableForDayOffSpecification>().As<IScheduleDayAvailableForDayOffSpecification>().SingleInstance();

			builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().SingleInstance();
			builder.RegisterInstance(new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().SingleInstance();
			builder.RegisterType<FixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

			builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();

			//OptimizationCommand


			builder.RegisterType<AdvanceDaysOffSchedulingService>().As<IAdvanceDaysOffSchedulingService>();
			builder.RegisterType<SkillResolutionProvider>().As<ISkillResolutionProvider>().SingleInstance();
			builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().SingleInstance();
			builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().SingleInstance();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().SingleInstance();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().InstancePerLifetimeScope();
			builder.RegisterType<DeleteAndResourceCalculateService>().As<IDeleteAndResourceCalculateService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
			builder.RegisterType<IntervalDataMedianCalculator>().As<IIntervalDataCalculator>().SingleInstance();
			builder.RegisterType<RestrictionAggregator>().As<IRestrictionAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftFilterService>().As<IWorkShiftFilterService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRestrictionExtractor>().As<IScheduleRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<SuggestedShiftRestrictionExtractor>().As<ISuggestedShiftRestrictionExtractor>().SingleInstance();
			builder.RegisterType<ScheduleDayEquator>().As<IScheduleDayEquator>().SingleInstance();

			builder.RegisterType<WorkShiftFinderResultHolder>().As<IWorkShiftFinderResultHolder>().InstancePerLifetimeScope();

			builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().SingleInstance();
			builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerDependency();
			builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
			builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().SingleInstance();

			builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().SingleInstance();
			builder.RegisterType<SchedulingStateHolderAllSkillExtractor>().InstancePerDependency();
			builder.RegisterType<DailySkillForecastAndScheduledValueCalculator>().As<IDailySkillForecastAndScheduledValueCalculator>().InstancePerDependency();

			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().SingleInstance();
			builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().SingleInstance();
			builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixUserLockLocker>().As<IMatrixUserLockLocker>().InstancePerLifetimeScope();
			builder.RegisterType<MatrixNotPermittedLocker>().As<IMatrixNotPermittedLocker>().SingleInstance();
			builder.RegisterType<ScheduleFairnessCalculator>().As<IScheduleFairnessCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().As<IScheduleMatrixValueCalculatorProFactory>().SingleInstance();
			builder.RegisterType<SchedulePeriodListShiftCategoryBackToLegalStateService>().As<ISchedulePeriodListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().As<IWorkShiftLegalStateDayIndexCalculator>().SingleInstance();

			builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().SingleInstance();
			builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
			builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();

			builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().SingleInstance();

			builder.RegisterType<SchedulerGroupPagesProvider>().As<ISchedulerGroupPagesProvider>().InstancePerLifetimeScope();

			builder.RegisterType<GroupScheduleGroupPageDataProvider>().As<IGroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageCreator>().As<IGroupPageCreator>().SingleInstance();
			builder.RegisterType<GroupPageFactory>().As<IGroupPageFactory>().SingleInstance();
			builder.RegisterType<GroupCreator>().As<IGroupCreator>().SingleInstance();
			builder.RegisterType<SwapServiceNew>().As<ISwapServiceNew>();
			builder.RegisterType<GroupPersonBuilderForOptimization>().As<IGroupPersonBuilderForOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();
			builder.RegisterType<EditableShiftMapper>().As<IEditableShiftMapper>().SingleInstance();
			builder.RegisterType<MaxMovedDaysOverLimitValidator>().As<IMaxMovedDaysOverLimitValidator>().SingleInstance();
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().As<ITeamBlockRestrictionOverLimitValidator>();
			builder.RegisterType<TeamBlockOptimizationLimits>().As<ITeamBlockOptimizationLimits>();

			//ITeamBlockRestrictionOverLimitValidator
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().SingleInstance();
			builder.RegisterType<MatrixDataListInSteadyState>().As<IMatrixDataListInSteadyState>().SingleInstance();
			builder.RegisterType<HasContractDayOffDefinition>().As<IHasContractDayOffDefinition>().SingleInstance();
			builder.RegisterType<ScheduleDayDataMapper>().As<IScheduleDayDataMapper>().SingleInstance();
			builder.RegisterType<MatrixDataListCreator>().As<IMatrixDataListCreator>().InstancePerDependency();
			builder.RegisterType<UniqueSchedulePartExtractor>().As<IUniqueSchedulePartExtractor>().SingleInstance();
			builder.RegisterType<MatrixDataWithToFewDaysOff>().As<IMatrixDataWithToFewDaysOff>().InstancePerDependency();
			builder.RegisterType<MissingDaysOffScheduler>().As<IMissingDaysOffScheduler>().InstancePerDependency();
			builder.RegisterType<TeamDayOffScheduler>().As<ITeamDayOffScheduler>().InstancePerDependency();
			builder.RegisterType<DaysOffSchedulingService>().As<IDaysOffSchedulingService>().InstancePerDependency();

			builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().SingleInstance();
			builder.RegisterType<TrueFalseRandomizer>().As<ITrueFalseRandomizer>().SingleInstance();
			builder.RegisterType<OfficialWeekendDays>().As<IOfficialWeekendDays>().SingleInstance();
			builder.RegisterType<CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker>().As<IDayOffDecisionMaker>().SingleInstance();
			builder.RegisterType<SchedulingOptionsCreator>().As<ISchedulingOptionsCreator>().SingleInstance();
			builder.RegisterType<LockableBitArrayChangesTracker>().As<ILockableBitArrayChangesTracker>().SingleInstance();

			builder.RegisterType<DaysOffLegalStateValidatorsFactory>().As<IDaysOffLegalStateValidatorsFactory>().SingleInstance();
			//IDaysOffLegalStateValidatorsFactory

			registerWorkShiftFilters(builder);
			registerWorkShiftSelector(builder);
			registerTeamBlockCommon(builder);
			registerTeamBlockDayOffOptimizerService(builder);
			registerTeamBlockIntradayOptimizerService(builder);
			registerTeamBlockSchedulingService(builder);

			builder.RegisterType<AgentRestrictionsNoWorkShiftfFinder>().As<IAgentRestrictionsNoWorkShiftfFinder>().InstancePerLifetimeScope();
			registerFairnessOptimizationService(builder);
			registerDayOffFairnessOptimizationService(builder);
			registerMoveTimeOptimizationClasses(builder);

			builder.RegisterType<AnalyzePersonAccordingToAvailability>().As<IAnalyzePersonAccordingToAvailability>().SingleInstance();
			builder.RegisterType<AdjustOvertimeLengthBasedOnAvailability>().SingleInstance();
			builder.RegisterType<OvertimeSkillIntervalDataAggregator>().As<IOvertimeSkillIntervalDataAggregator>().SingleInstance();
			builder.RegisterType<OvertimePeriodValueMapper>().SingleInstance();
			builder.RegisterType<MergeOvertimeSkillIntervalData>().As<IMergeOvertimeSkillIntervalData>().SingleInstance();
			
			builder.RegisterType<OvertimeLengthDecider>().As<IOvertimeLengthDecider>();
			builder.RegisterType<OvertimeSkillIntervalData>().As<IOvertimeSkillIntervalData>();
			builder.RegisterType<OvertimeSkillIntervalDataDivider>().As<IOvertimeSkillIntervalDataDivider>().SingleInstance();
			builder.RegisterType<OvertimeSkillStaffPeriodToSkillIntervalDataMapper>().As<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>().SingleInstance();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>().SingleInstance();
			builder.RegisterType<NightlyRestRule>().As<IAssignmentPeriodRule>().SingleInstance();
			builder.RegisterType<ScheduleMatrixLockableBitArrayConverterEx>().As<IScheduleMatrixLockableBitArrayConverterEx>().SingleInstance();
			builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().SingleInstance();
			builder.RegisterType<RestrictionCombiner>().As<IRestrictionCombiner>().SingleInstance();
			builder.RegisterType<RestrictionRetrievalOperation>().As<IRestrictionRetrievalOperation>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionCreator30393>().As<EffectiveRestrictionCreator30393>();
			builder.RegisterType<EffectiveRestrictionCreator>().As<EffectiveRestrictionCreator>();
			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_SudentAvailabilityForFixedStaff_30393)
				? (IEffectiveRestrictionCreator) c.Resolve<EffectiveRestrictionCreator30393>()
				: c.Resolve<EffectiveRestrictionCreator>())
				.As<IEffectiveRestrictionCreator>();

			builder.RegisterType<MinWeekWorkTimeRule>().As<MinWeekWorkTimeRule>();
			builder.RegisterType<MinWeekWorkTimeRuleToggle31992Off>().As<MinWeekWorkTimeRuleToggle31992Off>();
			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_DoNotBreakMinWeekWorkTimeOptimization_31992)
				? (IMinWeekWorkTimeRule)c.Resolve<MinWeekWorkTimeRule>()
				: c.Resolve<MinWeekWorkTimeRuleToggle31992Off>())
				.As<IMinWeekWorkTimeRule>();

			builder.RegisterType<DailyValueByAllSkillsExtractor>().As<IDailyValueByAllSkillsExtractor>();
			//IDailyValueByAllSkillsExtractor
		}

		private void registerMoveTimeOptimizationClasses(ContainerBuilder builder)
		{
			builder.RegisterType<ValidateFoundMovedDaysSpecification>().As<IValidateFoundMovedDaysSpecification>().SingleInstance();
			builder.RegisterType<DayValueUnlockedIndexSorter>().As<IDayValueUnlockedIndexSorter>().SingleInstance();
			builder.RegisterType<TeamBlockMoveTimeDescisionMaker>().As<ITeamBlockMoveTimeDescisionMaker>();

			builder.RegisterType<TeamBlockMoveTimeBetweenDaysService>().As<ITeamBlockMoveTimeBetweenDaysService>();
			builder.RegisterType<TeamBlockMoveTimeOptimizer>().As<ITeamBlockMoveTimeOptimizer>();
			builder.RegisterType<LockUnSelectedInTeamBlock>().As<ILockUnSelectedInTeamBlock>().SingleInstance();
		}

		private void registerDayOffFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockDayOffFairnessOptimizationServiceFacade>();
			builder.RegisterType<TeamBlockDayOffFairnessOptimizationServiceFacadeSeniorityTurnedOff>()
				.As<TeamBlockDayOffFairnessOptimizationServiceFacadeSeniorityTurnedOff>();
			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_Seniority_24331)
				? (ITeamBlockDayOffFairnessOptimizationServiceFacade) c.Resolve<TeamBlockDayOffFairnessOptimizationServiceFacade>()
				: c.Resolve<TeamBlockDayOffFairnessOptimizationServiceFacadeSeniorityTurnedOff>())
				.As<ITeamBlockDayOffFairnessOptimizationServiceFacade>();

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
			builder.RegisterType<PersonShiftCategoryPointCalculator>().As<IPersonShiftCategoryPointCalculator>().SingleInstance();
		}

		private void registerFairnessOptimizationService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockSeniorityValidator>().As<ITeamBlockSeniorityValidator>().SingleInstance();
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>().SingleInstance();
			builder.RegisterType<TeamBlockSeniorityFairnessOptimizationService>().As<ITeamBlockSeniorityFairnessOptimizationService>();
			builder.RegisterType<ConstructTeamBlock>().As<IConstructTeamBlock>();
			builder.RegisterType<ShiftCategoryPoints>().As<IShiftCategoryPoints>().SingleInstance();
			builder.RegisterType<ShiftCategoryPointExtractor>().As<IShiftCategoryPointExtractor>().SingleInstance();
			builder.RegisterType<SeniorityExtractor>().As<ISeniorityExtractor>().SingleInstance();
			builder.RegisterType<DetermineTeamBlockPriority>().As<IDetermineTeamBlockPriority>();
			builder.RegisterType<TeamBlockSwapValidator>().As<ITeamBlockSwapValidator>();
			builder.RegisterType<TeamBlockSwapDayValidator>().As<ITeamBlockSwapDayValidator>();
			builder.RegisterType<TeamBlockSwap>().As<ITeamBlockSwap>();
			builder.RegisterType<TeamBlockLockValidator>().As<ITeamBlockLockValidator>();
			builder.RegisterType<SeniorityTeamBlockSwapValidator>().As<ISeniorityTeamBlockSwapValidator>();
			builder.RegisterType<DayOffRulesValidator>().As<IDayOffRulesValidator>().SingleInstance();
			builder.RegisterType<SeniorityTeamBlockSwapper>().As<ISeniorityTeamBlockSwapper>();

			//ITeamBlockSameTimeZoneValidator

			//common
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>().SingleInstance();
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
			builder.RegisterType<MedianCalculatorForDays>().As<IMedianCalculatorForDays>().SingleInstance();
			builder.RegisterType<TwoDaysIntervalGenerator>().As<ITwoDaysIntervalGenerator>().SingleInstance();
			builder.RegisterType<MedianCalculatorForSkillInterval>().As<IMedianCalculatorForSkillInterval>().SingleInstance();
			builder.RegisterType<SkillIntervalDataOpenHour>().As<ISkillIntervalDataOpenHour>();
			builder.RegisterType<SameOpenHoursInTeamBlockSpecification>().As<ISameOpenHoursInTeamBlockSpecification>();
			builder.RegisterType<SameEndTimeTeamSpecification>().As<ISameEndTimeTeamSpecification>();
			builder.RegisterType<SameShiftCategoryBlockSpecification>().As<ISameShiftCategoryBlockSpecification>();
			builder.RegisterType<SameShiftCategoryTeamSpecification>().As<ISameShiftCategoryTeamSpecification>();
			builder.RegisterType<SameStartTimeBlockSpecification>().As<ISameStartTimeBlockSpecification>();
			builder.RegisterType<SameStartTimeTeamSpecification>().As<ISameStartTimeTeamSpecification>();
			builder.RegisterType<SameShiftBlockSpecification>().As<ISameShiftBlockSpecification>().SingleInstance();
			builder.RegisterType<ValidSampleDayPickerFromTeamBlock>().As<IValidSampleDayPickerFromTeamBlock>().SingleInstance();
			builder.RegisterType<TeamBlockSchedulingOptions>().As<ITeamBlockSchedulingOptions>();
			builder.RegisterType<TeamBlockRoleModelSelector>().As<ITeamBlockRoleModelSelector>();
			builder.RegisterType<TeamBlockSchedulingCompletionChecker>().As<ITeamBlockSchedulingCompletionChecker>();
			builder.RegisterType<ProposedRestrictionAggregator>().As<IProposedRestrictionAggregator>();
			builder.RegisterType<TeamBlockRestrictionAggregator>().As<ITeamBlockRestrictionAggregator>();
			builder.RegisterType<TeamRestrictionAggregator>().As<ITeamRestrictionAggregator>();
			builder.RegisterType<BlockRestrictionAggregator>().As<IBlockRestrictionAggregator>();
			builder.RegisterType<TeamBlockMissingDaysOffScheduler>().As<ITeamBlockMissingDaysOffScheduler>();
			builder.RegisterType<TeamMatrixChecker>().As<ITeamMatrixChecker>().SingleInstance();
			//ITeamMatrixChecker

			builder.RegisterType<TeamMemberTerminationOnBlockSpecification>().As<ITeamMemberTerminationOnBlockSpecification>().SingleInstance();
			builder.RegisterType<TeamBlockMissingDayOffHandler>().As<ITeamBlockMissingDayOffHandler>();
			builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().SingleInstance();
			builder.RegisterType<SplitSchedulePeriodToWeekPeriod>().SingleInstance();
			builder.RegisterType<ValidNumberOfDayOffInAWeekSpecification>().As<IValidNumberOfDayOffInAWeekSpecification>().SingleInstance();
			builder.RegisterType<TeamScheduling>().As<ITeamScheduling>();
			builder.RegisterType<TeamBlockSingleDayScheduler>().As<ITeamBlockSingleDayScheduler>();
			builder.RegisterType<TeamBlockScheduler>().As<TeamBlockScheduler>();
			builder.Register(c =>
			{
				//ugly hack. should be two different implementations instead
				var isMaxSeatToggleEnabled =
					c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_TeamBlockAdhereWithMaxSeatRule_23419);
				return c.Resolve<TeamBlockScheduler>(new NamedParameter("isMaxSeatToggleEnabled", isMaxSeatToggleEnabled));
			}).As<ITeamBlockScheduler>();
			builder.RegisterType<TeamBlockGenerator>().As<ITeamBlockGenerator>();
		}

		private static void registerTeamBlockSchedulingService(ContainerBuilder builder)
		{

			builder.RegisterType<CreateSkillIntervalDatasPerActivtyForDate>().As<ICreateSkillIntervalDatasPerActivtyForDate>();
			builder.RegisterType<CalculateAggregatedDataForActivtyAndDate>().As<ICalculateAggregatedDataForActivtyAndDate>();
			builder.RegisterType<CreateSkillIntervalDataPerDateAndActivity>().As<ICreateSkillIntervalDataPerDateAndActivity>();
			builder.RegisterType<OpenHourForDate>().As<IOpenHourForDate>().SingleInstance();
			builder.RegisterType<ActivityIntervalDataCreator>().As<IActivityIntervalDataCreator>();
			builder.RegisterType<WorkShiftFromEditableShift>().As<IWorkShiftFromEditableShift>().SingleInstance();
			builder.RegisterType<FirstShiftInTeamBlockFinder>().As<IFirstShiftInTeamBlockFinder>();
			builder.RegisterType<TeamBlockOpenHoursValidator>().As<ITeamBlockOpenHoursValidator>();
			//IFirstShiftInTeamBlockFinder
		}

		private static void registerTeamBlockIntradayOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockIntradayDecisionMaker>().As<ITeamBlockIntradayDecisionMaker>().SingleInstance();
			builder.RegisterType<RelativeDailyValueCalculatorForTeamBlock>().As<IRelativeDailyValueCalculatorForTeamBlock>().SingleInstance();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>().SingleInstance();
			builder.RegisterType<TeamDayOffModifier>().As<ITeamDayOffModifier>();
		}

		private static void registerWorkShiftSelector(ContainerBuilder builder)
		{
			builder.RegisterType<WorkShiftLengthValueCalculator>().As<IWorkShiftLengthValueCalculator>().SingleInstance();
			builder.RegisterType<WorkShiftValueCalculator>().As<IWorkShiftValueCalculator>();
			builder.RegisterType<EqualWorkShiftValueDecider>().As<IEqualWorkShiftValueDecider>().SingleInstance();
			builder.RegisterType<WorkShiftSelector>().As<IWorkShiftSelector>();
			builder.RegisterType<VisualLayerToBaseDateMapper>().As<IVisualLayerToBaseDateMapper>().SingleInstance();
			builder.RegisterType<MaxSeatsCalculationForTeamBlock>().As<IMaxSeatsCalculationForTeamBlock>().SingleInstance();
			builder.RegisterType<MaxSeatInformationGeneratorBasedOnIntervals>().As<IMaxSeatInformationGeneratorBasedOnIntervals>();
			builder.RegisterType<MaxSeatSkillAggregator>().As<IMaxSeatSkillAggregator>().SingleInstance();
			builder.RegisterType<MaxSeatInformationGeneratorBasedOnIntervals>().As<IMaxSeatInformationGeneratorBasedOnIntervals>();
			builder.RegisterType<MaxSeatsSpecificationDictionaryExtractor>().As<IMaxSeatsSpecificationDictionaryExtractor>();
			builder.RegisterType<IsMaxSeatsReachedOnSkillStaffPeriodSpecification>().As<IIsMaxSeatsReachedOnSkillStaffPeriodSpecification>().SingleInstance();
			builder.RegisterType<IntervalLevelMaxSeatInfo>();
			builder.RegisterType<MaxSeatBoostingFactorCalculator>().SingleInstance();
			builder.RegisterType<PullTargetValueFromSkillIntervalData>().SingleInstance();
			builder.RegisterType<ExtractIntervalsVoilatingMaxSeat>().As<IExtractIntervalsVoilatingMaxSeat>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private static void registerWorkShiftFilters(ContainerBuilder builder)
		{
			builder.RegisterType<ActivityRestrictionsShiftFilter>().As<IActivityRestrictionsShiftFilter>().SingleInstance();
			builder.RegisterType<BusinessRulesShiftFilter>().As<IBusinessRulesShiftFilter>();
			builder.RegisterType<CommonMainShiftFilter>().As<ICommonMainShiftFilter>().SingleInstance();
			builder.RegisterType<ContractTimeShiftFilter>().As<IContractTimeShiftFilter>();
			builder.RegisterType<DisallowedShiftCategoriesShiftFilter>().As<IDisallowedShiftCategoriesShiftFilter>().SingleInstance();
			builder.RegisterType<EarliestEndTimeLimitationShiftFilter>().As<IEarliestEndTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<EffectiveRestrictionShiftFilter>().As<IEffectiveRestrictionShiftFilter>().SingleInstance();
			builder.RegisterType<LatestStartTimeLimitationShiftFilter>().As<ILatestStartTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<MainShiftOptimizeActivitiesSpecificationShiftFilter>().As<IMainShiftOptimizeActivitiesSpecificationShiftFilter>().SingleInstance();
			builder.RegisterType<NotOverWritableActivitiesShiftFilter>().As<INotOverWritableActivitiesShiftFilter>();
			builder.RegisterType<PersonalShiftsShiftFilter>().As<IPersonalShiftsShiftFilter>();
			builder.RegisterType<RuleSetPersonalSkillsActivityFilter>().As<IRuleSetPersonalSkillsActivityFilter>().SingleInstance();
			builder.RegisterType<ShiftCategoryRestrictionShiftFilter>().As<IShiftCategoryRestrictionShiftFilter>().SingleInstance();
			builder.RegisterType<ValidDateTimePeriodShiftFilter>().As<IValidDateTimePeriodShiftFilter>();
			builder.RegisterType<TimeLimitsRestrictionShiftFilter>().As<ITimeLimitsRestrictionShiftFilter>();
			builder.RegisterType<WorkTimeLimitationShiftFilter>().As<IWorkTimeLimitationShiftFilter>().SingleInstance();
			builder.RegisterType<CommonActivityFilter>().As<ICommonActivityFilter>().SingleInstance();
			builder.RegisterType<RuleSetAccordingToAccessabilityFilter>().As<IRuleSetAccordingToAccessabilityFilter>();
			builder.RegisterType<TeamBlockRuleSetBagExtractor>().As<ITeamBlockRuleSetBagExtractor>().SingleInstance();
			builder.RegisterType<TeamBlockIncludedWorkShiftRuleFilter>().As<ITeamBlockIncludedWorkShiftRuleFilter>().SingleInstance();
			builder.RegisterType<RuleSetSkillActivityChecker>().As<IRuleSetSkillActivityChecker>().SingleInstance();
			builder.RegisterType<PersonalShiftAndMeetingFilter>().As<IPersonalShiftAndMeetingFilter>();
			builder.RegisterType<PersonalShiftMeetingTimeChecker>().As<IPersonalShiftMeetingTimeChecker>().SingleInstance();
			

			builder.RegisterType<DisallowedShiftProjectionCashesFilter>().As<DisallowedShiftProjectionCashesFilter>().SingleInstance();
			builder.RegisterType<DisallowedShiftProjectionCashesFilter29846Off>().As<DisallowedShiftProjectionCashesFilter29846Off>().SingleInstance();

			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Schedule_IntraIntervalOptimizer_29846)
			   ? (IDisallowedShiftProjectionCashesFilter)c.Resolve<DisallowedShiftProjectionCashesFilter>()
			   : c.Resolve<DisallowedShiftProjectionCashesFilter29846Off>())
				   .As<IDisallowedShiftProjectionCashesFilter>();
		}
	}
}