using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ScheduleOptimizerHelper : IScheduleOptimizerHelper
	{
		private Func<IWorkShiftFinderResultHolder> _allResults;
		private IBackgroundWorkerWrapper _backgroundWorker;
		private readonly ILifetimeScope _container;
		private readonly OptimizerHelperHelper _optimizerHelper;
		private readonly IToggleManager _toggleManager;
		private readonly IRequiredScheduleHelper _requiredScheduleHelper;
		private readonly IExtendReduceTimeHelper _extendReduceTimeHelper;
		private readonly IExtendReduceDaysOffHelper _extendReduceDaysOffHelper;
		private readonly Func<ISchedulingResultStateHolder> _stateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;

		public ScheduleOptimizerHelper(ILifetimeScope container, OptimizerHelperHelper optimizerHelper, IToggleManager toggleManager, IRequiredScheduleHelper requiredScheduleHelper)
		{
			_container = container;
			_optimizerHelper = optimizerHelper;
			_toggleManager = toggleManager;
			_requiredScheduleHelper = requiredScheduleHelper; 
			_allResults = () => _container.Resolve<IWorkShiftFinderResultHolder>();
			_extendReduceTimeHelper = new ExtendReduceTimeHelper(_container);
			_extendReduceDaysOffHelper = new ExtendReduceDaysOffHelper(_container,optimizerHelper,_allResults);
			_schedulerStateHolder = () => _container.Resolve<ISchedulerStateHolder>(); 
			_stateHolder = () => _schedulerStateHolder().SchedulingResultState;
			_scheduleDayChangeCallback = ()=>_container.Resolve<IScheduleDayChangeCallback>();
			_resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			_scheduleMatrixListCreator = _container.Resolve<IScheduleMatrixListCreator>();
			_optimizerHelperHelper = _container.Resolve<IOptimizerHelperHelper>();
			_bitArrayConverter = _container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();
			_dayOffOptimizationDecisionMakerFactory = container.Resolve<IDayOffOptimizationDecisionMakerFactory>();
		}

		private void optimizeIntraday(IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod)
		{
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					_stateHolder(),
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			var decisionMaker = new IntradayDecisionMaker(_bitArrayConverter);
			var scheduleService = _container.Resolve<IScheduleService>();

			IIntradayOptimizer2Creator creator = new IntradayOptimizer2Creator(
				matrixContainerList,
				workShiftContainerList,
				decisionMaker,
				scheduleService,
				optimizerPreferences,
				rollbackService,
				_stateHolder(),
				_container.Resolve<ISkillStaffPeriodToSkillIntervalDataMapper>(),
				_container.Resolve<ISkillIntervalDataDivider>(),
				_container.Resolve<ISkillIntervalDataAggregator>(),
				_container.Resolve<IEffectiveRestrictionCreator>(),
				_container.Resolve<IMinWeekWorkTimeRule>(),
				_container.Resolve<IResourceOptimizationHelper>());

			IList<IIntradayOptimizer2> optimizers = creator.Create();
			var service = new IntradayOptimizerContainer(optimizers, _container.Resolve<IDailyValueByAllSkillsExtractor>());

			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute(selectedPeriod, optimizerPreferences.Advanced.TargetValueCalculation);
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void optimizeWorkShifts(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerList,
			IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod)
		{

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder());
			IPeriodValueCalculator periodValueCalculator =
				_optimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IMoveTimeDecisionMaker decisionMaker = new MoveTimeDecisionMaker2(_bitArrayConverter);

			var scheduleService = _container.Resolve<IScheduleService>();

			IMoveTimeOptimizerCreator creator =
				new MoveTimeOptimizerCreator(scheduleMatrixOriginalStateContainerList,
					workShiftOriginalStateContainerList,
					decisionMaker,
					scheduleService,
					optimizerPreferences,
					rollbackService,
					_stateHolder(),
					_container.Resolve<IEffectiveRestrictionCreator>(),
					_container.Resolve<IMinWeekWorkTimeRule>(),
					_container.Resolve<IResourceOptimizationHelper>());

			IList<IMoveTimeOptimizer> optimizers = creator.Create();
			IScheduleOptimizationService service = new MoveTimeOptimizerContainer(optimizers, periodValueCalculator);

			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute();
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}


		private void optimizeDaysOff(
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
			IDayOffTemplate dayOffTemplate,
			DateOnlyPeriod selectedPeriod,
			IScheduleService scheduleService)
		{
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					_stateHolder(),
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
				new SchedulePartModifyAndRollbackService(
					_stateHolder(),
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IList<IDayOffOptimizerContainer> optimizerContainers = new List<IDayOffOptimizerContainer>();

			for (int index = 0; index < matrixContainerList.Count; index++)
			{
				IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixContainerList[index];
				IScheduleMatrixPro matrix = matrixContainerList[index].ScheduleMatrix;
				IScheduleResultDataExtractor personalSkillsDataExtractor =
					_optimizerHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
				IPeriodValueCalculator localPeriodValueCalculator =
					_optimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);
				IDayOffOptimizerContainer optimizerContainer =
					createOptimizer(matrix, optimizerPreferences.DaysOff, optimizerPreferences,
						rollbackService, dayOffTemplate, scheduleService, localPeriodValueCalculator,
						rollbackServiceDayOffConflict, matrixOriginalStateContainer);
				optimizerContainers.Add(optimizerContainer);
			}

			IScheduleResultDataExtractor allSkillsDataExtractor =
				_optimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder());
			IPeriodValueCalculator periodValueCalculator =
				_optimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IDayOffOptimizationService service = new DayOffOptimizationService(periodValueCalculator);
			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute(optimizerContainers);
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		
		public IEditableShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IWorkShiftFinderService finderService)
		{
			if (schedulePart == null)
				throw new ArgumentNullException("schedulePart");
			if (schedulingOptions == null)
				throw new ArgumentNullException("schedulingOptions");
			if (finderService == null)
				throw new ArgumentNullException("finderService");

			DateTime scheduleDayUtc = schedulePart.Period.StartDateTime;
			TimeZoneInfo timeZoneInfo = schedulePart.Person.PermissionInformation.DefaultTimeZone();
			var scheduleDateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(scheduleDayUtc, timeZoneInfo).Date);
			IPersonPeriod personPeriod = schedulePart.Person.Period(scheduleDateOnlyPerson);
			if (personPeriod != null)
			{
				//only fixed staff will be scheduled this way
				if (personPeriod.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)
					if (!schedulePart.IsScheduled())
					{
						DateTime schedulingTime = DateTime.Now;
						WorkShiftFinderServiceResult cache;
						using (PerformanceOutput.ForOperation("Finding the best shift"))
						{
							IScheduleMatrixPro matrix = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(
								new List<IScheduleDay> { schedulePart })[0];

							var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
							var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(
								schedulePart, schedulingOptions);
							cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction, null);
						}
						var result = cache.FinderResult;
						_allResults().AddResults(new List<IWorkShiftFinderResult> { result }, schedulingTime);

						if (cache.ResultHolder == null)
							return null;

						result.Successful = true;
						return cache.ResultHolder.ShiftProjection.TheMainShift;
					}
			}
			return null;
		}

		public IWorkShiftFinderResultHolder WorkShiftFinderResultHolder
		{
			get { return _allResults(); }
		}

		public void ResetWorkShiftFinderResults()
		{
			_allResults().Clear();
		}

		public void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
			ISchedulerStateHolder schedulerStateHolder,
			IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes)
		{
			if (matrixList == null) throw new ArgumentNullException("matrixList");
			if (schedulerStateHolder == null) throw new ArgumentNullException("schedulerStateHolder");
			if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			foreach (IScheduleMatrixPro scheduleMatrix in matrixList)
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(),
						new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro =
					_optimizerHelper.CreateWorkShiftBackToLegalStateServicePro(_container);
				workShiftBackToLegalStateServicePro.Execute(scheduleMatrix, schedulingOptions, schedulePartModifyAndRollbackService);

				backgroundWorker.ReportProgress(1);
			}

			if (optimizerPreferences.General.UseShiftCategoryLimitations)
			{
				_requiredScheduleHelper.RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker, optimizerPreferences, schedulingOptions,
					selectedPeriod, allMatrixes);
			}
		}

		public void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IBackgroundWorkerWrapper backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			bool reschedule,
			ISchedulingOptions schedulingOptions,
			IDaysOffPreferences daysOffPreferences)
		{
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			_allResults = ()=>new WorkShiftFinderResultHolder();
			_backgroundWorker = backgroundWorker;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers =
				_optimizerHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, daysOffPreferences, scheduleMatrixLockableBitArrayConverterEx);

			using (PerformanceOutput.ForOperation("SmartSolver for " + solverContainers.Count + " containers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					backToLegalStateSolverContainer.Execute();
					//create list to send to bruteforce
					if (!backToLegalStateSolverContainer.Result)
					{
						backToLegalStateSolverContainer.MatrixOriginalStateContainer.StillAlive = false;
						IWorkShiftFinderResult workShiftFinderResult =
							new WorkShiftFinderResult(backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix.Person,
								DateOnly.Today) { Successful = false };
						foreach (string descriptionKey in backToLegalStateSolverContainer.FailedSolverDescriptionKeys)
						{
							string localizedText = Resources.ResourceManager.GetString(descriptionKey);
							IWorkShiftFilterResult workShiftFilterResult =
								new WorkShiftFilterResult(localizedText, 0, 0);
							workShiftFinderResult.AddFilterResults(workShiftFilterResult);
						}
						WorkShiftFinderResultHolder.AddResults(new List<IWorkShiftFinderResult> { workShiftFinderResult }, DateTime.Now);
					}
				}
			}

			using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					if (backToLegalStateSolverContainer.Result)
					{
						_optimizerHelper.SyncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							dayOffTemplate,
							daysOffPreferences,
							_scheduleDayChangeCallback(),
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling),
							scheduleMatrixLockableBitArrayConverterEx);

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer);

						var optimizationLimits = new OptimizationLimits(optimizationOverLimitByRestrictionDecider, _container.Resolve<IMinWeekWorkTimeRule>());
						var overLimitCounts = optimizationLimits.OverLimitsCounts(matrix);


						if (overLimitCounts.AvailabilitiesOverLimit > 0 || overLimitCounts.MustHavesOverLimit > 0 ||
						    overLimitCounts.PreferencesOverLimit > 0 || overLimitCounts.RotationsOverLimit > 0 ||
						    overLimitCounts.StudentAvailabilitiesOverLimit > 0 ||
						    optimizationLimits.MoveMaxDaysOverLimit())
						{
							var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(),
								new ScheduleTagSetter(
									KeepOriginalScheduleTag.Instance));
							rollbackMatrixChanges(originalStateContainer, rollbackService);
						}
					}
				}
			}

			if (reschedule)
			{
				//call backtolegal
				//reschedule blank days
			}

		}

		public void ReOptimize(IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulingOptions schedulingOptions)
		{
			_backgroundWorker = backgroundWorker;
			_progressEvent = null;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
			
			var selectedPeriod = _optimizerHelperHelper.GetSelectedPeriod(selectedDays);

			optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
			var tagSetter = _container.Resolve<IScheduleTagSetter>();
			tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);
			IList<IPerson> selectedPersons =
				new List<IPerson>(new PersonListExtractorFromScheduleParts(selectedDays).ExtractPersons());
			IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization =
				_container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForDayOffOptimization =
				_container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
			IList<IScheduleMatrixPro> matrixListForIntradayOptimization =
				_container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);

			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization =
				createMatrixContainerList(matrixListForWorkShiftOptimization);
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization =
				createMatrixContainerList(matrixListForDayOffOptimization);
			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization =
				createMatrixContainerList(matrixListForIntradayOptimization);

			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForMoveMax =
				createMatrixContainerList(matrixListForIntradayOptimization);

			_optimizerHelper.SetConsiderShortBreaks(selectedPersons, selectedPeriod, optimizerPreferences.Rescheduling,
				_container);

			using (PerformanceOutput.ForOperation("Optimizing " + matrixListForWorkShiftOptimization.Count + " matrixes"))
			{
				var continuedStep = false;
				if (optimizerPreferences.General.OptimizationStepDaysOff)
				{
					runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization,
						selectedPeriod);
					continuedStep = true;

				}

				IList<IScheduleMatrixPro> matrixListForWorkShiftAndIntradayOptimization =
					_container.Resolve<IMatrixListFactory>().CreateMatrixList(selectedDays, selectedPeriod);
				IList<IScheduleMatrixOriginalStateContainer>
					workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization =
						createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization);

				if (optimizerPreferences.General.OptimizationStepTimeBetweenDays)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						RunWorkShiftOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForWorkShiftOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							selectedPeriod,
							_backgroundWorker);
						continuedStep = true;
					}
				}

				if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						_extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
							selectedDays, _stateHolder(),
							selectedPeriod,
							matrixOriginalStateContainerListForMoveMax);
						continuedStep = true;
					}
				}

				if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						_extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
							selectedDays, _schedulerStateHolder(),
							selectedPeriod,
							matrixOriginalStateContainerListForMoveMax);
						continuedStep = true;
					}

				}

				if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						RunIntradayOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForIntradayOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							backgroundWorker,
							selectedPeriod);
						continuedStep = true;
					}
				}

				if (optimizerPreferences.General.OptimizationStepFairness)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						runFairness(tagSetter, selectedPersons, schedulingOptions, selectedPeriod, optimizerPreferences);
						continuedStep = true;
					}
				}

				if (optimizerPreferences.General.OptimizationStepIntraInterval)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);
					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						runIntraInterval(schedulingOptions, optimizerPreferences, selectedPeriod, selectedDays, tagSetter);
					}
				}

			}

			//set back
			optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = onlyShiftsWhenUnderstaffed;
		}


		private void recalculateIfContinuedStep(bool continuedStep, DateOnlyPeriod selectedPeriod)
		{
			if (continuedStep)
			{
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
				}
			}
		}

		private void runFairness(IScheduleTagSetter tagSetter, IList<IPerson> selectedPersons,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences)
		{
			var matrixListForFairness = _container.Resolve<IMatrixListFactory>().CreateMatrixListAll(selectedPeriod);
			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForFairness, optimizationPreferences, selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), new DoNothingScheduleDayChangeCallBack(), tagSetter);

			var equalNumberOfCategoryFairnessService = _container.Resolve<IEqualNumberOfCategoryFairnessService>();
			equalNumberOfCategoryFairnessService.ReportProgress += resourceOptimizerPersonOptimized;
			equalNumberOfCategoryFairnessService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
				schedulingOptions, _schedulerStateHolder().Schedules, rollbackService,
				optimizationPreferences, true, true);
			equalNumberOfCategoryFairnessService.ReportProgress -= resourceOptimizerPersonOptimized;

			if (!_toggleManager.IsEnabled(Toggles.Scheduler_Seniority_24331)) return;

			////day off fairness
			var teamBlockDayOffFairnessOptimizationService = _container.Resolve<ITeamBlockDayOffFairnessOptimizationServiceFacade>();
			teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockDayOffFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions,
				_schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, true, _stateHolder().SeniorityWorkDayRanks);
			teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;

			var teamBlockSeniorityFairnessOptimizationService = _container.Resolve<ITeamBlockSeniorityFairnessOptimizationService>();
			teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockSeniorityFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions, _stateHolder().ShiftCategories.ToList(), _schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, true);
			teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void runIntraInterval(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedDays, IScheduleTagSetter tagSetter)
		{
			var args = new ResourceOptimizerProgressEventArgs(0, 0, LanguageResourceHelper.Translate("XXCollectingData"));
			_backgroundWorker.ReportProgress(1, args);
			var allMatrixes = _container.Resolve<IMatrixListFactory>().CreateMatrixListAll(selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);
			var intraIntervalOptimizationCommand = _container.Resolve<IIntraIntervalOptimizationCommand>();
			intraIntervalOptimizationCommand.Execute(schedulingOptions, optimizationPreferences, selectedPeriod, selectedDays, _schedulerStateHolder().SchedulingResultState, allMatrixes, rollbackService, resourceCalculateDelayer, _backgroundWorker);
		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
			IList<IScheduleMatrixOriginalStateContainer> result =
				matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}

		internal void RunIntradayOptimization(
			IOptimizationPreferences optimizerPreferences,
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
			IBackgroundWorkerWrapper backgroundWorker,
			DateOnlyPeriod selectedPeriod)
		{
			_backgroundWorker = backgroundWorker;
			using (PerformanceOutput.ForOperation("Running new intraday optimization"))
			{
				if (_backgroundWorker.CancellationPending)
					return;

				if (_progressEvent != null && _progressEvent.Cancel) return;

				IList<IScheduleMatrixPro> matrixList =
					matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

				_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixList, selectedPeriod);

				optimizeIntraday(matrixContainerList, workShiftContainerList, optimizerPreferences, selectedPeriod);
			}
		}

		internal void RunWorkShiftOptimization(
			IOptimizationPreferences optimizerPreferences,
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workshiftOriginalStateContainerList,
			DateOnlyPeriod selectedPeriod,
			IBackgroundWorkerWrapper backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
			using (PerformanceOutput.ForOperation("Running move time optimization"))
			{
				if (_backgroundWorker.CancellationPending)
					return;

				if (_progressEvent != null && _progressEvent.Cancel) return;

				IList<IScheduleMatrixPro> matrixList =
					scheduleMatrixOriginalStateContainerList.Select(container => container.ScheduleMatrix).ToList();

				_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixList, selectedPeriod);

				optimizeWorkShifts(scheduleMatrixOriginalStateContainerList, workshiftOriginalStateContainerList, optimizerPreferences, selectedPeriod);

				// we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
				ISchedulePartModifyAndRollbackService rollbackService =
					new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				foreach (
					IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer in scheduleMatrixOriginalStateContainerList)
				{
					if (!matrixOriginalStateContainer.IsFullyScheduled())
					{

						rollbackMatrixChanges(matrixOriginalStateContainer, rollbackService);
					}

				}
			}
		}

		private void runDayOffOptimization(IOptimizationPreferences optimizerPreferences,
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod)
		{
			if (_backgroundWorker.CancellationPending)
				return;

			if (_progressEvent != null && _progressEvent.Cancel) return;

			IList<IScheduleMatrixPro> matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container.Resolve<IOptimizationPreferences>(), selectedPeriod);

			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
			resourceOptimizerPersonOptimized(this, e);

			// to make sure we are in legal state before we can do day off optimization
			IList<IDayOffTemplate> displayList = _schedulerStateHolder().CommonStateHolder.ActiveDayOffs.ToList();
			((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
			DaysOffBackToLegalState(matrixContainerList, _backgroundWorker,
				displayList[0], false, schedulingOptions, optimizerPreferences.DaysOff);

			var workShiftBackToLegalStateService =
				_optimizerHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var matrixOriginalStateContainer in matrixContainerList)
			{
				rollbackService.ClearModificationCollection();
				workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions,
					rollbackService);
			}

			e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.Rescheduling + Resources.ThreeDots);
			resourceOptimizerPersonOptimized(this, e);

			// Schedule White Spots after back to legal state
			var scheduleService = _container.Resolve<IScheduleService>();

			// schedule those are the white spots after back to legal state
			_optimizerHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container, rollbackService);


			bool notFullyScheduledMatrixFound = false;
			IList<IScheduleMatrixOriginalStateContainer> validMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer>();
			foreach (IScheduleMatrixOriginalStateContainer matrixContainer in matrixContainerList)
			{
				bool isFullyScheduled = matrixContainer.IsFullyScheduled();
				if (!isFullyScheduled)
				{
					notFullyScheduledMatrixFound = true;
					rollbackMatrixChanges(matrixContainer, rollbackService);
					continue;
				}
				validMatrixContainerList.Add(matrixContainer);
			}

			if (notFullyScheduledMatrixFound)
			{
				foreach (var dateOnly in selectedPeriod.DayCollection())
				{
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, optimizerPreferences.Rescheduling.ConsiderShortBreaks);
				}
			}

			optimizeDaysOff(validMatrixContainerList,
				displayList[0],
				selectedPeriod,
				scheduleService);

			// we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
			rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			foreach (IScheduleMatrixOriginalStateContainer matrixContainer in validMatrixContainerList)
			{
				if (!matrixContainer.IsFullyScheduled())
					rollbackMatrixChanges(matrixContainer, rollbackService);
			}
		}

		private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var cancel = false;
			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name,()=>cancel=true);
			resourceOptimizerPersonOptimized(this, e);
			if (cancel || (_progressEvent != null && _progressEvent.Cancel)) return;

			rollbackService.ClearModificationCollection();
			foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
			{
				IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
				rollbackService.Modify(originalPart);
			}
		}

		private void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
				e.CancelAction();
			}
			_backgroundWorker.ReportProgress(1, e);

			if (_progressEvent != null && _progressEvent.Cancel) return;
			_progressEvent = e;
		}

		public IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			IList<IScheduleMatrixOriginalStateContainer> retList = new List<IScheduleMatrixOriginalStateContainer>();
			var scheduleDayEquator = _container.Resolve<IScheduleDayEquator>();
			foreach (IScheduleMatrixPro scheduleMatrixPro in _container.Resolve<IMatrixListFactory>().CreateMatrixList(scheduleDays, selectedPeriod))
				retList.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, scheduleDayEquator));

			return retList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private IDayOffOptimizerContainer createOptimizer(
			IScheduleMatrixPro scheduleMatrix,
			IDaysOffPreferences daysOffPreferences,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IDayOffTemplate dayOffTemplate,
			IScheduleService scheduleService,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict,
			IScheduleMatrixOriginalStateContainer originalStateContainer)
		{
			IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateService =
				_optimizerHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

			ILockableBitArray scheduleMatrixArray = _bitArrayConverter.Convert(scheduleMatrix, daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter);

			// create decisionmakers
			var decisionMakers = _dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences);
			var scheduleResultDataExtractor = _optimizerHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, scheduleMatrix);

			ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService =
				new SmartDayOffBackToLegalStateService(
					_container.Resolve<IDayOffBackToLegalStateFunctions>(),
					daysOffPreferences,
					25,
					_container.Resolve<IDayOffDecisionMaker>());

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, true);

			var dayOffOptimizerConflictHandler = new DayOffOptimizerConflictHandler(scheduleMatrix, scheduleService,
				_container.Resolve<IEffectiveRestrictionCreator>(),
				rollbackServiceDayOffConflict,
				resourceCalculateDelayer);

			var restrictionChecker = new RestrictionChecker();
			var optimizationUserPreferences = _container.Resolve<IOptimizationPreferences>();
			var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizationUserPreferences, originalStateContainer);

			var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider, _container.Resolve<IMinWeekWorkTimeRule>());

			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_stateHolder), resourceOptimizationHelper);
			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
				new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
					deleteAndResourceCalculateService,
					scheduleService, ()=>WorkShiftFinderResultHolder,
					resourceCalculateDelayer);
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();
			var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(_stateHolder);
			var deviationStatisticData = new DeviationStatisticData();
			var dayOffOptimizerPreMoveResultPredictor =
				new DayOffOptimizerPreMoveResultPredictor(dailySkillForecastAndScheduledValueCalculator, deviationStatisticData);

			IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter
				= new DayOffDecisionMakerExecuter(rollbackService,
					dayOffBackToLegalStateService,
					dayOffTemplate,
					scheduleService,
					optimizerPreferences,
					periodValueCalculatorForAllSkills,
					workShiftBackToLegalStateService,
					_container.Resolve<IEffectiveRestrictionCreator>(),
					_resourceOptimizationHelper,
					new ResourceCalculateDaysDecider(),
					_container.Resolve<IDayOffOptimizerValidator>(),
					dayOffOptimizerConflictHandler,
					originalStateContainer,
					optimizationLimits,
					nightRestWhiteSpotSolverService,
					_container.Resolve<ISchedulingOptionsCreator>(),
					mainShiftOptimizeActivitySpecificationSetter,
					dayOffOptimizerPreMoveResultPredictor);

			IDayOffOptimizerContainer optimizerContainer =
				new DayOffOptimizerContainer(_bitArrayConverter,
					decisionMakers,
					scheduleResultDataExtractor,
					daysOffPreferences,
					scheduleMatrix,
					dayOffDecisionMakerExecuter,
					originalStateContainer);
			return optimizerContainer;
		}
	}
}