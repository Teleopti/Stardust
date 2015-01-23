using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
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
		                     ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
		                     IResourceCalculateDelayer resourceCalculateDelayer,
		                     ISchedulingResultStateHolder schedulingResultStateHolder);

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
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private readonly ITeamBlockDaysOffMoveFinder _teamBlockDaysOffMoveFinder;
		private bool _cancelMe;
	    private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private ResourceOptimizerProgressEventArgs _progressEvent;

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
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			ITeamBlockDaysOffMoveFinder teamBlockDaysOffMoveFinder, ITeamBlockSchedulingOptions teamBlockSchedulingOptions, IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification)
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
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_teamBlockDaysOffMoveFinder = teamBlockDaysOffMoveFinder;
	        _teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder
			)
		{
			_progressEvent = null;
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList);
				if(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff  )
				{
					if (!_allTeamMembersInSelectionSpecification.IsSatifyBy(teamInfo, selectedPersons))
						continue;
				}
				if (teamInfo != null )
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(groupMember);
				}
			}

			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

			while (remainingInfoList.Count > 0)
			{
				IEnumerable<ITeamInfo> teamInfosToRemove;
				if(optimizationPreferences.Extra.UseTeamSameDaysOff)
				{
					teamInfosToRemove = runOneOptimizationRound(optimizationPreferences, rollbackService,
					                                            remainingInfoList, schedulingOptions,
					                                            resourceCalculateDelayer, schedulingResultStateHolder);
				}
				else
				{
					teamInfosToRemove = runOneOptimizationRoundWithFreeDaysOff(optimizationPreferences, rollbackService,
					                                                           remainingInfoList, schedulingOptions,
					                                                           selectedPersons,
					                                                           resourceCalculateDelayer, schedulingResultStateHolder);
				}

				if (_cancelMe)
					break;

				if (_progressEvent != null && _progressEvent.UserCancel)
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

				if (_progressEvent != null && _progressEvent.UserCancel)
					return;

				_progressEvent = args;
			}
		}

		private IEnumerable<ITeamInfo> runOneOptimizationRoundWithFreeDaysOff(IOptimizationPreferences optimizationPreferences,
															   ISchedulePartModifyAndRollbackService rollbackService,
															   List<ITeamInfo> remainingInfoList,
															   ISchedulingOptions schedulingOptions,
															   IList<IPerson> selectedPersons,
																IResourceCalculateDelayer resourceCalculateDelayer,
																ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var teamInfosToRemove = new HashSet<ITeamInfo>();
			double previousPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count;
			var currentTeamInfoCounter = 0;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				currentTeamInfoCounter++;

				if (teamInfo.GroupMembers.Any())
				{
					var allFailed = true;
					foreach (var matrix in teamInfo.MatrixesForGroup())
					{
						if (!(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff ))
						{
							if (!selectedPersons.Contains(matrix.Person)) 
								continue;
						}
						rollbackService.ClearModificationCollection();

						bool checkPeriodValue;
						var success = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix, schedulingOptions, teamInfo,
						                               resourceCalculateDelayer,
													   schedulingResultStateHolder,
													   out checkPeriodValue);
						double currentPeriodValue =
							_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
						success = handleResult(rollbackService, schedulingOptions, previousPeriodValue, success,
													   teamInfo, totalLiveTeamInfos, currentTeamInfoCounter, currentPeriodValue, checkPeriodValue);

						if (success)
						{
							previousPeriodValue = currentPeriodValue;
							allFailed = false;
						}
					}
					
					if(allFailed)
						teamInfosToRemove.Add(teamInfo);

					if (_cancelMe)
						break;

					if (_progressEvent != null && _progressEvent.UserCancel)
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
																IResourceCalculateDelayer resourceCalculateDelayer,
																ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var teamInfosToRemove = new HashSet<ITeamInfo>();
			double previousPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			var totalLiveTeamInfos = remainingInfoList.Count;
			var currentTeamInfoCounter = 0;
			foreach (ITeamInfo teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				currentTeamInfoCounter++;

				if (teamInfo.GroupMembers.Any())
				{
					var allFailed = true;
					foreach (IScheduleMatrixPro matrix in teamInfo.MatrixesForGroupMember(0))
					{
						rollbackService.ClearModificationCollection();

						bool checkPeriodValue;
						var success = runOneTeam(optimizationPreferences, rollbackService, schedulingOptions, matrix, teamInfo,
						                         resourceCalculateDelayer,
						                         schedulingResultStateHolder,
												 out checkPeriodValue);
						double currentPeriodValue =
							_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
						success = handleResult(rollbackService, schedulingOptions, previousPeriodValue, success,
						                                   teamInfo, totalLiveTeamInfos, currentTeamInfoCounter, currentPeriodValue, checkPeriodValue);

						if (success)
						{
							allFailed = false;
							previousPeriodValue = currentPeriodValue;
						}

						if (_cancelMe)
							break;

						if (_progressEvent != null && _progressEvent.UserCancel)
							break;
					}

					if (allFailed)
						teamInfosToRemove.Add(teamInfo);
				}

				if (_cancelMe)
					break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;
			}

			return teamInfosToRemove;
		}

		private bool handleResult(ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions,
		                            double previousPeriodValue, bool success, ITeamInfo teamInfo,
									int totalLiveTeamInfos, int currentTeamInfoCounter, double currentPeriodValue, bool checkPeriodValue)
		{
			var failed = !success;

			if (!failed && checkPeriodValue)
			{
				failed = currentPeriodValue >= previousPeriodValue;	
			}
		
			if (failed)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				
				currentPeriodValue =
					_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
			}

			OnReportProgress(Resources.OptimizingDaysOff + Resources.Colon + "(" + totalLiveTeamInfos.ToString("####") + ")(" +
			                 currentTeamInfoCounter.ToString("####") + ") " +
			                 StringHelper.DisplayString(teamInfo.Name, 20) + " (" + currentPeriodValue +
			                 ")");

			return !failed;
		}

		private bool runOneMatrixOnly(IOptimizationPreferences optimizationPreferences,
		                              ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix,
		                              ISchedulingOptions schedulingOptions, ITeamInfo teamInfo, 
		                              IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder,
										out bool checkPeriodValue)
		{

			var movedDaysOff = affectedDaysOff(optimizationPreferences, matrix);
			if (movedDaysOff == null)
			{
				checkPeriodValue = true;
				return false;
			}

			IPerson person = matrix.Person;
			removeAllDecidedDaysOffForMember(rollbackService, movedDaysOff.RemovedDaysOff, person);
			addAllDecidedDaysOffForMember(rollbackService, schedulingOptions, movedDaysOff.AddedDaysOff, person);

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo,
			                                         movedDaysOff.RemovedDaysOff,
			                                         rollbackService, resourceCalculateDelayer,
			                                         schedulingResultStateHolder);
			if (!success)
			{
				checkPeriodValue = true;
				return false;
			}

			var isMaxSeatRuleViolated =
				movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
				movedDaysOff.RemovedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions));
			
			if (isMaxSeatRuleViolated || !_teamBlockOptimizationLimits.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
				checkPeriodValue = true;
				return true;
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				checkPeriodValue = false;
				return true;
			}

			checkPeriodValue = true;
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
		                        ITeamInfo teamInfo,
								IResourceCalculateDelayer resourceCalculateDelayer,
								ISchedulingResultStateHolder schedulingResultStateHolder,
								out bool checkPeriodValue)
		{
			var movedDaysOff = affectedDaysOff(optimizationPreferences, matrix);
			if (movedDaysOff == null)
			{
				checkPeriodValue = true;
				return false;
			}

			removeAllDecidedDaysOffForTeam(rollbackService, teamInfo, movedDaysOff.RemovedDaysOff);
			addAllDecidedDaysOffForTeam(rollbackService, schedulingOptions, teamInfo, movedDaysOff.AddedDaysOff);

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo,
			                                         movedDaysOff.RemovedDaysOff,
			                                         rollbackService, resourceCalculateDelayer,
			                                         schedulingResultStateHolder);
			if (!success)
			{
				checkPeriodValue = true;
				return false;
			}

			var isMaxSeatRuleViolated = movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
										movedDaysOff.AddedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x.AddDays(1), schedulingOptions));
			
			if (isMaxSeatRuleViolated || !_teamBlockOptimizationLimits.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				lockDaysInMatrixes(movedDaysOff.RemovedDaysOff, teamInfo);
				checkPeriodValue = true;
				return true;
			}

			if (!_teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamInfo))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(movedDaysOff.AddedDaysOff, teamInfo);
				checkPeriodValue = false;
				return true;
			}

			checkPeriodValue = true;
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
		                                       IEnumerable<DateOnly> removedDaysOff,
		                                       ISchedulePartModifyAndRollbackService rollbackService,
		                                       IResourceCalculateDelayer resourceCalculateDelayer,
												ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                         schedulingOptions
					                                                                         .BlockFinderTypeForAdvanceScheduling,
				                                                                         _teamBlockSchedulingOptions
					                                                                         .IsSingleAgentTeam(schedulingOptions));
				if (teamBlockInfo == null)
					continue;
				if (!_teamTeamBlockSteadyStateValidator.IsTeamBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				bool success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective());
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