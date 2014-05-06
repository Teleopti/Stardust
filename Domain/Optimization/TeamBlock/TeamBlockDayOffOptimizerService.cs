using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList,
		                     DateOnlyPeriod selectedPeriod,
		                     IList<IPerson> selectedPersons,
		                     IOptimizationPreferences optimizationPreferences,
		                     ISchedulePartModifyAndRollbackService rollbackService,ISchedulingOptions schedulingOptions );

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private bool _cancelMe;
	    private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

	    public TeamBlockDayOffOptimizerService(
			ITeamInfoFactory teamInfoFactory,
			ILockableBitArrayFactory lockableBitArrayFactory,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_teamInfoFactory = teamInfoFactory;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamTeamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockClearer = teamBlockClearer;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_teamBlockDaysOffMoveFinder = teamBlockDaysOffMoveFinder;
	        _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,ISchedulingOptions schedulingOptions 
			)
		{
			
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList);
				if (teamInfo != null)
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

			while (remainingInfoList.Count > 0)
			{
				IEnumerable<ITeamInfo> teamInfosToRemove;
				if(optimizationPreferences.Extra.UseTeamSameDaysOff)
				{
					teamInfosToRemove = runOneOptimizationRound(optimizationPreferences, rollbackService,
					                                            remainingInfoList, schedulingOptions,
					                                            selectedPeriod, selectedPersons, allPersonMatrixList);
				}
				else
				{
					teamInfosToRemove = runOneOptimizationRoundWithFreeDaysOff(optimizationPreferences, rollbackService,
																remainingInfoList, schedulingOptions,
																selectedPeriod, selectedPersons, allPersonMatrixList);
				}

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
			EventHandler<ResourceOptimizerProgressEventArgs> handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		private IEnumerable<ITeamInfo> runOneOptimizationRoundWithFreeDaysOff(IOptimizationPreferences optimizationPreferences,
															   ISchedulePartModifyAndRollbackService rollbackService,
															   List<ITeamInfo> remainingInfoList,
															   ISchedulingOptions schedulingOptions,
															   DateOnlyPeriod selectedPeriod,
															   IList<IPerson> selectedPersons,
															   IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			var teamInfosToRemove = new List<ITeamInfo>();
			double previousPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count;
			var currentTeamInfoCounter = 0;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				currentTeamInfoCounter++;

				if (teamInfo.GroupPerson.GroupMembers.Any())
				{
					foreach (var matrix in teamInfo.MatrixesForGroup())
					{
						rollbackService.ClearModificationCollection();

						var success = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix, schedulingOptions, teamInfo,
												   selectedPeriod, selectedPersons, allPersonMatrixList);

						previousPeriodValue = handleResult(rollbackService, schedulingOptions, previousPeriodValue, success,
													   teamInfosToRemove, teamInfo, totalLiveTeamInfos, currentTeamInfoCounter);
					}
					
					if (_cancelMe)
						break;
				}
			}

			return teamInfosToRemove;
		}
			
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.TeamBlock.TeamBlockDayOffOptimizerService.OnReportProgress(System.String)")]
		private IEnumerable<ITeamInfo> runOneOptimizationRound(IOptimizationPreferences optimizationPreferences,
		                                                       ISchedulePartModifyAndRollbackService rollbackService,
		                                                       List<ITeamInfo> remainingInfoList,
		                                                       ISchedulingOptions schedulingOptions,
		                                                       DateOnlyPeriod selectedPeriod,
		                                                       IList<IPerson> selectedPersons,
															   IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			var teamInfosToRemove = new List<ITeamInfo>();
			double previousPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count;
			var currentTeamInfoCounter = 0;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				currentTeamInfoCounter++;

				if (teamInfo.GroupPerson.GroupMembers.Any())
				{
					foreach (IScheduleMatrixPro matrix in teamInfo.MatrixesForGroupMember(0))
					{
						rollbackService.ClearModificationCollection();
						var success = runOneTeam(optimizationPreferences, rollbackService, schedulingOptions, matrix, teamInfo,
						                           selectedPeriod, selectedPersons, allPersonMatrixList);

						previousPeriodValue = handleResult(rollbackService, schedulingOptions, previousPeriodValue, success,
						                                   teamInfosToRemove, teamInfo, totalLiveTeamInfos, currentTeamInfoCounter);

						if (_cancelMe)
							break;
					}
				}

				if (_cancelMe)
					break;
			}

			return teamInfosToRemove;
		}

		private double handleResult(ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
		                            double previousPeriodValue, bool success, List<ITeamInfo> teamInfosToRemove, ITeamInfo teamInfo,
		                            int totalLiveTeamInfos, int currentTeamInfoCounter)
		{
			double currentPeriodValue =
				_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

			if (currentPeriodValue >= previousPeriodValue || !success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				teamInfosToRemove.Add(teamInfo);
				currentPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			}

			previousPeriodValue = currentPeriodValue;

			OnReportProgress(Resources.OptimizingDaysOff + Resources.Colon + "(" + totalLiveTeamInfos.ToString("####") + ")(" +
			                 currentTeamInfoCounter.ToString("####") + ") " +
			                 StringHelper.DisplayString(teamInfo.GroupPerson.Name.ToString(), 20) + " (" + currentPeriodValue +
			                 ")");

			return previousPeriodValue;
		}

		private bool runOneMatrixOnly(IOptimizationPreferences optimizationPreferences,
		                              ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix,
		                              ISchedulingOptions schedulingOptions, ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod,
		                              IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			var movedDaysOff = affectedDaysOff(optimizationPreferences, matrix);
			if (movedDaysOff == null)
				return false;

			IPerson person = matrix.Person;
			removeAllDecidedDaysOffForMember(rollbackService, movedDaysOff.RemovedDaysOff, person);
			addAllDecidedDaysOffForMember(rollbackService, schedulingOptions, movedDaysOff.AddedDaysOff, person);

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, selectedPeriod, selectedPersons, movedDaysOff.RemovedDaysOff,
			                                         rollbackService, allPersonMatrixList);
			if (!success)
				return false;

			var isMaxSeatRuleViolated = movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
										movedDaysOff.RemovedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions));
			if (isMaxSeatRuleViolated || !_restrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);

				return true;
			}

			return true;
		}

		private void addAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService,
		                                           ISchedulingOptions schedulingOptions, IList<DateOnly> addedDaysOff, IPerson person)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForMember(rollbackService, person, dateOnly, schedulingOptions.DayOffTemplate, true);
			}
		}

		private void removeAllDecidedDaysOffForMember(ISchedulePartModifyAndRollbackService rollbackService,
		                                              IList<DateOnly> removedDaysOff, IPerson person)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForMember(rollbackService, person, dateOnly);
			}
		}

		private bool runOneTeam(IOptimizationPreferences optimizationPreferences,
		                        ISchedulePartModifyAndRollbackService rollbackService,
		                        ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
		                        ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
		                        IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			var movedDaysOff = affectedDaysOff(optimizationPreferences, matrix);
			if (movedDaysOff == null)
				return false;

			removeAllDecidedDaysOffForTeam(rollbackService, teamInfo, movedDaysOff.RemovedDaysOff);
			addAllDecidedDaysOffForTeam(rollbackService, schedulingOptions, teamInfo, movedDaysOff.AddedDaysOff);

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, selectedPeriod, selectedPersons, movedDaysOff.RemovedDaysOff,
			                                         rollbackService, allPersonMatrixList);
			if (!success)
				return false;

			var isMaxSeatRuleViolated = movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
										movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x.AddDays(1), schedulingOptions));
			if (isMaxSeatRuleViolated || !_restrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);

				return true;
			}

			return true;
		}

		private movedDaysOff affectedDaysOff(IOptimizationPreferences optimizationPreferences, IScheduleMatrixPro matrix)
		{
			bool considerWeekBefore = optimizationPreferences.DaysOff.ConsiderWeekBefore;
			bool considerWeekAfter = optimizationPreferences.DaysOff.ConsiderWeekAfter;
			ILockableBitArray originalArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
			                                                                             matrix);
			ILockableBitArray resultingArray = _teamBlockDaysOffMoveFinder.TryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return null;

			// find out what have changed, Does the predictor beleve in this? depends on how many members
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
			                                                                            considerWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray, matrix,
			                                                                                considerWeekBefore);

			return new movedDaysOff
				{
					AddedDaysOff = addedDaysOff,
					RemovedDaysOff = removedDaysOff
				};
		}

		private static void lockDaysInMatrixes(IList<DateOnly> datesToLock , ITeamInfo teamInfo)
		{
			foreach (var dateOnly in datesToLock)
			{
				foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
				{
					scheduleMatrixPro.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
				}
			}
		}

		private bool reScheduleAllMovedDaysOff(ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                       DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
		                                       IEnumerable<DateOnly> removedDaysOff, ISchedulePartModifyAndRollbackService rollbackService
			, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
                ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                        schedulingOptions
                                                                                            .BlockFinderTypeForAdvanceScheduling, 
																							_teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions) ,
																							allPersonMatrixList);
				if (teamBlockInfo == null) continue;
				if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				bool success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod,
																		selectedPersons, rollbackService);
				
				if (!success)
					return false;
			}

			return true;
		}

		private void addAllDecidedDaysOffForTeam(ISchedulePartModifyAndRollbackService rollbackService,
		                                 ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                 IEnumerable<DateOnly> addedDaysOff)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(rollbackService, teamInfo, dateOnly,
				                                                         schedulingOptions.DayOffTemplate);
			}
		}

		private void removeAllDecidedDaysOffForTeam(ISchedulePartModifyAndRollbackService rollbackService,
		                                     ITeamInfo teamInfo,
		                                     IEnumerable<DateOnly> removedDaysOff)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForTeam(rollbackService, teamInfo, dateOnly);
			}
		}

		private class movedDaysOff
		{
			public IList<DateOnly> AddedDaysOff { get; set; }
			public IList<DateOnly> RemovedDaysOff { get; set; }
		}
	}
}