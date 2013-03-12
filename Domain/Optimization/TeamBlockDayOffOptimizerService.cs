using System;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, 
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons, 
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDayOffTemplate dayOffTemplate);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private bool _cancelMe;

		public TeamBlockDayOffOptimizerService(
			ITeamInfoFactory teamInfoFactory, 				
			ILockableBitArrayFactory lockableBitArrayFactory,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
			ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ISchedulingResultStateHolder stateHolder,
			ITeamBlockScheduler teamBlockScheduler,
			IResourceOptimizationHelper resourceOptimizationHelper,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory
			)
		{
			_teamInfoFactory = teamInfoFactory;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_stateHolder = stateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList, 
			DateOnlyPeriod selectedPeriod,
		    IList<IPerson> selectedPersons, 
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IDayOffTemplate dayOffTemplate
			)
		{
			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			schedulingOptions.DayOffTemplate = dayOffTemplate;
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			}

			// find a random selected TeamInfo/matrix
			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

			
			var previousPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

			while (remainingInfoList.Count > 0)
			{
				var teamInfosToRemove = runOneOptimizationRound(optimizationPreferences, schedulePartModifyAndRollbackService,
				                                                remainingInfoList, schedulingOptions, previousPeriodValue);

				if (_cancelMe)
					break;

				foreach (var teamInfo in teamInfosToRemove)
				{
					remainingInfoList.Remove(teamInfo);
				}
			}

			
		}

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		private IEnumerable<ITeamInfo> runOneOptimizationRound(IOptimizationPreferences optimizationPreferences,
		                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                     List<ITeamInfo> remainingInfoList, ISchedulingOptions schedulingOptions,
		                                     double previousPeriodValue)
		{
			var teamInfosToRemove = new List<ITeamInfo>();
			foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				if (_cancelMe)
					break;

				foreach (var matrix in teamInfo.MatrixesForGroupMember(0))
				{
					runOneMatrix(optimizationPreferences, schedulePartModifyAndRollbackService, schedulingOptions, matrix, teamInfo);
				}
				// rollback id failed or not good
				var currentPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (currentPeriodValue >= previousPeriodValue)
					teamInfosToRemove.Add(teamInfo);
				previousPeriodValue = currentPeriodValue;

				OnReportProgress("Periodvalue: " + currentPeriodValue + " Optimized team " + teamInfo.GroupPerson.Name);
			}

			return teamInfosToRemove;
		}

		private void runOneMatrix(IOptimizationPreferences optimizationPreferences,
		                          ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                          ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, ITeamInfo teamInfo)
		{
			ILockableBitArray originalArray =
				_lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
				                                           optimizationPreferences.DaysOff.ConsiderWeekAfter, matrix);
			ILockableBitArray resultingArray = tryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return;

			// find out what have changed
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
			                                                                            optimizationPreferences.DaysOff
			                                                                                                   .ConsiderWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray,
			                                                                                matrix,
			                                                                                optimizationPreferences.DaysOff
			                                                                                                       .ConsiderWeekBefore);
			// Does the predictor beleve in this?
			foreach (var dateOnly in removedDaysOff)
			{
				var toRemove = removeDaysOff(optimizationPreferences, schedulePartModifyAndRollbackService, teamInfo, dateOnly);
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true, toRemove, new List<IScheduleDay>());
			}

			foreach (var dateOnly in addedDaysOff)
			{
				var tupleOfLists = addDaysOff(optimizationPreferences, schedulePartModifyAndRollbackService, teamInfo, dateOnly,
				                              schedulingOptions);
				IList<IScheduleDay> toRemove = tupleOfLists.Item1;
				IList<IScheduleDay> toAdd = tupleOfLists.Item2;
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true, toRemove, toAdd);
			}

			foreach (var dateOnly in removedDaysOff)
			{
				ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                         schedulingOptions
					                                                                         .BlockFinderTypeForAdvanceScheduling);
				_teamBlockScheduler.ScheduleTeamBlock(teamBlockInfo, dateOnly, schedulingOptions, true);
			}


			// ev back to legal state?
			// if possible reschedule block without clearing
			// else
			//	clear involved teamblocks
			//	reschedule involved teamblocks

			// remember not to break anything in shifts or restrictions
		}

		private Tuple<IList<IScheduleDay>, IList<IScheduleDay>> addDaysOff(IOptimizationPreferences optimizationPreferences,
								   ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
								   ITeamInfo teamInfo, DateOnly dateOnly, ISchedulingOptions schedulingOptions)
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();
			IList<IScheduleDay> toAdd = new List<IScheduleDay>();
			if (optimizationPreferences.Extra.KeepSameDaysOffInTeam) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleDay scheduleDay = _stateHolder.Schedules[person].ScheduledDay(dateOnly);
					toRemove.Add((IScheduleDay)scheduleDay.Clone());
					scheduleDay.DeleteMainShift(scheduleDay);
					scheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
					toAdd.Add(_stateHolder.Schedules[person].ReFetch(scheduleDay));
				}
			}

			return new Tuple<IList<IScheduleDay>, IList<IScheduleDay>>(toRemove, toAdd);
		}

		private IList<IScheduleDay> removeDaysOff(IOptimizationPreferences optimizationPreferences,
		                           ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                           ITeamInfo teamInfo, DateOnly dateOnly)
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();
			if (optimizationPreferences.Extra.KeepSameDaysOffInTeam) // do it on every team member
			{
				foreach (var person in teamInfo.GroupPerson.GroupMembers)
				{
					IScheduleDay scheduleDay = _stateHolder.Schedules[person].ScheduledDay(dateOnly);
					toRemove.Add((IScheduleDay) scheduleDay.Clone());
					scheduleDay.DeleteDayOff();
					schedulePartModifyAndRollbackService.Modify(scheduleDay);
				}
			}
			return toRemove;
		}

		private ILockableBitArray tryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray, IOptimizationPreferences optimizationPreferences)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers = _dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences);
			foreach (var dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray)originalArray.Clone();

				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}


		// create factory of this
		
	}
}