using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Commands
{
    public interface ITeamBlockOptimizationCommand
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        void Execute(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod,
                                 IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences);
    }

    public class TeamBlockOptimizationCommand : ITeamBlockOptimizationCommand
    {
        private readonly IDayOffBackToLegalStateFunctions _dayOffBackToLegalStateFunctions;
        private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
        private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
        private readonly IGroupPageCreator _groupPageCreator;
        private readonly IGroupScheduleGroupPageDataProvider _groupPageDataProvider;
        private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
        private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
        private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
        private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
        private readonly IMatrixListFactory _matrixListFactory;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IRestrictionExtractor _restrictionExtractor;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;
        private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
        private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IScheduleDayEquator _scheduleDayEquator;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly ISchedulerStateHolder _schedulerState;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
        private readonly IStandardDeviationSumCalculator _standardDeviationSumCalculator;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly ITeamBlockClearer _teamBlockCleaner;
        private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
        private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
        private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
        private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
        private readonly ITeamDayOffModifier _teamDayOffModifier;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly IWorkShiftSelector _workShiftSelector;
        private BackgroundWorker _backgroundWorker;
        private readonly ITeamBlockSchedulingOptions _teamBlockScheudlingOptions;

        public TeamBlockOptimizationCommand(IScheduleDayChangeCallback scheduleDayChangeCallback,
                                            IResourceOptimizationHelper resourceOptimizationHelper,
                                            ISchedulerStateHolder schedulerState, IScheduleDayEquator scheduleDayEquator,
                                            IRestrictionOverLimitDecider restrictionOverLimitDecider,
                                            IGroupScheduleGroupPageDataProvider groupPageDataProvider,
                                            IGroupPagePerDateHolder groupPagePerDateHolder,
                                            IGroupPageCreator groupPageCreator, ITeamBlockClearer teamBlockCleaner,
                                            ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
                                            IRestrictionAggregator restrictionAggregator,
                                            IWorkShiftFilterService workShiftFilterService,
                                            IWorkShiftSelector workShiftSelector,
                                            ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
                                            IDayOffBackToLegalStateFunctions dayOffBackToLegalStateFunctions,
                                            IDayOffDecisionMaker dayOffDecisionMaker,
                                            IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
                                            IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
                                            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
                                            ILockableBitArrayFactory lockableBitArrayFactory,
                                            ISchedulingOptionsCreator schedulingOptionsCreator,
                                            ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
                                            ITeamBlockInfoFactory teamBlockInfoFactory,
                                            ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
                                            ITeamDayOffModifier teamDayOffModifier,
                                            ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
                                            ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
                                            ITeamBlockIntradayDecisionMaker teamBlockIntradayDecisionMaker,
                                            IStandardDeviationSumCalculator standardDeviationSumCalculator,
                                            IRestrictionExtractor restrictionExtractor,
                                            IMatrixListFactory matrixListFactory, ITeamBlockSchedulingOptions teamBlockScheudlingOptions)
        {
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _schedulerState = schedulerState;
            _scheduleDayEquator = scheduleDayEquator;
            _restrictionOverLimitDecider = restrictionOverLimitDecider;
            _groupPageDataProvider = groupPageDataProvider;
            _groupPagePerDateHolder = groupPagePerDateHolder;
            _groupPageCreator = groupPageCreator;
            _teamBlockCleaner = teamBlockCleaner;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
            _restrictionAggregator = restrictionAggregator;
            _workShiftFilterService = workShiftFilterService;
            _workShiftSelector = workShiftSelector;
            _sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
            _dayOffBackToLegalStateFunctions = dayOffBackToLegalStateFunctions;
            _dayOffDecisionMaker = dayOffDecisionMaker;
            _groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
            _dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _lockableBitArrayFactory = lockableBitArrayFactory;
            _schedulingOptionsCreator = schedulingOptionsCreator;
            _lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
            _teamBlockInfoFactory = teamBlockInfoFactory;
            _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
            _teamDayOffModifier = teamDayOffModifier;
            _teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
            _teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
            _teamBlockIntradayDecisionMaker = teamBlockIntradayDecisionMaker;
            _standardDeviationSumCalculator = standardDeviationSumCalculator;
            _restrictionExtractor = restrictionExtractor;
            _matrixListFactory = matrixListFactory;
            _teamBlockScheudlingOptions = teamBlockScheudlingOptions;

            _stateHolder = _schedulerState.SchedulingResultState;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public void Execute(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod,
                                        IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences)
        {
            _backgroundWorker = backgroundWorker;

            IDictionary<IPerson, IScheduleRange> allSelectedScheduleRangeClones =
                new Dictionary<IPerson, IScheduleRange>();
            foreach (IPerson selectedPerson in selectedPersons)
            {
                allSelectedScheduleRangeClones.Add(selectedPerson, _schedulerState.Schedules[selectedPerson]);
            }
            IMaxMovedDaysOverLimitValidator maxMovedDaysOverLimitValidator =
                new MaxMovedDaysOverLimitValidator(allSelectedScheduleRangeClones, _scheduleDayEquator);
            ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator = new TeamBlockRestrictionOverLimitValidator
                (_restrictionOverLimitDecider, maxMovedDaysOverLimitValidator);
            IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
            //this one handles userlocks as well
            if (optimizationPreferences.General.OptimizationStepDaysOff)
                optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences,
                                         teamBlockRestrictionOverLimitValidator, allMatrixes);
            if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
                optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences,
                                          teamBlockRestrictionOverLimitValidator, allMatrixes);
        }

        private IGroupPersonBuilderForOptimization callGroupPage(ISchedulingOptions schedulingOptions)
        {
            if (_schedulerState.LoadedPeriod != null)
            {
                IList<DateOnly> dates =
                    _schedulerState.LoadedPeriod.Value.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone).
                                    DayCollection();
                _groupPagePerDateHolder.GroupPersonGroupPagePerDate =
                    _groupPageCreator.CreateGroupPagePerDate(dates, _groupPageDataProvider,
                                                             schedulingOptions.GroupOnGroupPageForTeamBlockPer,
                                                             true);
            }
            IGroupPersonFactory groupPersonFactory = new GroupPersonFactory();
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
                new GroupPersonBuilderForOptimization(_schedulerState.SchedulingResultState, groupPersonFactory,
                                                      _groupPagePerDateHolder);
            return groupPersonBuilderForOptimization;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                              IOptimizationPreferences optimizationPreferences,
                                              ITeamBlockRestrictionOverLimitValidator
                                                  teamBlockRestrictionOverLimitValidator,
                                              IList<IScheduleMatrixPro> allMatrixes)
        {
            OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _restrictionExtractor,
                                                                optimizationPreferences, selectedPeriod);

            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService;
            ITeamBlockScheduler teamBlockScheduler;
            ISchedulingOptions schedulingOptions = initializeSharedFields(optimizationPreferences,
                                                                          out schedulePartModifyAndRollbackService,
                                                                          out teamBlockScheduler);

            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
                = new SmartDayOffBackToLegalStateService(
                    _dayOffBackToLegalStateFunctions,
                    optimizationPreferences.DaysOff,
                    100,
                    _dayOffDecisionMaker);
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
                _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
            ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
                                                                   _stateHolder);
            IPeriodValueCalculator periodValueCalculatorForAllSkills =
                OptimizerHelperHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced,
                                                                  allSkillsDataExtractor);
            ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder =
                new TeamBlockDaysOffMoveFinder(_scheduleResultDataExtractorProvider,
                                               dayOffBackToLegalStateService,
                                               _dayOffOptimizationDecisionMakerFactory);

            ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService =
                new TeamBlockDayOffOptimizerService(
                    teamInfoFactory,
                    _lockableBitArrayFactory,
                    _lockableBitArrayChangesTracker,
                    teamBlockScheduler,
                    _teamBlockInfoFactory,
                    periodValueCalculatorForAllSkills,
                    _safeRollbackAndResourceCalculation,
                    _teamDayOffModifier,
                    _teamBlockSteadyStateValidator,
                    _teamBlockCleaner,
                    teamBlockRestrictionOverLimitValidator,
                    _teamBlockMaxSeatChecker,
                    teamBlockDaysOffMoveFinder,_teamBlockScheudlingOptions
                    );

            IList<IDayOffTemplate> dayOffTemplates = (from item in _schedulerState.CommonStateHolder.DayOffs
                                                      where ((IDeleteTag) item).IsDeleted == false
                                                      select item).ToList();

            ((List<IDayOffTemplate>) dayOffTemplates).Sort(new DayOffTemplateSorter());

            teamBlockDayOffOptimizerService.ReportProgress += resourceOptimizerPersonOptimized;
            schedulingOptions.DayOffTemplate = dayOffTemplates[0];
            teamBlockDayOffOptimizerService.OptimizeDaysOff(
                allMatrixes,
                selectedPeriod,
                selectedPersons,
                optimizationPreferences,
                schedulePartModifyAndRollbackService,schedulingOptions);
            teamBlockDayOffOptimizerService.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                               IOptimizationPreferences optimizationPreferences,
                                               ITeamBlockRestrictionOverLimitValidator
                                                   teamBlockRestrictionOverLimitValidator,
                                               IList<IScheduleMatrixPro> allMatrixes)
        {
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService;
            ITeamBlockScheduler teamBlockScheduler;
            ISchedulingOptions schedulingOptions = initializeSharedFields(optimizationPreferences,
                                                                          out schedulePartModifyAndRollbackService,
                                                                          out teamBlockScheduler);

            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization = callGroupPage(schedulingOptions);
            var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
            var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,_teamBlockScheudlingOptions);

            ITeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService =
                new TeamBlockIntradayOptimizationService(
                    teamBlockGenerator,
                    teamBlockScheduler,
                    _schedulingOptionsCreator,
                    _safeRollbackAndResourceCalculation,
                    _teamBlockIntradayDecisionMaker,
                    teamBlockRestrictionOverLimitValidator,
                    _teamBlockCleaner,
                    _standardDeviationSumCalculator, _teamBlockMaxSeatChecker
                    );

            teamBlockIntradayOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
            teamBlockIntradayOptimizationService.Optimize(
                allMatrixes,
                selectedPeriod,
                selectedPersons,
                optimizationPreferences,
                schedulePartModifyAndRollbackService
                );
            teamBlockIntradayOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        private ISchedulingOptions initializeSharedFields(IOptimizationPreferences optimizationPreferences,
                                                          out ISchedulePartModifyAndRollbackService
                                                              schedulePartModifyAndRollbackService,
                                                          out ITeamBlockScheduler teamBlockScheduler)
        {
            ISchedulingOptions schedulingOptions =
                _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

            var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
                                                                        schedulingOptions.ConsiderShortBreaks);
            schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
                                                                                            _scheduleDayChangeCallback,
                                                                                            new ScheduleTagSetter(
                                                                                                schedulingOptions
                                                                                                    .TagToUseOnScheduling));
            var teamScheduling = new TeamScheduling(resourceCalculateDelayer, schedulePartModifyAndRollbackService);
            teamBlockScheduler = new TeamBlockScheduler(_skillDayPeriodIntervalDataGenerator,
                                                        _restrictionAggregator,
                                                        _workShiftFilterService, teamScheduling,
                                                        _workShiftSelector,
                                                        _teamBlockCleaner, schedulePartModifyAndRollbackService,
                                                        _sameOpenHoursInTeamBlockSpecification);
            return schedulingOptions;
        }


        private void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
        }
    }
}