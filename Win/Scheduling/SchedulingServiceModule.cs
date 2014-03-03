using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Scheduling;
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
			builder.RegisterType<GroupShiftLengthDecider>().As<IGroupShiftLengthDecider>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderService>().As<IWorkShiftFinderService>().InstancePerLifetimeScope();

            builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>().InstancePerLifetimeScope();
            builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().InstancePerLifetimeScope();
	        builder.RegisterType<SingleSkillMaxSeatCalculator>().As<ISingleSkillMaxSeatCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();

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
            builder.RegisterType<KeepRestrictionCreator>().As<IKeepRestrictionCreator>().InstancePerLifetimeScope();
            builder.RegisterInstance(new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().SingleInstance();
            builder.RegisterType<FixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();

            
            builder.RegisterType<AdvanceDaysOffSchedulingService>().As<IAdvanceDaysOffSchedulingService>();
            builder.RegisterType<SkillResolutionProvider>().As<ISkillResolutionProvider>().InstancePerLifetimeScope();
            builder.RegisterType<SkillIntervalDataDivider>().As<ISkillIntervalDataDivider>().InstancePerLifetimeScope();
            builder.RegisterType<BlockSchedulingService>().As<IBlockSchedulingService>().InstancePerLifetimeScope();
            builder.RegisterType<SkillDayPeriodIntervalDataGenerator>().As<ISkillDayPeriodIntervalDataGenerator>().InstancePerLifetimeScope();
			builder.RegisterType<SkillStaffPeriodToSkillIntervalDataMapper>().As<ISkillStaffPeriodToSkillIntervalDataMapper>().InstancePerLifetimeScope();
            builder.RegisterType<SkillIntervalDataSkillFactorApplier>().As<ISkillIntervalDataSkillFactorApplier>().InstancePerLifetimeScope();
			builder.RegisterType<SkillIntervalDataAggregator>().As<ISkillIntervalDataAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<DayIntervalDataCalculator>().As<IDayIntervalDataCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<BestBlockShiftCategoryFinder>().As<IBestBlockShiftCategoryFinder>().InstancePerLifetimeScope();
            builder.RegisterType<BlockSchedulingWorkShiftFinderService>().As<IBlockSchedulingWorkShiftFinderService>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteAndResourceCalculateService >().As<IDeleteAndResourceCalculateService >().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
            builder.RegisterType<BlockFinderFactory>().As<IBlockFinderFactory>();
            builder.RegisterType<IntervalDataMedianCalculator>().As<IIntervalDataCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<RestrictionAggregator>().As<IRestrictionAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<OpenHoursToEffectiveRestrictionConverter>().As<IOpenHoursToEffectiveRestrictionConverter>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFilterService>().As<IWorkShiftFilterService>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRestrictionExtractor>().As<IScheduleRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<SuggestedShiftRestrictionExtractor>().As<ISuggestedShiftRestrictionExtractor>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayEquator>().As<IScheduleDayEquator>().InstancePerLifetimeScope();

	        
                    
            builder.RegisterType<GroupSchedulingService>().As<IGroupSchedulingService>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonsBuilder>().As<IGroupPersonsBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonFactory>().As<IGroupPersonFactory>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPersonConsistentChecker>().As<IGroupPersonConsistentChecker>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderResultHolder>().As<IWorkShiftFinderResultHolder>().InstancePerLifetimeScope();

            builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerDependency();
            builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerDependency();
            builder.RegisterType<SchedulePeriodTargetTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().InstancePerDependency();
            //builder.RegisterType<WorkShiftBackToLegalStateServicePro>().As<IWorkShiftBackToLegalStateServicePro>().InstancePerLifetimeScope();
            //need some refactoring in ctor

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
            builder.RegisterType<GroupListShiftCategoryBackToLegalStateService>().As<IGroupListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().As<IWorkShiftLegalStateDayIndexCalculator>().InstancePerDependency();

            builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
            builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();
            builder.RegisterType<GroupMatrixHelper>().As<IGroupMatrixHelper>().InstancePerLifetimeScope();
            builder.RegisterType<GroupMatrixContainerCreator>().As<IGroupMatrixContainerCreator>().InstancePerLifetimeScope();

            
            builder.RegisterType<GroupDayOffOptimizerCreator>().As<IGroupDayOffOptimizerCreator>().InstancePerLifetimeScope();

            builder.RegisterType<BlockOptimizerBlockCleaner>().As<IBlockOptimizerBlockCleaner>();
            builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().InstancePerLifetimeScope();

			builder.RegisterType<SchedulerGroupPagesProvider>().As<ISchedulerGroupPagesProvider>().InstancePerLifetimeScope();

			builder.RegisterType<BestGroupValueExtractorThreadFactory>().As<IBestGroupValueExtractorThreadFactory>().InstancePerLifetimeScope();
			builder.RegisterType<PossibleCombinationsOfStartEndCategoryCreator>().As<IPossibleCombinationsOfStartEndCategoryCreator>().InstancePerLifetimeScope();
			builder.RegisterType<PossibleCombinationsOfStartEndCategoryRunner>().As<IPossibleCombinationsOfStartEndCategoryRunner>().InstancePerLifetimeScope();


			builder.RegisterType<GroupScheduleGroupPageDataProvider>().As<IGroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageCreator>().As<IGroupPageCreator>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPageFactory>().As<IGroupPageFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessComparer>().As<IShiftCategoryFairnessComparer>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessAggregator>().As<IShiftCategoryFairnessAggregator>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessGroupPersonHolder>().As<IShiftCategoryFairnessGroupPersonHolder>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessAggregateManager>().As<IShiftCategoryFairnessAggregateManager>().InstancePerLifetimeScope();
			
			builder.RegisterType<SwapServiceNew>().As<ISwapServiceNew>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessOptimizer>().As<IShiftCategoryFairnessOptimizer>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessSwapper>().As<IShiftCategoryFairnessSwapper>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessSwapFinder>().As<IShiftCategoryFairnessSwapFinder>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessSwapCategorySorter>().As<IShiftCategoryFairnessCategorySorter>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessReScheduler>().As<IShiftCategoryFairnessReScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryChecker>().As<IShiftCategoryChecker>().InstancePerLifetimeScope();
			builder.RegisterType<GroupPersonBuilderForOptimization>().As<IGroupPersonBuilderForOptimization>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessPersonsSwappableChecker>().As<IShiftCategoryFairnessPersonsSwappableChecker>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessPersonsSkillChecker>().As<IShiftCategoryFairnessPersonsSkillChecker>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessRuleSetChecker>().As<IShiftCategoryFairnessRuleSetChecker>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessContractTimeChecker>().As<IShiftCategoryFairnessContractTimeChecker>().InstancePerLifetimeScope();
			builder.RegisterType<ShiftCategoryFairnessContractToleranceChecker>().As<IShiftCategoryFairnessContractToleranceChecker>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationOverLimitByRestrictionDeciderCreator>().As<IOptimizationOverLimitByRestrictionDeciderCreator>().InstancePerLifetimeScope();
        	builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();

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


			registerWorkShiftFilters(builder);
			registerWorkShiftSelector(builder);
			registerTeamBlockCommon(builder);
			registerTeamBlockDayOffOptimizerService(builder);
			registerTeamBlockIntradayOptimizerService(builder);
        }

		private static void registerTeamBlockCommon(ContainerBuilder builder)
		{
			builder.RegisterType<GroupPersonSkillAggregator>().As<IGroupPersonSkillAggregator>();
			builder.RegisterType<DynamicBlockFinder>().As<IDynamicBlockFinder>();
			builder.RegisterType<TeamBlockInfoFactory>().As<ITeamBlockInfoFactory>();
			builder.RegisterType<TeamInfoFactory>().As<ITeamInfoFactory>();
			builder.RegisterType<SafeRollbackAndResourceCalculation>().As<ISafeRollbackAndResourceCalculation>();
			builder.RegisterType<TeamBlockClearer>().As<ITeamBlockClearer>();
			builder.RegisterType<TeamBlockRestrictionOverLimitValidator>().As<ITeamBlockRestrictionOverLimitValidator>();
			builder.RegisterType<BlockSteadyStateValidator>().As<IBlockSteadyStateValidator>();
			builder.RegisterType<RestrictionOverLimitDecider>().As<IRestrictionOverLimitDecider>();
			builder.RegisterType<RestrictionChecker>().As<ICheckerRestriction>();
            builder.RegisterType<MedianCalculatorForDays >().As<IMedianCalculatorForDays>();
		}

		private static void registerTeamBlockIntradayOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<TeamBlockIntradayDecisionMaker>().As<ITeamBlockIntradayDecisionMaker>();
			builder.RegisterType<StandardDeviationSumCalculator>().As<IStandardDeviationSumCalculator>();
		}

		private static void registerTeamBlockDayOffOptimizerService(ContainerBuilder builder)
		{
			builder.RegisterType<LockableBitArrayFactory>().As<ILockableBitArrayFactory>();
			builder.RegisterType<TeamDayOffModifier>().As<ITeamDayOffModifier>();
			builder.RegisterType<BlockSteadyStateValidator>().As<IBlockSteadyStateValidator>();
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
		    builder.RegisterType<ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter>().As<IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter>();
		    builder.RegisterType<ValidDateTimePeriodShiftFilter>().As<IValidDateTimePeriodShiftFilter>();
		    builder.RegisterType<TimeLimitsRestrictionShiftFilter>().As<ITimeLimitsRestrictionShiftFilter>();
		    builder.RegisterType<WorkTimeLimitationShiftFilter>().As<IWorkTimeLimitationShiftFilter>();
			builder.RegisterType<RuleSetSkillActivityChecker>().As<IRuleSetSkillActivityChecker>();
	    }
    }
}
