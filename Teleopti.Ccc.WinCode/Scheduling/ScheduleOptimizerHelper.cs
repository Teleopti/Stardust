using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
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
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ScheduleOptimizerHelper : IScheduleOptimizerHelper
	{
		private Func<IWorkShiftFinderResultHolder> _allResults;
		private ISchedulingProgress _backgroundWorker;
		private readonly ILifetimeScope _container;
		private readonly OptimizerHelperHelper _optimizerHelper;
		private readonly IRequiredScheduleHelper _requiredScheduleHelper;
		private readonly IExtendReduceTimeHelper _extendReduceTimeHelper;
		private readonly IExtendReduceDaysOffHelper _extendReduceDaysOffHelper;
		private readonly Func<ISchedulingResultStateHolder> _stateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private ResourceOptimizerProgressEventArgs _progressEvent;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _bitArrayConverter;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly Func<IPersonSkillProvider> _personalSkillProvider;
		private readonly VirtualSkillContext _virtualSkillContext;

		public ScheduleOptimizerHelper(ILifetimeScope container, OptimizerHelperHelper optimizerHelper, IRequiredScheduleHelper requiredScheduleHelper)
		{
			_container = container;
			_optimizerHelper = optimizerHelper;
			_requiredScheduleHelper = requiredScheduleHelper; 
			_allResults = () => _container.Resolve<IWorkShiftFinderResultHolder>();
			_extendReduceTimeHelper = new ExtendReduceTimeHelper(_container);
			_extendReduceDaysOffHelper = new ExtendReduceDaysOffHelper(_container,optimizerHelper,_allResults);
			_schedulerStateHolder = () => _container.Resolve<ISchedulerStateHolder>(); 
			_stateHolder = () => _schedulerStateHolder().SchedulingResultState;
			_scheduleDayChangeCallback = ()=>_container.Resolve<IScheduleDayChangeCallback>();
			_resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			_optimizerHelperHelper = _container.Resolve<IOptimizerHelperHelper>();
			_bitArrayConverter = _container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();
			_matrixListFactory = _container.Resolve<IMatrixListFactory>();
			_personalSkillProvider = () => _container.Resolve<IPersonSkillProvider>();
			_virtualSkillContext = _container.Resolve<VirtualSkillContext>();
		}

		private void optimizeIntraday(IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					_stateHolder(),
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			var decisionMaker = new IntradayDecisionMaker(_bitArrayConverter);
			var scheduleService = _container.Resolve<IScheduleService>();

			IIntradayOptimizer2Creator creator = new IntradayOptimizer2Creator(
				decisionMaker,
				scheduleService,
				_container.Resolve<ISkillStaffPeriodToSkillIntervalDataMapper>(),
				_container.Resolve<ISkillIntervalDataDivider>(),
				_container.Resolve<ISkillIntervalDataAggregator>(),
				_container.Resolve<IEffectiveRestrictionCreator>(),
				_container.Resolve<IMinWeekWorkTimeRule>(),
				_container.Resolve<IResourceOptimizationHelper>(),
				_container.Resolve<IDeleteAndResourceCalculateService>());

			var optimizers = creator.Create(matrixContainerList,
				workShiftContainerList, optimizerPreferences,
				rollbackService,
				dayOffOptimizationPreferenceProvider);
			var service = new IntradayOptimizerContainer();

			using (_virtualSkillContext.Create(selectedPeriod))
			{
				using (createResourceCalculationContext())
				{
					service.ReportProgress += resourceOptimizerPersonOptimized;
					service.Execute(optimizers, selectedPeriod);
					service.ReportProgress -= resourceOptimizerPersonOptimized;
				}
			}
		}

		private void optimizeWorkShifts(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerList,
			IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
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
					_container.Resolve<IResourceOptimizationHelper>(),
					dayOffOptimizationPreferenceProvider,
					_container.Resolve<IDeleteAndResourceCalculateService>());

			IList<IMoveTimeOptimizer> optimizers = creator.Create();
			IScheduleOptimizationService service = new MoveTimeOptimizerContainer(optimizers, periodValueCalculator);

			using (createResourceCalculationContext())
			{
				service.ReportProgress += resourceOptimizerPersonOptimized;
				service.Execute();
				service.ReportProgress -= resourceOptimizerPersonOptimized;
			}
		}

		private ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>
			createResourceCalculationContext()
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var minutesPerInterval = 15;
			if (schedulerStateHolder.SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}
			var extractor = new ScheduleProjectionExtractor(_personalSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
			return new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources);
		}

		private void classicDaysOffOptimization(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod, ISchedulingProgress schedulingProgress)
		{
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var dayOffOptimzePreferences = _container.Resolve<IDaysOffPreferences>();
			var dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(dayOffOptimzePreferences);
			var classicDaysOffOptimizationCommand = _container.Resolve<ClassicDaysOffOptimizationCommand>();
			classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod, optimizerPreferences, _schedulerStateHolder(), schedulingProgress, dayOffOptimizationPreferenceProvider);
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

			var scheduleDateOnlyPerson = schedulePart.DateOnlyAsPeriod.DateOnly;
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
							IScheduleMatrixPro matrix =
								_matrixListFactory.CreateMatrixListForSelection(new List<IScheduleDay> {schedulePart})[0];

							var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
							var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(
								schedulePart, schedulingOptions);
							cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction);
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
			ISchedulingProgress backgroundWorker,
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
			ISchedulingProgress backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			bool reschedule,
			ISchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			_allResults = ()=>new WorkShiftFinderResultHolder();
			_backgroundWorker = backgroundWorker;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers =
				_optimizerHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, scheduleMatrixLockableBitArrayConverterEx, dayOffOptimizationPreferenceProvider);

			using (PerformanceOutput.ForOperation("SmartSolver for " + solverContainers.Count + " containers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
					var dayOffOptimizePreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,matrix.EffectivePeriodDays.First().Day);

					backToLegalStateSolverContainer.Execute(dayOffOptimizePreference);

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
							_scheduleDayChangeCallback(),
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling),
							scheduleMatrixLockableBitArrayConverterEx,
							_schedulerStateHolder().SchedulingResultState,
							dayOffOptimizationPreferenceProvider
							);

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;

						var dayOffOptimizePrefrerence = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,matrix.EffectivePeriodDays.First().Day);

						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, dayOffOptimizePrefrerence);

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

		public void ReOptimize(ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedDays,
			ISchedulingOptions schedulingOptions, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_backgroundWorker = backgroundWorker;
			_progressEvent = null;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

			var selectedPeriod = _container.Resolve<PeriodExctractorFromScheduleParts>().ExtractPeriod(selectedDays).Value;

			optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
			var tagSetter = _container.Resolve<IScheduleTagSetter>();
			tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);
			IList<IPerson> selectedPersons =
				_container.Resolve<IPersonListExtractorFromScheduleParts>().ExtractPersons(selectedDays);
	
			IList<IScheduleMatrixPro> matrixListForIntradayOptimization =
				_container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);

			_optimizerHelper.SetConsiderShortBreaks(selectedPersons, selectedPeriod, optimizerPreferences.Rescheduling,
				_container);

			using (PerformanceOutput.ForOperation("Optimizing"))
			{
				var continuedStep = false;
				if (optimizerPreferences.General.OptimizationStepDaysOff)
				{
					IList<IScheduleMatrixPro> matrixListForDayOffOptimization =
						_container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);
					IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization =
						createMatrixContainerList(matrixListForDayOffOptimization);

					runDayOffOptimization(optimizerPreferences, matrixOriginalStateContainerListForDayOffOptimization,
						selectedPeriod, dayOffOptimizationPreferenceProvider);
					continuedStep = true;

				}

				IList<IScheduleMatrixPro> matrixListForWorkShiftAndIntradayOptimization =
					_container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);
				IList<IScheduleMatrixOriginalStateContainer>
					workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization =
						createMatrixContainerList(matrixListForWorkShiftAndIntradayOptimization);

				if (optimizerPreferences.General.OptimizationStepTimeBetweenDays)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						IList<IScheduleMatrixPro> matrixListForWorkShiftOptimization =
							_container.Resolve<IMatrixListFactory>().CreateMatrixListForSelection(selectedDays);
						IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForWorkShiftOptimization =
							createMatrixContainerList(matrixListForWorkShiftOptimization);

						RunWorkShiftOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForWorkShiftOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							selectedPeriod,
							_backgroundWorker,
							dayOffOptimizationPreferenceProvider);
						continuedStep = true;
					}
				}

				continuedStep = runFlexibleTime(optimizerPreferences, continuedStep, selectedPeriod, selectedDays,
					dayOffOptimizationPreferenceProvider, matrixListForIntradayOptimization);

				if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForIntradayOptimization =
							createMatrixContainerList(matrixListForIntradayOptimization);

						RunIntradayOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForIntradayOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							backgroundWorker,
							selectedPeriod,
							dayOffOptimizationPreferenceProvider);
						continuedStep = true;
					}
				}

				if (optimizerPreferences.General.OptimizationStepFairness)
				{
					recalculateIfContinuedStep(continuedStep, selectedPeriod);

					if (_progressEvent == null || !_progressEvent.Cancel)
					{
						runFairness(tagSetter, selectedPersons, schedulingOptions, selectedPeriod, optimizerPreferences, dayOffOptimizationPreferenceProvider);
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


		private bool runFlexibleTime(IOptimizationPreferences optimizerPreferences, bool continuedStep,
			DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedDays,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IEnumerable<IScheduleMatrixPro> matrixListForIntradayOptimization)
		{
			bool runned = false;
			if (!optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime ||
				optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
				return false;


			IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForMoveMax =
				createMatrixContainerList(matrixListForIntradayOptimization);

			if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
			{
				recalculateIfContinuedStep(continuedStep, selectedPeriod);

				if (_progressEvent == null || !_progressEvent.Cancel)
				{
					using (createResourceCalculationContext())
					{
						_extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
						selectedDays, _stateHolder(),
						selectedPeriod,
						matrixOriginalStateContainerListForMoveMax, dayOffOptimizationPreferenceProvider);
					}
					
					runned = true;
				}
			}

			if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
			{
				recalculateIfContinuedStep(continuedStep, selectedPeriod);

				if (_progressEvent == null || !_progressEvent.Cancel)
				{
					using (createResourceCalculationContext())
					{
						_extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
							selectedDays, _schedulerStateHolder(),
							selectedPeriod,
							matrixOriginalStateContainerListForMoveMax,
							dayOffOptimizationPreferenceProvider);
					}
					runned = true;
				}
			}

			return runned;
		}

		private void recalculateIfContinuedStep(bool continuedStep, DateOnlyPeriod selectedPeriod)
		{
			if (continuedStep)
			{
				using (createResourceCalculationContext())
				{
					foreach (var dateOnly in selectedPeriod.DayCollection())
					{
						_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, false);
					}
				}
			}
		}

		private void runFairness(IScheduleTagSetter tagSetter, IList<IPerson> selectedPersons,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, 
			IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var matrixListForFairness = _container.Resolve<IMatrixListFactory>().CreateMatrixListAllForLoadedPeriod(selectedPeriod);
			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForFairness, optimizationPreferences, selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), new DoNothingScheduleDayChangeCallBack(), tagSetter);

			var equalNumberOfCategoryFairnessService = _container.Resolve<IEqualNumberOfCategoryFairnessService>();
			equalNumberOfCategoryFairnessService.ReportProgress += resourceOptimizerPersonOptimized;
			equalNumberOfCategoryFairnessService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
				schedulingOptions, _schedulerStateHolder().Schedules, rollbackService,
				optimizationPreferences, dayOffOptimizationPreferenceProvider);
			equalNumberOfCategoryFairnessService.ReportProgress -= resourceOptimizerPersonOptimized;
			var teamBlockDayOffFairnessOptimizationService = _container.Resolve<ITeamBlockDayOffFairnessOptimizationServiceFacade>();
			teamBlockDayOffFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockDayOffFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions,
				_schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, _stateHolder().SeniorityWorkDayRanks, dayOffOptimizationPreferenceProvider);
			teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;

			var teamBlockSeniorityFairnessOptimizationService = _container.Resolve<ITeamBlockSeniorityFairnessOptimizationService>();
			teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockSeniorityFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions, _stateHolder().ShiftCategories.ToList(), 
																_schedulerStateHolder().Schedules, rollbackService, optimizationPreferences, true, dayOffOptimizationPreferenceProvider);
			teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void runIntraInterval(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedDays, IScheduleTagSetter tagSetter)
		{
			var args = new ResourceOptimizerProgressEventArgs(0, 0, LanguageResourceHelper.Translate("XXCollectingData"));
			_backgroundWorker.ReportProgress(1, args);
			var allMatrixes = _container.Resolve<IMatrixListFactory>().CreateMatrixListAllForLoadedPeriod(selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);
			var intraIntervalOptimizationCommand = _container.Resolve<IIntraIntervalOptimizationCommand>();
			using (createResourceCalculationContext())
			{
				intraIntervalOptimizationCommand.Execute(optimizationPreferences, selectedPeriod, selectedDays, _schedulerStateHolder().SchedulingResultState, allMatrixes, rollbackService, resourceCalculateDelayer, _backgroundWorker);
			}
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
			ISchedulingProgress backgroundWorker,
			DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
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

				optimizeIntraday(matrixContainerList, workShiftContainerList, optimizerPreferences, selectedPeriod, dayOffOptimizationPreferenceProvider);
			}
		}

		internal void RunWorkShiftOptimization(
			IOptimizationPreferences optimizerPreferences,
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workshiftOriginalStateContainerList,
			DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
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

				optimizeWorkShifts(scheduleMatrixOriginalStateContainerList, workshiftOriginalStateContainerList, optimizerPreferences, selectedPeriod, dayOffOptimizationPreferenceProvider);

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
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList, DateOnlyPeriod selectedPeriod,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
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
				displayList[0], false, schedulingOptions, dayOffOptimizationPreferenceProvider);

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
			rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder(), _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
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
					_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, optimizerPreferences.Rescheduling.ConsiderShortBreaks, false);
				}
			}

			classicDaysOffOptimization(validMatrixContainerList, selectedPeriod, _backgroundWorker);

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
			foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixListForSelection(scheduleDays))
				retList.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, scheduleDayEquator));

			return retList;
		}	
	}
}