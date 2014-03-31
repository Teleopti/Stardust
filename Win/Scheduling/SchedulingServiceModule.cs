using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Win.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class SchedulingServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SchedulerStateHolder>().As<ISchedulerStateHolder>().InstancePerLifetimeScope();
            builder.RegisterType<OverriddenBusinessRulesHolder>().As<IOverriddenBusinessRulesHolder>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
            builder.RegisterType<PreSchedulingStatusChecker>().As<IPreSchedulingStatusChecker>().InstancePerLifetimeScope();
			builder.RegisterType<VirtualSkillHelper>().As<IVirtualSkillHelper>().InstancePerLifetimeScope();
            
            builder.RegisterType<SeatLimitationWorkShiftCalculator2>().As<ISeatLimitationWorkShiftCalculator2>().InstancePerLifetimeScope();
            builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<LongestPeriodForAssignmentCalculator>().As<ILongestPeriodForAssignmentCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<PersonSkillPeriodsDataHolderManager>().As<IPersonSkillPeriodsDataHolderManager>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessShiftValueCalculator>().As<IShiftCategoryFairnessShiftValueCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftProjectionCacheManager>().As<IShiftProjectionCacheManager>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftFromMasterActivityService>().As<IShiftFromMasterActivityService>().InstancePerLifetimeScope();
            builder.RegisterType<RuleSetDeletedActivityChecker>().As<IRuleSetDeletedActivityChecker>().InstancePerLifetimeScope();
            builder.RegisterType<RuleSetDeletedShiftCategoryChecker>().As<IRuleSetDeletedShiftCategoryChecker>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<FairnessValueCalculator>().As<IFairnessValueCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftCalculatorsManager>().As<IWorkShiftCalculatorsManager>().InstancePerLifetimeScope();
            builder.RegisterType<FairnessAndMaxSeatCalculatorsManager>().As<IFairnessAndMaxSeatCalculatorsManager>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulerStateScheduleDayChangedCallback>().As<IScheduleDayChangeCallback>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceCalculateDaysDecider>().As<IResourceCalculateDaysDecider>().InstancePerLifetimeScope();
            builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().InstancePerLifetimeScope();
            builder.RegisterType<OptimizationPreferences>().As<IOptimizationPreferences>().InstancePerLifetimeScope();
            builder.RegisterType<DayOffPlannerRules>().As<IDayOffPlannerRules>().InstancePerLifetimeScope();
            builder.RegisterType<NonBlendWorkShiftCalculator>().As<INonBlendWorkShiftCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessManager>().As<IShiftCategoryFairnessManager>().InstancePerLifetimeScope();
            builder.RegisterType<GroupShiftCategoryFairnessCreator>().As<IGroupShiftCategoryFairnessCreator>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessCalculator>().As<IShiftCategoryFairnessCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPagePerDateHolder>().As<IGroupPagePerDateHolder>().InstancePerLifetimeScope();
        	builder.RegisterType<DesiredShiftLengthCalculator>().As<IDesiredShiftLengthCalculator>().
        		InstancePerLifetimeScope();
        	builder.RegisterType<ShiftLengthDecider>().As<IShiftLengthDecider>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderService>().As<IWorkShiftFinderService>().InstancePerLifetimeScope();

            builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>().InstancePerLifetimeScope();
            builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();
            builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();

            builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().InstancePerDependency();
            builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleMatrixListCreator>().As<IScheduleMatrixListCreator>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryLimitationChecker>().As<IShiftCategoryLimitationChecker>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePartModifyAndRollbackService>().As<ISchedulePartModifyAndRollbackService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduler>().As<IAbsencePreferenceScheduler>().InstancePerLifetimeScope();
	        builder.RegisterType<AbsencePreferenceFullDayLayerCreator>().As<IAbsencePreferenceFullDayLayerCreator>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffScheduler>().As<IDayOffScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayAvailableForDayOffSpecification>().As<IScheduleDayAvailableForDayOffSpecification>().InstancePerLifetimeScope();

            builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().InstancePerLifetimeScope();
            builder.RegisterInstance(new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().SingleInstance();
            builder.RegisterType<FixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();

            
            builder.RegisterType<AdvanceDaysOffSchedulingService>().As<IAdvanceDaysOffSchedulingService>();
            builder.RegisterType<SkillResolutionProvider>().As<ISkillResolutionProvider>().InstancePerLifetimeScope();
            builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().InstancePerLifetimeScope();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().InstancePerLifetimeScope();
            builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteAndResourceCalculateService >().As<IDeleteAndResourceCalculateService >().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
            builder.RegisterType<IntervalDataMedianCalculator>().As<IIntervalDataCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<RestrictionAggregator>().As<IRestrictionAggregator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFilterService>().As<IWorkShiftFilterService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRestrictionExtractor>().As<IScheduleRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<SuggestedShiftRestrictionExtractor>().As<ISuggestedShiftRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayEquator>().As<IScheduleDayEquator>().InstancePerLifetimeScope();

            builder.RegisterType<GroupPersonsBuilder>().As<IGroupPersonsBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonFactory>().As<IGroupPersonFactory>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderResultHolder>().As<IWorkShiftFinderResultHolder>().InstancePerLifetimeScope();

            builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerDependency();
            builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
            builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().InstancePerDependency();

            builder.RegisterType<ScheduleMatrixLockableBitArrayConverter>().As<IScheduleMatrixLockableBitArrayConverter>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulingStateHolderAllSkillExtractor>().InstancePerDependency();
            builder.RegisterType<DailySkillForecastAndScheduledValueCalculator>().As<IDailySkillForecastAndScheduledValueCalculator>().InstancePerDependency();

            builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().InstancePerLifetimeScope();
            builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().InstancePerLifetimeScope();
            builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
            builder.RegisterType<MatrixUserLockLocker>().As<IMatrixUserLockLocker>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleFairnessCalculator>().As<IScheduleFairnessCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().As<IScheduleMatrixValueCalculatorProFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePeriodListShiftCategoryBackToLegalStateService>().As<ISchedulePeriodListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().As<IWorkShiftLegalStateDayIndexCalculator>().InstancePerDependency();

            builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
            builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();

            builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().InstancePerLifetimeScope();

			builder.RegisterType<SchedulerGroupPagesProvider>().As<ISchedulerGroupPagesProvider>().InstancePerLifetimeScope();

			builder.RegisterType<GroupScheduleGroupPageDataProvider>().As<IGroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageCreator>().As<IGroupPageCreator>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageFactory>().As<IGroupPageFactory>().InstancePerLifetimeScope();
			builder.RegisterType<GroupCreator>().As<IGroupCreator>().InstancePerLifetimeScope();
	        builder.RegisterType<SwapServiceNew>().As<ISwapServiceNew>();
			builder.RegisterType<GroupPersonBuilderForOptimization>().As<IGroupPersonBuilderForOptimization>().InstancePerLifetimeScope();
        	builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();
	        builder.RegisterType<EditableShiftMapper>().As<IEditableShiftMapper>();

			//DaysOffSchedulingService
        	builder.RegisterType<BestSpotForAddingDayOffFinder>().As<IBestSpotForAddingDayOffFinder>().
        		InstancePerDependency();
        	builder.RegisterType<MatrixDataListInSteadyState>().As<IMatrixDataListInSteadyState>().InstancePerDependency();
        	builder.RegisterType<HasContractDayOffDefinition>().As<IHasContractDayOffDefinition>().InstancePerDependency();
        	builder.RegisterType<ScheduleDayDataMapper>().As<IScheduleDayDataMapper>().InstancePerDependency();
        	builder.RegisterType<MatrixDataListCreator>().As<IMatrixDataListCreator>().InstancePerDependency();
        	builder.RegisterType<MatrixDataWithToFewDaysOff>().As<IMatrixDataWithToFewDaysOff>().InstancePerDependency();
        	builder.RegisterType<MissingDaysOffScheduler>().As<IMissingDaysOffScheduler>().InstancePerDependency();
			builder.RegisterType<TeamDayOffScheduler>().As<ITeamDayOffScheduler>().InstancePerDependency();
        	builder.RegisterType<DaysOffSchedulingService>().As<IDaysOffSchedulingService>().InstancePerDependency();

	        builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>();
	        builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
	        builder.RegisterType<TrueFalseRandomizer>().As<ITrueFalseRandomizer>();
	        builder.RegisterType<OfficialWeekendDays>().As<IOfficialWeekendDays>();
	        builder.RegisterType<CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker>().As<IDayOffDecisionMaker>();
	        builder.RegisterType<SchedulingOptionsCreator>().As<ISchedulingOptionsCreator>();
	        builder.RegisterType<LockableBitArrayChangesTracker>().As<ILockableBitArrayChangesTracker>();
	        builder.RegisterType<DayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>();
			//ITeamBlockScheduleCommand

			registerWorkShiftFilters(builder);
			registerWorkShiftSelector(builder);
			registerTeamBlockCommon(builder);
			registerTeamBlockDayOffOptimizerService(builder);
			registerTeamBlockIntradayOptimizerService(builder);
			registerTeamBlockSchedulingService(builder);
            registerTeamBlockOptimizationService(builder);

	        builder.RegisterType<AgentRestrictionsNoWorkShiftfFinder>().As<IAgentRestrictionsNoWorkShiftfFinder>();
            registerFairnessOptimizationService(builder);
			registerEqualNumberOfCategoryFairnessService(builder);

            builder.RegisterType<ScheduleOvertimeCommand>().As<IScheduleOvertimeCommand>();
            builder.RegisterType<OvertimePeriodValueMapper>();
            builder.RegisterType<MergeOvertimeSkillIntervalData>().As<IMergeOvertimeSkillIntervalData>();
            builder.RegisterType<CalculateBestOvertime>().As<ICalculateBestOvertime>();
            builder.RegisterType<OvertimeLengthDecider>().As<IOvertimeLengthDecider>();
            builder.RegisterType<ResourceCalculateDelayer>().As<IResourceCalculateDelayer>();
            builder.RegisterType<OvertimeSkillIntervalData>().As<IOvertimeSkillIntervalData>();
            builder.RegisterType<OvertimeSkillIntervalDataDivider>().As<IOvertimeSkillIntervalDataDivider>();
            builder.RegisterType<OvertimeSkillStaffPeriodToSkillIntervalDataMapper>().As<IOvertimeSkillStaffPeriodToSkillIntervalDataMapper>();
			builder.RegisterType<ProjectionProvider>().As<IProjectionProvider>();
            builder.RegisterType<NightlyRestRule>().As<IAssignmentPeriodRule>();
        }

        private void registerEqualNumberOfCategoryFairnessService(ContainerBuilder builder)
        {
			builder.RegisterType<EqualNumberOfCategoryFairnessService>().As<IEqualNumberOfCategoryFairnessService>();
			builder.RegisterType<DistributionForPersons>().As<IDistributionForPersons>();
			builder.RegisterType<FilterForEqualNumberOfCategoryFairness>().As<IFilterForEqualNumberOfCategoryFairness>();
			builder.RegisterType<FilterForTeamBlockInSelection>().As<IFilterForTeamBlockInSelection>();
			builder.RegisterType<FilterOnSwapableTeamBlocks>().As<IFilterOnSwapableTeamBlocks>();
			builder.RegisterType<TeamBlockSwapper>().As<ITeamBlockSwapper>();
			builder.RegisterType<EqualCategoryDistributionBestTeamBlockDecider>().As<IEqualCategoryDistributionBestTeamBlockDecider>();
			builder.RegisterType<EqualCategoryDistributionWorstTeamBlockDecider>().As<IEqualCategoryDistributionWorstTeamBlockDecider>();
			builder.RegisterType<FilterPersonsForTotalDistribution>().As<IFilterPersonsForTotalDistribution>();
			builder.RegisterType<DistributionReportService>().As<IDistributionReportService>();
			builder.RegisterType<EqualCategoryDistributionValue>().As<IEqualCategoryDistributionValue>();
			builder.RegisterType<FilterForFullyScheduledBlocks>().As<IFilterForFullyScheduledBlocks>();
			builder.RegisterType<FilterForNoneLockedTeamBlocks>().As<IFilterForNoneLockedTeamBlocks>();
			builder.RegisterType<TeamBlockShiftCategoryLimitationValidator>().As<ITeamBlockShiftCategoryLimitationValidator>();
			//ITeamBlockShiftCategoryLimitationValidator
        }

        private void registerFairnessOptimizationService(ContainerBuilder builder)
        {
            builder.RegisterType<TeamBlockSeniorityFairnessOptimizationService>().As<ITeamBlockSeniorityFairnessOptimizationService>();
            builder.RegisterType<PrioritiseWeekDay>().As<IPrioritiseWeekDay>();
            builder.RegisterType<ConstructTeamBlock>().As<IConstructTeamBlock>();
	        builder.RegisterType<ShiftCategoryPointExtractor>().As<IShiftCategoryPointExtractor>();
	        builder.RegisterType<ShiftCategoryPointInfoExtractor>().As<IShiftCategoryPointInfoExtractor>();
	        builder.RegisterType<SeniorityExtractor>().As<ISeniorityExtractor>();
            builder.RegisterType<DetermineTeamBlockPriority>().As<IDetermineTeamBlockPriority>();
            builder.RegisterType<TeamBlockSizeClassifier>().As<ITeamBlockSizeClassifier>();
            builder.RegisterType<TeamBlockWeightExtractor>().As<ITeamBlockWeightExtractor>();
            builder.RegisterType<TeamBlockListSwapAnalyzer>().As<ITeamBlockListSwapAnalyzer>();
            builder.RegisterType<ShiftCategoryPointExtractor>().As<IShiftCategoryPointExtractor>();
			builder.RegisterType<SeniorityExtractor>().As<ISeniorityExtractor>();
			builder.RegisterType<ShiftCategoryPointInfoExtractor>().As<IShiftCategoryPointInfoExtractor>();
			builder.RegisterType<DetermineTeamBlockPriority>().As<IDetermineTeamBlockPriority>();
			builder.RegisterType<TeamBlockListSwapAnalyzer>().As<ITeamBlockListSwapAnalyzer>();

			//common
			builder.RegisterType<TeamBlockPeriodValidator>().As<ITeamBlockPeriodValidator>();
			builder.RegisterType<TeamMemberCountValidator>().As<ITeamMemberCountValidator>();
			builder.RegisterType<TeamBlockContractTimeValidator>().As<ITeamBlockContractTimeValidator>();
			builder.RegisterType<TeamBlockSameSkillValidator>().As<ITeamBlockSameSkillValidator>();
			builder.RegisterType<TeamBlockPersonsSkillChecker>().As<ITeamBlockPersonsSkillChecker>();
			builder.RegisterType<TeamBlockSameRuleSetBagValidator>().As<ITeamBlockSameRuleSetBagValidator>();
            
        }

        private static void registerTeamBlockCommon(ContainerBuilder builder)
		{
            builder.RegisterType<LocateMissingIntervalsIfMidNightBreak>().As<ILocateMissingIntervalsIfMidNightBreak>();
            builder.RegisterType<FilterOutIntervalsAfterMidNight>().As<IFilterOutIntervalsAfterMidNight>();
            builder.RegisterType<GroupPersonSkillAggregator>().As<IGroupPersonSkillAggregator>();
			builder.RegisterType<DynamicBlockFinder>().As<IDynamicBlockFinder>();
			builder.RegisterType<TeamBlockInfoFactory>().As<ITeamBlockInfoFactory>();
			builder.RegisterType<TeamInfoFactory>().As<ITeamInfoFactory>();
			builder.RegisterType<SafeRollbackAndResourceCalculation>().As<ISafeRollbackAndResourceCalculation>();
			builder.RegisterType<TeamBlockClearer>().As<ITeamBlockClearer>();
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().As<ITeamBlockRestrictionOverLimitValidator>();
			builder.RegisterType<TeamBlockSteadyStateValidator>().As<ITeamBlockSteadyStateValidator>();
			builder.RegisterType<RestrictionOverLimitDecider>().As<IRestrictionOverLimitDecider>();
			builder.RegisterType<RestrictionChecker>().As<ICheckerRestriction>();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>();
			builder.RegisterType<MatrixListFactory>().As<IMatrixListFactory>();
			builder.RegisterType<TeamBlockMaxSeatChecker>().As<ITeamBlockMaxSeatChecker>();
            builder.RegisterType<DailyTargetValueCalculatorForTeamBlock>().As<IDailyTargetValueCalculatorForTeamBlock>();
			builder.RegisterType<NightlyRestRestrictionForTeamBlock>().As<INightlyRestRestrictionForTeamBlock>();
            builder.RegisterType<MedianCalculatorForDays>().As<IMedianCalculatorForDays>();
            builder.RegisterType<TwoDaysIntervalGenerator>().As<ITwoDaysIntervalGenerator>();
            builder.RegisterType<MedianCalculatorForSkillInterval>().As<IMedianCalculatorForSkillInterval>();
            builder.RegisterType<SkillIntervalDataOpenHour>().As<ISkillIntervalDataOpenHour>();
            builder.RegisterType<SameOpenHoursInTeamBlockSpecification>().As<ISameOpenHoursInTeamBlockSpecification>();
			builder.RegisterType<SameEndTimeTeamSpecification>().As<ISameEndTimeTeamSpecification>();
			builder.RegisterType<SameShiftCategoryBlockSpecification>().As<ISameShiftCategoryBlockSpecification>();
			builder.RegisterType<SameShiftCategoryTeamSpecification>().As<ISameShiftCategoryTeamSpecification>();
            builder.RegisterType<SameStartTimeBlockSpecification>().As<ISameStartTimeBlockSpecification>();
            builder.RegisterType<SameStartTimeTeamSpecification>().As<ISameStartTimeTeamSpecification>();
			builder.RegisterType<SameShiftBlockSpecification>().As<ISameShiftBlockSpecification>();
            builder.RegisterType<ValidSampleDayPickerFromTeamBlock>().As<IValidSampleDayPickerFromTeamBlock>();
            builder.RegisterType<TeamBlockSchedulingOptions>().As<ITeamBlockSchedulingOptions>();
			builder.RegisterType<TeamBlockRoleModelSelector>().As<ITeamBlockRoleModelSelector>();
			builder.RegisterType<TeamBlockSchedulingCompletionChecker>().As<ITeamBlockSchedulingCompletionChecker>();
			builder.RegisterType<ProposedRestrictionAggregator>().As<IProposedRestrictionAggregator>();
			builder.RegisterType<TeamBlockRestrictionAggregator>().As<ITeamBlockRestrictionAggregator>();
			builder.RegisterType<TeamRestrictionAggregator>().As<ITeamRestrictionAggregator>();
			builder.RegisterType<BlockRestrictionAggregator>().As<IBlockRestrictionAggregator>();
            builder.RegisterType<TeamBlockMissingDaysOffScheduler>().As<ITeamBlockMissingDaysOffScheduler>();
            builder.RegisterType<TeamMemberTerminationOnBlockSpecification>().As<ITeamMemberTerminationOnBlockSpecification>();
		}

		private static void registerTeamBlockSchedulingService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockScheduleCommand>().As<ITeamBlockScheduleCommand>();
			builder.RegisterType<CreateSkillIntervalDatasPerActivtyForDate>().As<ICreateSkillIntervalDatasPerActivtyForDate>();
			builder.RegisterType<CalculateAggregatedDataForActivtyAndDate>().As<ICalculateAggregatedDataForActivtyAndDate>();
			builder.RegisterType<CreateSkillIntervalDataPerDateAndActivity>().As<ICreateSkillIntervalDataPerDateAndActivity>();
			builder.RegisterType<TeamBlockOpenHourService>().As<ITeamBlockOpenHourService>();
			builder.RegisterType<OpenHourForDate>().As<IOpenHourForDate>();
		}

        private void registerTeamBlockOptimizationService(ContainerBuilder builder)
        {
            builder.RegisterType<TeamBlockOptimizationCommand>().As<ITeamBlockOptimizationCommand>();
        }

		private static void registerTeamBlockIntradayOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockIntradayDecisionMaker>().As<ITeamBlockIntradayDecisionMaker>();
		    builder.RegisterType<RelativeDailyValueCalculatorForTeamBlock>().As<IRelativeDailyValueCalculatorForTeamBlock>();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>();
			builder.RegisterType<TeamDayOffModifier>().As<ITeamDayOffModifier>();
		}

	    private static void registerWorkShiftSelector(ContainerBuilder builder)
	    {
		    builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();
		    builder.RegisterType<WorkShiftLengthValueCalculator>().As<IWorkShiftLengthValueCalculator>();
		    builder.RegisterType<WorkShiftValueCalculator>().As<IWorkShiftValueCalculator>();
		    builder.RegisterType<EqualWorkShiftValueDecider>().As<IEqualWorkShiftValueDecider>();
		    builder.RegisterType<WorkShiftSelector>().As<IWorkShiftSelector>();
	    }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static void registerWorkShiftFilters(ContainerBuilder builder)
	    {
		    builder.RegisterType<ActivityRestrictionsShiftFilter>().As<IActivityRestrictionsShiftFilter>();
		    builder.RegisterType<BusinessRulesShiftFilter>().As<IBusinessRulesShiftFilter>();
		    builder.RegisterType<CommonMainShiftFilter>().As<ICommonMainShiftFilter>();
		    builder.RegisterType<ContractTimeShiftFilter>().As<IContractTimeShiftFilter>();
		    builder.RegisterType<DisallowedShiftCategoriesShiftFilter>().As<IDisallowedShiftCategoriesShiftFilter>();
		    builder.RegisterType<EarliestEndTimeLimitationShiftFilter>().As<IEarliestEndTimeLimitationShiftFilter>();
		    builder.RegisterType<EffectiveRestrictionShiftFilter>().As<IEffectiveRestrictionShiftFilter>();
		    builder.RegisterType<LatestStartTimeLimitationShiftFilter>().As<ILatestStartTimeLimitationShiftFilter>();
		    builder.RegisterType<MainShiftOptimizeActivitiesSpecificationShiftFilter>().As<IMainShiftOptimizeActivitiesSpecificationShiftFilter>();
		    builder.RegisterType<NotOverWritableActivitiesShiftFilter>().As<INotOverWritableActivitiesShiftFilter>();
		    builder.RegisterType<PersonalShiftsShiftFilter>().As<IPersonalShiftsShiftFilter>();
		    builder.RegisterType<RuleSetToShiftsGenerator>().As<IRuleSetToShiftsGenerator>();
		    builder.RegisterType<ShiftCategoryRestrictionShiftFilter>().As<IShiftCategoryRestrictionShiftFilter>();
		    builder.RegisterType<ValidDateTimePeriodShiftFilter>().As<IValidDateTimePeriodShiftFilter>();
		    builder.RegisterType<TimeLimitsRestrictionShiftFilter>().As<ITimeLimitsRestrictionShiftFilter>();
		    builder.RegisterType<WorkTimeLimitationShiftFilter>().As<IWorkTimeLimitationShiftFilter>();
			builder.RegisterType<CommonActivityFilter>().As<ICommonActivityFilter>();
            builder.RegisterType<RuleSetAccordingToAccessabilityFilter>().As<IRuleSetAccordingToAccessabilityFilter>();
            builder.RegisterType<TeamBlockRuleSetBagExtractor>().As<ITeamBlockRuleSetBagExtractor>();
            builder.RegisterType<TeamBlockIncludedWorkShiftRuleFilter>().As<ITeamBlockIncludedWorkShiftRuleFilter>();
			builder.RegisterType<RuleSetSkillActivityChecker>().As<IRuleSetSkillActivityChecker>();
			//IRuleSetSkillActivityChecker
	    }
    }
}
