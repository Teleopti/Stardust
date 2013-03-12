using System;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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
			IPeriodValueCalculator periodValueCalculatorForAllSkills
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

			var teamInfosToRemove = new List<ITeamInfo>();
			var previousPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

			while (remainingInfoList.Count > 0)
			{
				runOneOptimizationRound(optimizationPreferences, schedulePartModifyAndRollbackService, remainingInfoList, schedulingOptions, previousPeriodValue, teamInfosToRemove);
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
				ResourceOptimizerProgressEventArgs args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		private void runOneOptimizationRound(IOptimizationPreferences optimizationPreferences,
		                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                                     List<ITeamInfo> remainingInfoList, ISchedulingOptions schedulingOptions,
		                                     double previousPeriodValue, List<ITeamInfo> teamInfosToRemove)
		{
			foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				foreach (var matrix in teamInfo.MatrixesForGroupMember(0))
				{
					ILockableBitArray originalArray =
						_lockableBitArrayFactory.ConvertFromMatrix(optimizationPreferences.DaysOff.ConsiderWeekBefore,
						                                           optimizationPreferences.DaysOff.ConsiderWeekAfter, matrix);

					IScheduleResultDataExtractor scheduleResultDataExtractor =
						_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

					ILockableBitArray resultingArray = tryFindMoves(originalArray, scheduleResultDataExtractor, optimizationPreferences);

					if (resultingArray.Equals(originalArray))
						continue;

					// find out what have changed
					IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
					                                                                            optimizationPreferences.DaysOff
					                                                                                                   .ConsiderWeekBefore);
					IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray,
					                                                                                matrix,
					                                                                                optimizationPreferences.DaysOff
					                                                                                                       .ConsiderWeekBefore);

					// Does the predictor beleve in this?
					// execute do moves
					foreach (var dateOnly in removedDaysOff)
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
						_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true, toRemove, new List<IScheduleDay>());
					}

					foreach (var dateOnly in addedDaysOff)
					{
						IList<IScheduleDay> toRemove = new List<IScheduleDay>();
						IList<IScheduleDay> toAdd = new List<IScheduleDay>();
						if (optimizationPreferences.Extra.KeepSameDaysOffInTeam) // do it on every team member
						{
							foreach (var person in teamInfo.GroupPerson.GroupMembers)
							{
								IScheduleDay scheduleDay = _stateHolder.Schedules[person].ScheduledDay(dateOnly);
								toRemove.Add((IScheduleDay) scheduleDay.Clone());
								scheduleDay.DeleteMainShift(scheduleDay);
								scheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
								schedulePartModifyAndRollbackService.Modify(scheduleDay);
								toAdd.Add(_stateHolder.Schedules[person].ReFetch(scheduleDay));
							}
						}
						_resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true, toRemove, new List<IScheduleDay>());
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
				// rollback id failed or not good
				var currentPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (currentPeriodValue >= previousPeriodValue)
					teamInfosToRemove.Add(teamInfo);
				previousPeriodValue = currentPeriodValue;

				OnReportProgress("Periodvalue: " + currentPeriodValue + " Optimized team " + teamInfo.GroupPerson.Name);
			}
		}

		private ILockableBitArray tryFindMoves(ILockableBitArray originalArray, IScheduleResultDataExtractor scheduleResultDataExtractor, IOptimizationPreferences optimizationPreferences)
		{
			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers = createDecisionMakers(originalArray, optimizationPreferences);
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
		private static IEnumerable<IDayOffDecisionMaker> createDecisionMakers(
			ILockableBitArray scheduleMatrixArray,
			IOptimizationPreferences optimizerPreferences)
		{
			var daysOffPreferences = optimizerPreferences.DaysOff;
			IList<IDayOffLegalStateValidator> legalStateValidators =
				 createLegalStateValidators(scheduleMatrixArray, daysOffPreferences, optimizerPreferences);

			IList<IDayOffLegalStateValidator> legalStateValidatorsToKeepWeekEnds =
				createLegalStateValidatorsToKeepWeekendNumbers(scheduleMatrixArray, optimizerPreferences);

			IOfficialWeekendDays officialWeekendDays = new OfficialWeekendDays();
			ILogWriter logWriter = new LogWriter<DayOffOptimizationService>();

			IDayOffDecisionMaker moveDayOffDecisionMaker = new MoveOneDayOffDecisionMaker(legalStateValidators, logWriter);
			IDayOffDecisionMaker moveWeekEndDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidatorsToKeepWeekEnds, officialWeekendDays, true, logWriter);
			IDayOffDecisionMaker moveTwoWeekEndDaysDecisionMaker = new MoveWeekendDayOffDecisionMaker(legalStateValidators, officialWeekendDays, false, logWriter);


			bool is2222 = false;
			if (daysOffPreferences.UseDaysOffPerWeek && daysOffPreferences.DaysOffPerWeekValue.Minimum == 2 && daysOffPreferences.DaysOffPerWeekValue.Maximum == 2)
			{
				if (daysOffPreferences.UseConsecutiveDaysOff && daysOffPreferences.ConsecutiveDaysOffValue.Minimum == 2 && daysOffPreferences.ConsecutiveDaysOffValue.Maximum == 2)
				{
					if (daysOffPreferences.UseConsecutiveWorkdays)
						is2222 = true;
				}
			}
			IDayOffDecisionMaker teDataDayOffDecisionMaker = new TeDataDayOffDecisionMaker(legalStateValidators, is2222, logWriter);

			IList<IDayOffDecisionMaker> retList = new List<IDayOffDecisionMaker> { moveDayOffDecisionMaker, moveTwoWeekEndDaysDecisionMaker, moveWeekEndDecisionMaker, teDataDayOffDecisionMaker };

			if (daysOffPreferences.UseConsecutiveWorkdays && daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
			{
				if (daysOffPreferences.UseFullWeekendsOff && daysOffPreferences.FullWeekendsOffValue.Equals(new MinMax<int>(1, 1)))
				{
					IDayOffDecisionMaker cMSBOneFreeWeekendMax5WorkingdaysDecisionMaker = new CMSBOneFreeWeekendMax5WorkingDaysDecisionMaker(officialWeekendDays, new TrueFalseRandomizer());
					retList.Add(cMSBOneFreeWeekendMax5WorkingdaysDecisionMaker);
				}
			}

			return retList;
		}

		private static IList<IDayOffLegalStateValidator> createLegalStateValidators(
		   ILockableBitArray bitArray,
		   IDaysOffPreferences dayOffPreferences,
		   IOptimizationPreferences optimizerPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!dayOffPreferences.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationLegalStateValidatorListCreator
					(optimizerPreferences.DaysOff,
					 weekendDays,
					 bitArray.ToLongBitArray(),
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		}

		private static IList<IDayOffLegalStateValidator> createLegalStateValidatorsToKeepWeekendNumbers(
			ILockableBitArray bitArray,
			IOptimizationPreferences optimizerPreferences)
		{
			MinMax<int> periodArea = bitArray.PeriodArea;
			if (!optimizerPreferences.DaysOff.ConsiderWeekBefore)
				periodArea = new MinMax<int>(periodArea.Minimum + 7, periodArea.Maximum + 7);
			IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
			IDayOffLegalStateValidatorListCreator validatorListCreator =
				new DayOffOptimizationWeekendLegalStateValidatorListCreator(optimizerPreferences.DaysOff,
					 weekendDays,
					 periodArea);

			return validatorListCreator.BuildActiveValidatorList();
		}
	}
}