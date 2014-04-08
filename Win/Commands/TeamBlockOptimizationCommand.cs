using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Commands
{
    public interface ITeamBlockOptimizationCommand
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		void Execute(BackgroundWorker backgroundWorker, DateOnlyPeriod selectedPeriod,
							IList<IPerson> selectedPersons,
							IOptimizationPreferences optimizationPreferences,
							ISchedulePartModifyAndRollbackService rollbackService,
							IScheduleTagSetter tagSetter,
							ISchedulingOptions schedulingOptions,
							IResourceCalculateDelayer resourceCalculateDelayer);
    }

    public class TeamBlockOptimizationCommand : ITeamBlockOptimizationCommand
    {
		private readonly IDayOffBackToLegalStateFunctions _dayOffBackToLegalStateFunctions;
		private readonly IDayOffDecisionMaker _dayOffDecisionMaker;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IRestrictionExtractor _restrictionExtractor;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockClearer _teamBlockCleaner;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockIntradayDecisionMaker _teamBlockIntradayDecisionMaker;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private BackgroundWorker _backgroundWorker;
		private readonly ITeamBlockSchedulingOptions _teamBlockScheudlingOptions;
		private readonly IDailyTargetValueCalculatorForTeamBlock _dailyTargetValueCalculatorForTeamBlock;
		private readonly IEqualNumberOfCategoryFairnessService _equalNumberOfCategoryFairness;
	    private readonly ITeamBlockSeniorityFairnessOptimizationService _teamBlockSeniorityFairnessOptimizationService;
	    private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
	    private readonly ITeamBlockDayOffFairnessOptimizationServiceFacade _teamBlockDayOffFairnessOptimizationService;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;

	    public TeamBlockOptimizationCommand(ISchedulerStateHolder schedulerStateHolder, 
											ITeamBlockClearer teamBlockCleaner,
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
                                            IRestrictionExtractor restrictionExtractor,
                                            IMatrixListFactory matrixListFactory, 
											ITeamBlockSchedulingOptions teamBlockScheudlingOptions, 
											IDailyTargetValueCalculatorForTeamBlock dailyTargetValueCalculatorForTeamBlock,
											IEqualNumberOfCategoryFairnessService equalNumberOfCategoryFairness,
											ITeamBlockSeniorityFairnessOptimizationService teamBlockSeniorityFairnessOptimizationService,
											ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator,
											ITeamBlockDayOffFairnessOptimizationServiceFacade teamBlockDayOffFairnessOptimizationService,
											ITeamBlockScheduler teamBlockScheduler)
	    {
		    _schedulerStateHolder = schedulerStateHolder;
			_teamBlockCleaner = teamBlockCleaner;
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
			_restrictionExtractor = restrictionExtractor;
			_matrixListFactory = matrixListFactory;
			_teamBlockScheudlingOptions = teamBlockScheudlingOptions;
			_dailyTargetValueCalculatorForTeamBlock = dailyTargetValueCalculatorForTeamBlock;
			_equalNumberOfCategoryFairness = equalNumberOfCategoryFairness;
			_teamBlockSeniorityFairnessOptimizationService = teamBlockSeniorityFairnessOptimizationService;
		    _teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
		    _teamBlockDayOffFairnessOptimizationService = teamBlockDayOffFairnessOptimizationService;
		    _teamBlockScheduler = teamBlockScheduler;
	    }

        public void Execute(BackgroundWorker backgroundWorker, 
							DateOnlyPeriod selectedPeriod,
							IList<IPerson> selectedPersons,
							IOptimizationPreferences optimizationPreferences,
							ISchedulePartModifyAndRollbackService rollbackServiceWithResourceCalculation,
							IScheduleTagSetter tagSetter,
							ISchedulingOptions schedulingOptions,
							IResourceCalculateDelayer resourceCalculateDelayer)
        {

			_backgroundWorker = backgroundWorker;

            IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);

	        IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization =
		        _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

			ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);

	        if (optimizationPreferences.General.OptimizationStepDaysOff)
		        optimizeTeamBlockDaysOff(selectedPeriod, selectedPersons, optimizationPreferences,
										 allMatrixes, rollbackServiceWithResourceCalculation,
		                                 schedulingOptions, teamInfoFactory, resourceCalculateDelayer);

            if (optimizationPreferences.General.OptimizationStepShiftsWithinDay)
                optimizeTeamBlockIntraday(selectedPeriod, selectedPersons, optimizationPreferences,
										  allMatrixes, rollbackServiceWithResourceCalculation, schedulingOptions, _teamBlockScheduler, resourceCalculateDelayer);

			if (optimizationPreferences.General.OptimizationStepFairness)
			{
				var rollbackServiceWithoutResourceCalculation = new SchedulePartModifyAndRollbackService(_schedulerStateHolder.SchedulingResultState, new DoNothingScheduleDayChangeCallBack(), tagSetter);
				OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _restrictionExtractor,
																optimizationPreferences, selectedPeriod);

				_equalNumberOfCategoryFairness.ReportProgress += resourceOptimizerPersonOptimized;
				_equalNumberOfCategoryFairness.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions,
				                                       _schedulerStateHolder.Schedules, rollbackServiceWithoutResourceCalculation,
				                                       optimizationPreferences);
				_equalNumberOfCategoryFairness.ReportProgress -= resourceOptimizerPersonOptimized;

				var instance = PrincipalAuthorization.Instance();
				if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction))
					return;

			    _teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
                _teamBlockDayOffFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions, _schedulerStateHolder.Schedules,
                                                    rollbackServiceWithoutResourceCalculation, optimizationPreferences);
                _teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;

			
			    _teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
				_teamBlockSeniorityFairnessOptimizationService.Execute(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions, _schedulerStateHolder.CommonStateHolder.ShiftCategories.ToList(), _schedulerStateHolder.Schedules, rollbackServiceWithoutResourceCalculation, optimizationPreferences);

                _teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
			}
				
        }

        private void optimizeTeamBlockDaysOff(DateOnlyPeriod selectedPeriod, 
											  IList<IPerson> selectedPersons,
                                              IOptimizationPreferences optimizationPreferences,
                                              IList<IScheduleMatrixPro> allMatrixes,
											  ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
											  ISchedulingOptions schedulingOptions,
												ITeamInfoFactory teamInfoFactory,
												IResourceCalculateDelayer resourceCalculateDelayer)
        {

            OptimizerHelperHelper.LockDaysForDayOffOptimization(allMatrixes, _restrictionExtractor,
                                                                optimizationPreferences, selectedPeriod);

            ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService
                = new SmartDayOffBackToLegalStateService(
                    _dayOffBackToLegalStateFunctions,
                    optimizationPreferences.DaysOff,
                    100,
                    _dayOffDecisionMaker);

            IScheduleResultDataExtractor allSkillsDataExtractor =
                OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod,
																   _schedulerStateHolder.SchedulingResultState);
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
                    _teamBlockScheduler,
                    _teamBlockInfoFactory,
                    periodValueCalculatorForAllSkills,
                    _safeRollbackAndResourceCalculation,
                    _teamDayOffModifier,
                    _teamBlockSteadyStateValidator,
                    _teamBlockCleaner,
                    _teamBlockRestrictionOverLimitValidator,
                    _teamBlockMaxSeatChecker,
                    teamBlockDaysOffMoveFinder, 
					_teamBlockScheudlingOptions
                    );

			IList<IDayOffTemplate> dayOffTemplates = (from item in _schedulerStateHolder.CommonStateHolder.DayOffs
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
		        schedulePartModifyAndRollbackService, schedulingOptions, resourceCalculateDelayer,
		        _schedulerStateHolder.SchedulingResultState);
            teamBlockDayOffOptimizerService.ReportProgress -= resourceOptimizerPersonOptimized;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void optimizeTeamBlockIntraday(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
												IOptimizationPreferences optimizationPreferences,

												IList<IScheduleMatrixPro> allMatrixes, 
												ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
												ISchedulingOptions schedulingOptions,
												ITeamBlockScheduler teamBlockScheduler,
												IResourceCalculateDelayer resourceCalculateDelayer)
        {
	        var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
            var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
	        var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
	                                                        _teamBlockScheudlingOptions);

	        ITeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService =
		        new TeamBlockIntradayOptimizationService(
			        teamBlockGenerator,
			        teamBlockScheduler,
			        _schedulingOptionsCreator,
			        _safeRollbackAndResourceCalculation,
			        _teamBlockIntradayDecisionMaker,
			        _teamBlockRestrictionOverLimitValidator,
			        _teamBlockCleaner, 
					_teamBlockMaxSeatChecker, 
					_dailyTargetValueCalculatorForTeamBlock
			        );

            teamBlockIntradayOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
	        teamBlockIntradayOptimizationService.Optimize(
		        allMatrixes,
		        selectedPeriod,
		        selectedPersons,
		        optimizationPreferences,
		        schedulePartModifyAndRollbackService, resourceCalculateDelayer, _schedulerStateHolder.SchedulingResultState);
            teamBlockIntradayOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
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