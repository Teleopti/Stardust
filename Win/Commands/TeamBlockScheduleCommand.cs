﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface ITeamBlockScheduleCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedSchedules);
	}

	public class TeamBlockScheduleCommand : ITeamBlockScheduleCommand
	{
		private readonly IFixedStaffSchedulingService _fixedStaffSchedulingService;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly IAdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IRestrictionAggregator _restrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IWorkShiftFinderResultHolder _workShiftFinderResultHolder;
		private BackgroundWorker _backgroundWorker;
		private int _scheduledCount;
		private ISchedulingOptions _schedulingOptions;

		public TeamBlockScheduleCommand(IFixedStaffSchedulingService fixedStaffSchedulingService,
			ISchedulerStateHolder schedulerStateHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			IAdvanceDaysOffSchedulingService advanceDaysOffSchedulingService,
			IMatrixListFactory matrixListFactory,
			IGroupPersonsBuilder groupPersonsBuilder,
			IResourceOptimizationHelper resourceOptimizationHelper,
			ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
			IRestrictionAggregator restrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			IWorkShiftSelector workShiftSelector,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			IWorkShiftFinderResultHolder workShiftFinderResultHolder)
		{
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_groupPersonsBuilder = groupPersonsBuilder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_restrictionAggregator = restrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_workShiftSelector = workShiftSelector;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
		}

		public void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedSchedules)
		{
			_schedulingOptions = schedulingOptions;
			_backgroundWorker = backgroundWorker;
			_fixedStaffSchedulingService.ClearFinderResults();
			if (schedulingOptions != null)
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff =
					new SchedulePartModifyAndRollbackService(_schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
															 new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

				var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);

				IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixList(selectedSchedules, selectedPeriod);
				if (matrixesOfSelectedScheduleDays.Count == 0)
					return;

				var allScheduleDays = new List<IScheduleDay>();

				foreach (var scheduleMatrixPro in matrixesOfSelectedScheduleDays)
				{
					allScheduleDays.AddRange(
						_schedulerStateHolder.Schedules[scheduleMatrixPro.Person].ScheduledDayCollection(
							scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod).ToList());
				}

				var allMatrixesOfSelectedPersons = _matrixListFactory.CreateMatrixList(allScheduleDays, selectedPeriod);
				

				_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				_advanceDaysOffSchedulingService.Execute(allMatrixesOfSelectedPersons,
				                                         schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				                                         groupPersonBuilderForOptimization);
				_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			
				var teamSteadyStateHolder = initiateTeamSteadyStateHolder(allMatrixesOfSelectedPersons, schedulingOptions, selectedPeriod);
				ISchedulePartModifyAndRollbackService rollbackService =
				   new SchedulePartModifyAndRollbackService(_schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
															new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			    var advanceSchedulingResults = new List<IWorkShiftFinderResult>();
                var advanceSchedulingService = callAdvanceSchedulingService(schedulingOptions, groupPersonBuilderForOptimization, rollbackService, advanceSchedulingResults);
				IDictionary<string, IWorkShiftFinderResult> schedulingResults = new Dictionary<string, IWorkShiftFinderResult>();
				var allVisibleMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);

				advanceSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				advanceSchedulingService.ScheduleSelected(allVisibleMatrixes, selectedPeriod,
												  matrixesOfSelectedScheduleDays.Select(x => x.Person).Distinct().ToList(),
												  teamSteadyStateHolder, rollbackService);
				advanceSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
                _workShiftFinderResultHolder.AddResults(new List<IWorkShiftFinderResult>(schedulingResults.Values), DateTime.Now);
				//if (schedulingOptions.RotationDaysOnly)
				//    schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();

                _workShiftFinderResultHolder.AddResults(advanceSchedulingResults, DateTime.Now);
			}
			_workShiftFinderResultHolder.AddResults(_fixedStaffSchedulingService.FinderResults, DateTime.Now);
		}

		private void schedulingServiceDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			_scheduledCount++;
			if (_scheduledCount >= _schedulingOptions.RefreshRate)
			{
				_backgroundWorker.ReportProgress(1, e.SchedulePart);
				_scheduledCount = 0;
			}
		}

		private TeamSteadyStateHolder initiateTeamSteadyStateHolder(IList<IScheduleMatrixPro> selectedPersonAllMatrixList,
																	ISchedulingOptions schedulingOptions,
																	DateOnlyPeriod selectedPeriod)
		{
			var targetTimeCalculator = new SchedulePeriodTargetTimeCalculator();

			var teamSteadyStateRunner = new TeamSteadyStateRunner(selectedPersonAllMatrixList, targetTimeCalculator);
			var teamSteadyStateCreator = new TeamBlockSteadyStateDictionaryCreator(teamSteadyStateRunner, selectedPersonAllMatrixList,
																			  _groupPersonsBuilder, schedulingOptions);
			var teamSteadyStateDictionary = teamSteadyStateCreator.Create(selectedPeriod);
			var teamSteadyStateHolder = new TeamSteadyStateHolder(teamSteadyStateDictionary);
			return teamSteadyStateHolder;
		}

		private TeamBlockSchedulingService callAdvanceSchedulingService(ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization, ISchedulePartModifyAndRollbackService rollbackService, List<IWorkShiftFinderResult> advanceSchedulingResults)
		{

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);
			var teamScheduling = new TeamScheduling(resourceCalculateDelayer, rollbackService);


			ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			ITeamBlockScheduler teamBlockScheduler =
				new TeamBlockScheduler(_skillDayPeriodIntervalDataGenerator,
									   _restrictionAggregator,
									   _workShiftFilterService, 
									   teamScheduling,
									   _workShiftSelector);

			var advanceSchedulingService =
				new TeamBlockSchedulingService(schedulingOptions,
											 teamInfoFactory,
											 _teamBlockInfoFactory,
											 teamBlockScheduler, 
											 new BlockSteadyStateValidator(), 
											 _safeRollbackAndResourceCalculation,
                                             _workShiftMinMaxCalculator, advanceSchedulingResults);

			return advanceSchedulingService;
		}
	}
}