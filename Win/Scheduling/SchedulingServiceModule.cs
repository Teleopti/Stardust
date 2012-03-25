using Autofac;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
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
            
            builder.RegisterType<SchedulingOptions>().As<ISchedulingOptions>().InstancePerLifetimeScope();
            builder.RegisterType<OptimizerOriginalPreferences>().As<IOptimizerOriginalPreferences>().InstancePerLifetimeScope();
            builder.RegisterType<OptimizationPreferences>().As<IOptimizationPreferences>().InstancePerLifetimeScope();
            builder.RegisterType<DayOffPlannerRules>().As<IDayOffPlannerRules>().InstancePerLifetimeScope();
            builder.RegisterType<OptimizerAdvancedPreferences>().As<IOptimizerAdvancedPreferences>().InstancePerLifetimeScope();
            
            builder.RegisterType<NonBlendWorkShiftCalculator>().As<INonBlendWorkShiftCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessManager>().As<IShiftCategoryFairnessManager>().InstancePerLifetimeScope();
            builder.RegisterType<GroupShiftCategoryFairnessCreator>().As<IGroupShiftCategoryFairnessCreator>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryFairnessCalculator>().As<IShiftCategoryFairnessCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPagePerDateHolder>().As<IGroupPagePerDateHolder>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderService>().As<IWorkShiftFinderService>().InstancePerLifetimeScope();

            builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>().InstancePerLifetimeScope();
            builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>().InstancePerLifetimeScope();
            builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();

            builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<RestrictionExtractor>().As<IRestrictionExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePeriodTargetTimeTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleMatrixListCreator>().As<IScheduleMatrixListCreator>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftCategoryLimitationChecker>().As<IShiftCategoryLimitationChecker>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePartModifyAndRollbackService>().As<ISchedulePartModifyAndRollbackService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
			builder.RegisterType<AbsencePreferenceScheduler>().As<IAbsencePreferenceScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffScheduler>().As<IDayOffScheduler>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayAvailableForDayOffSpecification>().As<IScheduleDayAvailableForDayOffSpecification>().InstancePerLifetimeScope();

            builder.RegisterType<DayOffsInPeriodCalculator>().As<IDayOffsInPeriodCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().InstancePerLifetimeScope();
            builder.RegisterType<KeepRestrictionCreator>().As<IKeepRestrictionCreator>().InstancePerLifetimeScope();
            builder.RegisterInstance(new ScheduleTagSetter(NullScheduleTag.Instance)).As<IScheduleTagSetter>().SingleInstance();
            builder.RegisterType<FixedStaffSchedulingService>().As<IFixedStaffSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<StudentSchedulingService>().As<IStudentSchedulingService>().InstancePerLifetimeScope();

            builder.RegisterType<ShiftProjectionCacheFilter>().As<IShiftProjectionCacheFilter>().InstancePerLifetimeScope();
            
            builder.RegisterType<BlockSchedulingService>().As<IBlockSchedulingService>().InstancePerLifetimeScope();
            builder.RegisterType<BestBlockShiftCategoryFinder>().As<IBestBlockShiftCategoryFinder>().InstancePerLifetimeScope();
            builder.RegisterType<BlockSchedulingWorkShiftFinderService>().As<IBlockSchedulingWorkShiftFinderService>().InstancePerLifetimeScope();
            builder.RegisterType<AverageShiftLengthValueCalculator>().As<IAverageShiftLengthValueCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<DeleteSchedulePartService>().As<IDeleteSchedulePartService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleDayService>().As<IScheduleDayService>().InstancePerLifetimeScope();
            builder.RegisterType<BlockFinderFactory>().As<IBlockFinderFactory>().InstancePerLifetimeScope();
                    
            builder.RegisterType<GroupSchedulingService>().As<IGroupSchedulingService>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonsBuilder>().As<IGroupPersonsBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonFactory>().As<IGroupPersonFactory>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonShiftCategoryConsistentChecker>().As<IGroupPersonShiftCategoryConsistentChecker>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftFinderResultHolder>().As<IWorkShiftFinderResultHolder>().InstancePerLifetimeScope();

            builder.RegisterType<WorkShiftWeekMinMaxCalculator>().As<IWorkShiftWeekMinMaxCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<PossibleMinMaxWorkShiftLengthExtractor>().As<IPossibleMinMaxWorkShiftLengthExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftMinMaxCalculator>().As<IWorkShiftMinMaxCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePeriodTargetTimeTimeCalculator>().As<ISchedulePeriodTargetTimeCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftBackToLegalStateBitArrayCreator>().As<IWorkShiftBackToLegalStateBitArrayCreator>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftBackToLegalStateServicePro>().As<IWorkShiftBackToLegalStateServicePro>().InstancePerLifetimeScope();
            //need some refactoring in ctor
            //builder.RegisterType<DayOffDecisionMakerExecuter>().As<IDayOffDecisionMakerExecuter>().InstancePerLifetimeScope();
            
            builder.RegisterType<IntradayActivityOptimizerService>().As<IIntradayActivityOptimizerService>().InstancePerLifetimeScope();

            //groupOpt
            builder.RegisterType<GroupPersonSameForPersonOnDateChecker>().As<IGroupPersonSameForPersonOnDateChecker>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonSameDayOffsChecker>().As<IGroupPersonSameDayOffsChecker>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonSchedulePeriodChecker>().As<IGroupPersonSchedulePeriodChecker>().InstancePerLifetimeScope();
            builder.RegisterType<GroupPersonPreOptimizationChecker>().As<IGroupPersonPreOptimizationChecker>().InstancePerLifetimeScope();

            builder.RegisterType<ScheduleMatrixLockableBitArrayConverter>().As<IScheduleMatrixLockableBitArrayConverter>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleResultDataExtractorProvider>().As<IScheduleResultDataExtractorProvider>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulingStateHolderAllSkillExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<DailySkillForecastAndScheduledValueCalculator>().As<IDailySkillForecastAndScheduledValueCalculator>().InstancePerLifetimeScope();

            builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>().InstancePerLifetimeScope();
            builder.RegisterType<RuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().As<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak>().InstancePerLifetimeScope();
            builder.RegisterType<GridlockManager>().As<IGridlockManager>().InstancePerLifetimeScope();
            builder.RegisterType<MatrixUserLockLocker>().As<IMatrixUserLockLocker>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleFairnessCalculator>().As<IScheduleFairnessCalculator>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleMatrixValueCalculatorProFactory>().As<IScheduleMatrixValueCalculatorProFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulePeriodListShiftCategoryBackToLegalStateService>().As<ISchedulePeriodListShiftCategoryBackToLegalStateService>().InstancePerLifetimeScope();
            builder.RegisterType<WorkShiftLegalStateDayIndexCalculator>().As<IWorkShiftLegalStateDayIndexCalculator>().InstancePerLifetimeScope();

            builder.RegisterType<WorkTimeStartEndExtractor>().As<IWorkTimeStartEndExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<DayOffOptimizerValidator>().As<IDayOffOptimizerValidator>().InstancePerLifetimeScope();
            builder.RegisterType<NewDayOffRule>().As<INewDayOffRule>().InstancePerLifetimeScope();
            builder.RegisterType<GroupMatrixHelper>().As<IGroupMatrixHelper>().InstancePerLifetimeScope();
            builder.RegisterType<GroupMatrixContainerCreator>().As<IGroupMatrixContainerCreator>().InstancePerLifetimeScope();

            builder.RegisterType<LockableBitArrayChangesTracker>().As<ILockableBitArrayChangesTracker>().InstancePerLifetimeScope();
            builder.RegisterType<GroupDayOffOptimizerCreator>().As<IGroupDayOffOptimizerCreator>().InstancePerLifetimeScope();
            
            builder.RegisterType<GroupScheduleGroupPageDataProvider>().InstancePerLifetimeScope();

            builder.RegisterType<GroupMatrixContainerCreator>().As<IGroupMatrixContainerCreator>().InstancePerLifetimeScope();

            builder.RegisterType<BlockOptimizerBlockCleaner>().As<IBlockOptimizerBlockCleaner>().InstancePerLifetimeScope();
            builder.RegisterType<EffectiveRestrictionCreator>().As<IEffectiveRestrictionCreator>().
                InstancePerLifetimeScope();

        }

    }
}
