using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Secret;
using Teleopti.Ccc.Domain.Secret.DayOffPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class ScheduleOptimizerHelper
	{
		private IWorkShiftFinderResultHolder _allResults;
		private BackgroundWorker _backgroundWorker;
		private int _scheduledCount;
		private int _sendEventEvery;
		private readonly ILifetimeScope _container;
		private readonly IExtendReduceTimeHelper _extendReduceTimeHelper;
		private readonly IExtendReduceDaysOffHelper _extendReduceDaysOffHelper;
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGridlockManager _gridlockManager;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IDaysOffSchedulingService _daysOffSchedulingService;
		private readonly IPersonSkillProvider _personSkillProvider;

		public ScheduleOptimizerHelper(ILifetimeScope container)
		{
			_container = container;
			_extendReduceTimeHelper = new ExtendReduceTimeHelper(_container);
			_extendReduceDaysOffHelper = new ExtendReduceDaysOffHelper(_container);
			_stateHolder = _container.Resolve<ISchedulingResultStateHolder>();
			_schedulerStateHolder = _container.Resolve<ISchedulerStateHolder>();
			_scheduleDayChangeCallback = _container.Resolve<IScheduleDayChangeCallback>();
			_gridlockManager = _container.Resolve<IGridlockManager>();
			_allResults = _container.Resolve<IWorkShiftFinderResultHolder>();
			_resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			_scheduleMatrixListCreator = _container.Resolve<IScheduleMatrixListCreator>();
			_daysOffSchedulingService = _container.Resolve<IDaysOffSchedulingService>();
			_personSkillProvider = _container.Resolve<IPersonSkillProvider>();
		}

		private void optimizeIntraday(
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
			IOptimizationPreferences optimizerPreferences)
		{
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					_stateHolder,
					_scheduleDayChangeCallback,
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IIntradayDecisionMaker decisionMaker = new IntradayDecisionMaker();
			var scheduleService = _container.Resolve<IScheduleService>();

			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

			IIntradayOptimizer2Creator creator = new IntradayOptimizer2Creator(
				matrixContainerList,
				workShiftContainerList,
				decisionMaker,
				scheduleService,
				optimizerPreferences,
				rollbackService,
				_stateHolder,
				_personSkillProvider, 
				new CurrentTeleoptiPrincipal(),
				scheduleMatrixLockableBitArrayConverterEx,
				_container.Resolve<ISkillStaffPeriodToSkillIntervalDataMapper>(),
				_container.Resolve<ISkillIntervalDataDivider>(),
				_container.Resolve<ISkillIntervalDataAggregator>());

			IList<IIntradayOptimizer2> optimizers = creator.Create();
			IScheduleOptimizationService service = new IntradayOptimizerContainer(optimizers);

			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute();
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		private void optimizeWorkShifts(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftOriginalStateContainerList,
			IOptimizationPreferences optimizerPreferences,
			DateOnlyPeriod selectedPeriod)
		{

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
				                                         new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IScheduleResultDataExtractor allSkillsDataExtractor =
				OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder);
			IPeriodValueCalculator periodValueCalculator =
				OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IMoveTimeDecisionMaker decisionMaker = new MoveTimeDecisionMaker2();

			var scheduleService = _container.Resolve<IScheduleService>();

			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

			IMoveTimeOptimizerCreator creator =
				new MoveTimeOptimizerCreator(scheduleMatrixOriginalStateContainerList,
				                             workShiftOriginalStateContainerList,
				                             decisionMaker,
				                             scheduleService,
				                             optimizerPreferences,
				                             rollbackService,
				                             _stateHolder,
				                             _personSkillProvider, 
											 new CurrentTeleoptiPrincipal(),
											 scheduleMatrixLockableBitArrayConverterEx);

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
					_stateHolder,
					_scheduleDayChangeCallback,
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			ISchedulePartModifyAndRollbackService rollbackServiceDayOffConflict =
				new SchedulePartModifyAndRollbackService(
					_stateHolder,
					_scheduleDayChangeCallback,
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IList<IDayOffOptimizerContainer> optimizerContainers = new List<IDayOffOptimizerContainer>();

			for (int index = 0; index < matrixContainerList.Count; index++)
			{
				IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer = matrixContainerList[index];
				IScheduleMatrixPro matrix = matrixContainerList[index].ScheduleMatrix;
				IScheduleResultDataExtractor personalSkillsDataExtractor =
					OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, matrix);
				IPeriodValueCalculator localPeriodValueCalculator =
					OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);
				IDayOffOptimizerContainer optimizerContainer =
					createOptimizer(matrix, optimizerPreferences.DaysOff, optimizerPreferences,
					                rollbackService, dayOffTemplate, scheduleService, localPeriodValueCalculator,
					                rollbackServiceDayOffConflict, matrixOriginalStateContainer);
				optimizerContainers.Add(optimizerContainer);
			}

			IScheduleResultDataExtractor allSkillsDataExtractor =
				OptimizerHelperHelper.CreateAllSkillsDataExtractor(optimizerPreferences.Advanced, selectedPeriod, _stateHolder);
			IPeriodValueCalculator periodValueCalculator =
				OptimizerHelperHelper.CreatePeriodValueCalculator(optimizerPreferences.Advanced, allSkillsDataExtractor);

			IDayOffOptimizationService service = new DayOffOptimizationService(periodValueCalculator);
			service.ReportProgress += resourceOptimizerPersonOptimized;
			service.Execute(optimizerContainers);
			service.ReportProgress -= resourceOptimizerPersonOptimized;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, BackgroundWorker backgroundWorker,
		                                     ISchedulingOptions schedulingOptions)
		{
			if (allSelectedSchedules == null) throw new ArgumentNullException("allSelectedSchedules");
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			var optimizationPreferences = _container.Resolve<IOptimizationPreferences>();
			IList<IScheduleDay> unlockedSchedules = new List<IScheduleDay>();
			foreach (var scheduleDay in allSelectedSchedules)
			{
				GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay);
				if (locks == null || locks.Count == 0)
					unlockedSchedules.Add(scheduleDay);
			}

			if (unlockedSchedules.Count == 0)
			{
				return;
			}

			var selectedPersons = new PersonListExtractorFromScheduleParts(unlockedSchedules).ExtractPersons();
			var selectedPeriod = unlockedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());

			OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, period, optimizationPreferences.Rescheduling,
			                                             _container);

			var studentSchedulingService = _container.Resolve<IStudentSchedulingService>();
			var tagSetter = _container.Resolve<IScheduleTagSetter>();
			tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
			studentSchedulingService.ClearFinderResults();
			_backgroundWorker = backgroundWorker;
			studentSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			DateTime schedulingTime = DateTime.Now;
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
			                                                                                                 _scheduleDayChangeCallback,
			                                                                                                 new ScheduleTagSetter
				                                                                                                 (schedulingOptions
					                                                                                                  .
					                                                                                                  TagToUseOnScheduling));
			using (PerformanceOutput.ForOperation("Scheduling " + unlockedSchedules.Count))
			{
				studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, false, rollbackService);
			}

			_allResults.AddResults(studentSchedulingService.FinderResults, schedulingTime);
			studentSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "5"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
		                                       IList<IScheduleMatrixPro> matrixListAll, bool useOccupancyAdjustment,
		                                       BackgroundWorker backgroundWorker, ISchedulingOptions schedulingOptions)
		{
			if (matrixList == null) throw new ArgumentNullException("matrixList");

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

			var unlockedSchedules = (from scheduleMatrixPro in matrixList
			                         from scheduleDayPro in scheduleMatrixPro.UnlockedDays
			                         select scheduleDayPro.DaySchedulePart()).ToList();

			if (!unlockedSchedules.Any()) return;

			var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();

			var selectedPeriod = allSelectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());

			OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, period, schedulingOptions, _container);

			var stateHolder = _container.Resolve<ISchedulingResultStateHolder>();

			var scheduleTagSetter = _container.Resolve<IScheduleTagSetter>();
			scheduleTagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
			var fixedStaffSchedulingService = _container.Resolve<IFixedStaffSchedulingService>();


			fixedStaffSchedulingService.ClearFinderResults();
			_backgroundWorker = backgroundWorker;
			_sendEventEvery = schedulingOptions.RefreshRate;
			_scheduledCount = 0;
			fixedStaffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			DateTime schedulingTime = DateTime.Now;
			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_stateHolder), resourceOptimizationHelper);
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder,
			                                                                                                 _scheduleDayChangeCallback,
			                                                                                                 new ScheduleTagSetter
				                                                                                                 (schedulingOptions.
					                                                                                                  TagToUseOnScheduling));
			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
				new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
				                                    deleteAndResourceCalculateService,
				                                    _container.Resolve<IScheduleService>(), WorkShiftFinderResultHolder,
				                                    new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
				                                                                 schedulingOptions.ConsiderShortBreaks));

			using (PerformanceOutput.ForOperation(string.Concat("Scheduling ", unlockedSchedules.Count, " days")))
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff =
					new SchedulePartModifyAndRollbackService(stateHolder, _scheduleDayChangeCallback,
					                                         new ScheduleTagSetter(
						                                         schedulingOptions.TagToUseOnScheduling));
				_daysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceForContractDaysOff,
				                                  schedulingOptions);
				_daysOffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

				//lock none selected days
				var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, period);
				matrixUnselectedDaysLocker.Execute();

				unlockedSchedules = (from scheduleMatrixPro in matrixList
				                     from scheduleDayPro in scheduleMatrixPro.UnlockedDays
				                     select scheduleDayPro.DaySchedulePart()).ToList();

				if (!unlockedSchedules.Any())
					return;

				IList<IScheduleMatrixOriginalStateContainer> originalStateContainers =
					CreateScheduleMatrixOriginalStateContainers(allSelectedSchedules,
					                                            new DateOnlyPeriod(selectedPeriod.First(), selectedPeriod.Last()));

				foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
				{
					foreach (var day in scheduleMatrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
					{
						if (day.DaySchedulePart().IsScheduled())
							scheduleMatrixOriginalStateContainer.ScheduleMatrix.LockPeriod(new DateOnlyPeriod(day.Day, day.Day));
					}
				}


				var minutesPerInterval = 15;
				if (_schedulerStateHolder.SchedulingResultState.Skills.Count > 0)
				{
					minutesPerInterval = _schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
				}
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval);
				var resources = extractor.CreateRelevantProjectionList(_schedulerStateHolder.Schedules);
				using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
				{
					fixedStaffSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, useOccupancyAdjustment, false,
					                                            rollbackService);
					_allResults.AddResults(fixedStaffSchedulingService.FinderResults, schedulingTime);
					fixedStaffSchedulingService.FinderResults.Clear();

					var progressChangeEvent = new TeleoptiProgressChangeMessage(Resources.TryingToResolveUnscheduledDaysDotDotDot);
					_backgroundWorker.ReportProgress(0, progressChangeEvent);
					foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
					{
						int iterations = 0;
						while (
							nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix, schedulingOptions,
							                                        rollbackService) && iterations < 10)
						{
							if (_backgroundWorker.CancellationPending)
								break;
							iterations++;
						}
						if (_backgroundWorker.CancellationPending)
							break;
					}

					if (schedulingOptions.RotationDaysOnly || schedulingOptions.PreferencesDaysOnly ||
					    schedulingOptions.UsePreferencesMustHaveOnly || schedulingOptions.AvailabilityDaysOnly)
						schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();
				}
			}
			fixedStaffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
		}

		private void schedulingServiceDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			if(e.IsSuccessful)
			_scheduledCount++;
			if (_scheduledCount >= _sendEventEvery)
			{
				_backgroundWorker.ReportProgress(1, e.SchedulePart);
				_scheduledCount = 0;
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")
		]
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
						IWorkShiftCalculationResultHolder cache;
						using (PerformanceOutput.ForOperation("Finding the best shift"))
						{
							IScheduleMatrixPro matrix = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(
								new List<IScheduleDay> {schedulePart})[0];

							var effectiveRestrictionCreator = _container.Resolve<IEffectiveRestrictionCreator>();
							var effectiveRestriction = effectiveRestrictionCreator.GetEffectiveRestriction(
								schedulePart, schedulingOptions);
							cache = finderService.FindBestShift(schedulePart, schedulingOptions, matrix, effectiveRestriction, null);
						}
						var result = finderService.FinderResult;
						_allResults.AddResults(new List<IWorkShiftFinderResult> {result}, schedulingTime);

						if (cache == null)
							return null;

						result.Successful = true;
						return cache.ShiftProjection.TheMainShift;
					}
			}
			return null;
		}

		public IWorkShiftFinderResultHolder WorkShiftFinderResultHolder
		{
			get { return _allResults; }
		}

		public void ResetWorkShiftFinderResults()
		{
			_allResults.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "3")]
		public void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
		                                ISchedulerStateHolder schedulerStateHolder,
		                                BackgroundWorker backgroundWorker,
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
					new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
					                                         new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				IWorkShiftBackToLegalStateServicePro workShiftBackToLegalStateServicePro =
					OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);
				workShiftBackToLegalStateServicePro.Execute(scheduleMatrix, schedulingOptions, schedulePartModifyAndRollbackService);

				backgroundWorker.ReportProgress(1);
			}

			if (optimizerPreferences.General.UseShiftCategoryLimitations)
			{
				RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker, optimizerPreferences, schedulingOptions,
				                                    selectedPeriod, allMatrixes);
			}

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
		                                    BackgroundWorker backgroundWorker,
		                                    IDayOffTemplate dayOffTemplate,
		                                    bool reschedule,
		                                    ISchedulingOptions schedulingOptions,
		                                    IDaysOffPreferences daysOffPreferences)
		{
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			_allResults = new WorkShiftFinderResultHolder();
			_backgroundWorker = backgroundWorker;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();

			var scheduleMatrixLockableBitArrayConverterEx =
				_container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

			IList<ISmartDayOffBackToLegalStateSolverContainer> solverContainers =
				OptimizerHelperHelper.CreateSmartDayOffSolverContainers(matrixOriginalStateContainers, daysOffPreferences, scheduleMatrixLockableBitArrayConverterEx);

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
							                          DateOnly.Today) {Successful = false};
						foreach (string descriptionKey in backToLegalStateSolverContainer.FailedSolverDescriptionKeys)
						{
							string localizedText = Resources.ResourceManager.GetString(descriptionKey);
							IWorkShiftFilterResult workShiftFilterResult =
								new WorkShiftFilterResult(localizedText, 0, 0);
							workShiftFinderResult.AddFilterResults(workShiftFilterResult);
						}
						WorkShiftFinderResultHolder.AddResults(new List<IWorkShiftFinderResult> {workShiftFinderResult}, DateTime.Now);
					}
				}
			}



			using (PerformanceOutput.ForOperation("Moving days off according to solvers"))
			{
				foreach (ISmartDayOffBackToLegalStateSolverContainer backToLegalStateSolverContainer in solverContainers)
				{
					if (backToLegalStateSolverContainer.Result)
					{
						OptimizerHelperHelper.SyncSmartDayOffContainerWithMatrix(
							backToLegalStateSolverContainer,
							dayOffTemplate,
							daysOffPreferences,
							_scheduleDayChangeCallback,
							new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling),
							scheduleMatrixLockableBitArrayConverterEx);

						var restrictionChecker = new RestrictionChecker();
						var matrix = backToLegalStateSolverContainer.MatrixOriginalStateContainer.ScheduleMatrix;
						var originalStateContainer = backToLegalStateSolverContainer.MatrixOriginalStateContainer;
						var optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(matrix,
						                                                                                              restrictionChecker,
						                                                                                              optimizerPreferences,
						                                                                                              originalStateContainer);
						if (optimizationOverLimitByRestrictionDecider.MoveMaxDaysOverLimit() ||
						    optimizationOverLimitByRestrictionDecider.OverLimit().Count > 0)
						{
							var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback,
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")
		]
		public void ReOptimize(BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedDays, ISchedulingOptions schedulingOptions)
		{
			_backgroundWorker = backgroundWorker;
			_scheduledCount = 0;
			var optimizerPreferences = _container.Resolve<IOptimizationPreferences>();
			var onlyShiftsWhenUnderstaffed = optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;
			_sendEventEvery = optimizerPreferences.Advanced.RefreshScreenInterval;

			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedDays);

			var minutesPerInterval = 15;
			if (_stateHolder.Skills.Any())
			{
				minutesPerInterval = _stateHolder.Skills.Min(s => s.DefaultResolution);
			}

			var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(_stateHolder.Schedules);
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				optimizerPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = false;
				var tagSetter = _container.Resolve<IScheduleTagSetter>();
				tagSetter.ChangeTagToSet(optimizerPreferences.General.ScheduleTag);
				IList<IPerson> selectedPersons = new List<IPerson>(new PersonListExtractorFromScheduleParts(selectedDays).ExtractPersons());
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

				OptimizerHelperHelper.SetConsiderShortBreaks(selectedPersons, selectedPeriod, optimizerPreferences.Rescheduling,
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
						RunWorkShiftOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForWorkShiftOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							selectedPeriod,
							_backgroundWorker);
						continuedStep = true;
					}

					if (optimizerPreferences.General.OptimizationStepShiftsForFlexibleWorkTime)
					{
						recalculateIfContinuedStep(continuedStep, selectedPeriod);
						_extendReduceTimeHelper.RunExtendReduceTimeOptimization(optimizerPreferences, _backgroundWorker,
						                                                        selectedDays, _stateHolder,
						                                                        selectedPeriod,
						                                                        matrixOriginalStateContainerListForMoveMax);
						continuedStep = true;
					}

					if (optimizerPreferences.General.OptimizationStepDaysOffForFlexibleWorkTime)
					{
						recalculateIfContinuedStep(continuedStep, selectedPeriod);
						_extendReduceDaysOffHelper.RunExtendReduceDayOffOptimization(optimizerPreferences, _backgroundWorker,
						                                                             selectedDays, _schedulerStateHolder,
						                                                             selectedPeriod,
						                                                             matrixOriginalStateContainerListForMoveMax);
						continuedStep = true;

					}

					if (optimizerPreferences.General.OptimizationStepShiftsWithinDay)
					{
						recalculateIfContinuedStep(continuedStep, selectedPeriod);
						RunIntradayOptimization(
							optimizerPreferences,
							matrixOriginalStateContainerListForIntradayOptimization,
							workShiftOriginalStateContainerListForWorkShiftAndIntradayOptimization,
							backgroundWorker,
							selectedPeriod);
						continuedStep = true;
					}

					if (optimizerPreferences.General.OptimizationStepFairness)
					{
						recalculateIfContinuedStep(continuedStep, selectedPeriod);
						runFairness(tagSetter, selectedPersons, schedulingOptions, selectedPeriod, optimizerPreferences);
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
			var restrictionExtractor = _container.Resolve<IRestrictionExtractor>();
			OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForFairness, restrictionExtractor, optimizationPreferences, selectedPeriod);
			var rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, new DoNothingScheduleDayChangeCallBack(), tagSetter);

			var equalNumberOfCategoryFairnessService = _container.Resolve<IEqualNumberOfCategoryFairnessService>();
			equalNumberOfCategoryFairnessService.ReportProgress += resourceOptimizerPersonOptimized;
			equalNumberOfCategoryFairnessService.Execute(matrixListForFairness, selectedPeriod, selectedPersons,
			                                             schedulingOptions, _schedulerStateHolder.Schedules, rollbackService,
			                                             optimizationPreferences);
			equalNumberOfCategoryFairnessService.ReportProgress -= resourceOptimizerPersonOptimized;

			var instance = PrincipalAuthorization.Instance();
			if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction))
				return;

			////day off fairness
            var teamBlockDayOffFairnessOptimizationService = _container.Resolve<ITeamBlockDayOffFairnessOptimizationServiceFacade>();
            teamBlockDayOffFairnessOptimizationService.ReportProgress  += resourceOptimizerPersonOptimized;
            teamBlockDayOffFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions,
                _schedulerStateHolder.Schedules, rollbackService, optimizationPreferences);
            teamBlockDayOffFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
			
			var teamBlockSeniorityFairnessOptimizationService = _container.Resolve<ITeamBlockSeniorityFairnessOptimizationService>();
			teamBlockSeniorityFairnessOptimizationService.ReportProgress += resourceOptimizerPersonOptimized;
			teamBlockSeniorityFairnessOptimizationService.Execute(matrixListForFairness, selectedPeriod, selectedPersons, schedulingOptions, _stateHolder.ShiftCategories.ToList(), _schedulerStateHolder.Schedules, rollbackService, optimizationPreferences);
			teamBlockSeniorityFairnessOptimizationService.ReportProgress -= resourceOptimizerPersonOptimized;
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
            BackgroundWorker backgroundWorker,
			DateOnlyPeriod selectedPeriod)
        {
            _backgroundWorker = backgroundWorker;
            using (PerformanceOutput.ForOperation("Running new intraday optimization"))
            {

                if (_backgroundWorker.CancellationPending)
                    return;

                IList<IScheduleMatrixPro> matrixList =
                    matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

				OptimizerHelperHelper.LockDaysForIntradayOptimization(matrixList, selectedPeriod);

                optimizeIntraday(matrixContainerList, workShiftContainerList, optimizerPreferences);
            }
        }

        internal void RunWorkShiftOptimization(
           IOptimizationPreferences optimizerPreferences,
           IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixOriginalStateContainerList,
           IList<IScheduleMatrixOriginalStateContainer> workshiftOriginalStateContainerList,
           DateOnlyPeriod selectedPeriod,
           BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            using (PerformanceOutput.ForOperation("Running move time optimization"))
            {
                if (_backgroundWorker.CancellationPending)
                    return;

                IList<IScheduleMatrixPro> matrixList =
                    scheduleMatrixOriginalStateContainerList.Select(container => container.ScheduleMatrix).ToList();

				OptimizerHelperHelper.LockDaysForIntradayOptimization(matrixList, selectedPeriod);

                optimizeWorkShifts(scheduleMatrixOriginalStateContainerList, workshiftOriginalStateContainerList, optimizerPreferences, selectedPeriod);

                // we create a rollback service and do the changes and check for the case that not all white spots can be scheduled
                ISchedulePartModifyAndRollbackService rollbackService =
                    new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
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

            IList<IScheduleMatrixPro> matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

			OptimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, _container.Resolve<IRestrictionExtractor>(),_container.Resolve<IOptimizationPreferences >(), selectedPeriod);

            var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
            resourceOptimizerPersonOptimized(this, e);

            // to make sure we are in legal state before we can do day off optimization
            IList<IDayOffTemplate> displayList = _schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
            ((List<IDayOffTemplate>)displayList).Sort(new DayOffTemplateSorter());
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
            DaysOffBackToLegalState(matrixContainerList, _backgroundWorker,
                                    displayList[0], false, schedulingOptions, optimizerPreferences.DaysOff);

			var workShiftBackToLegalStateService =
				OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

			ISchedulePartModifyAndRollbackService rollbackService =
			   new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
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
            OptimizerHelperHelper.ScheduleBlankSpots(matrixContainerList, scheduleService, _container, rollbackService);
           

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
            rollbackService = new SchedulePartModifyAndRollbackService(_stateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            foreach (IScheduleMatrixOriginalStateContainer matrixContainer in validMatrixContainerList)
            {
                if (!matrixContainer.IsFullyScheduled())
                    rollbackMatrixChanges(matrixContainer, rollbackService);
            }
        }

        private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name);
            resourceOptimizerPersonOptimized(this, e);

            rollbackService.ClearModificationCollection();
            foreach (IScheduleDayPro scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
            {
                IScheduleDay originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
                rollbackService.Modify(originalPart);
            }
        }

        void resourceOptimizerPersonOptimized(object sender, ResourceOptimizerProgressEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                e.UserCancel = true;
            }
            _backgroundWorker.ReportProgress(1, e);
        }

	    public void RemoveShiftCategoryBackToLegalState(
		    IList<IScheduleMatrixPro> matrixList,
		    BackgroundWorker backgroundWorker, IOptimizationPreferences optimizationPreferences,
		    ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes)
	    {
		    if (matrixList == null)
			    throw new ArgumentNullException("matrixList");
		    if (backgroundWorker == null)
			    throw new ArgumentNullException("backgroundWorker");
		    if (schedulingOptions == null)
			    throw new ArgumentNullException("schedulingOptions");
		    using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
		    {
			    var backToLegalStateServicePro =
				    _container.Resolve<ISchedulePeriodListShiftCategoryBackToLegalStateService>();

			    if (backgroundWorker.CancellationPending)
				    return;

			    backToLegalStateServicePro.Execute(matrixList, schedulingOptions, optimizationPreferences);
		    }
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
                 OptimizerHelperHelper.CreateWorkShiftBackToLegalStateServicePro(_container);

	        var scheduleMatrixLockableBitArrayConverterEx =
		        _container.Resolve<IScheduleMatrixLockableBitArrayConverterEx>();

            IScheduleMatrixLockableBitArrayConverter scheduleMatrixArrayConverter =
                new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix, scheduleMatrixLockableBitArrayConverterEx);
            ILockableBitArray scheduleMatrixArray =
                scheduleMatrixArrayConverter.Convert(daysOffPreferences.ConsiderWeekBefore, daysOffPreferences.ConsiderWeekAfter);

            // create decisionmakers

            IEnumerable<IDayOffDecisionMaker> decisionMakers =
                OptimizerHelperHelper.CreateDecisionMakers(scheduleMatrixArray, optimizerPreferences, _container);
            IScheduleResultDataExtractor scheduleResultDataExtractor = OptimizerHelperHelper.CreatePersonalSkillsDataExtractor(optimizerPreferences.Advanced, scheduleMatrix);

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
            var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, optimizationUserPreferences, originalStateContainer);

			var resourceOptimizationHelper = _container.Resolve<IResourceOptimizationHelper>();
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_stateHolder), resourceOptimizationHelper);
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
                new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
													deleteAndResourceCalculateService,
                                                    scheduleService, WorkShiftFinderResultHolder,
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
                                                  optimizerOverLimitDecider,
                                                  nightRestWhiteSpotSolverService,
												  _container.Resolve<ISchedulingOptionsCreator>(),
												  mainShiftOptimizeActivitySpecificationSetter,
												  dayOffOptimizerPreMoveResultPredictor);

            IDayOffOptimizerContainer optimizerContainer =
                new DayOffOptimizerContainer(scheduleMatrixArrayConverter,
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
