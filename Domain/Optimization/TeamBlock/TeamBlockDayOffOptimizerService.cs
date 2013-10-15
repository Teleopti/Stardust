using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList,
		                     DateOnlyPeriod selectedPeriod,
		                     IList<IPerson> selectedPersons,
		                     IOptimizationPreferences optimizationPreferences,
		                     ISchedulePartModifyAndRollbackService rollbackService,
		                     IDayOffTemplate dayOffTemplate);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ITeamDayOffModifier _teamDayOffModifier;
		private readonly ITeamBlockSteadyStateValidator _teamTeamBlockSteadyStateValidator;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockRestrictionOverLimitValidator _restrictionOverLimitValidator;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private bool _cancelMe;

		public TeamBlockDayOffOptimizerService(
			ITeamInfoFactory teamInfoFactory,
			ILockableBitArrayFactory lockableBitArrayFactory,
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
			ISmartDayOffBackToLegalStateService daysOffBackToLegal,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			ILockableBitArrayChangesTracker lockableBitArrayChangesTracker,
			ITeamBlockScheduler teamBlockScheduler,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			IPeriodValueCalculator periodValueCalculatorForAllSkills,
			IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			ITeamDayOffModifier teamDayOffModifier,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockClearer teamBlockClearer,
			ITeamBlockRestrictionOverLimitValidator restrictionOverLimitValidator,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker
			)
		{
			_teamInfoFactory = teamInfoFactory;
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_daysOffBackToLegal = daysOffBackToLegal;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_lockableBitArrayChangesTracker = lockableBitArrayChangesTracker;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_teamDayOffModifier = teamDayOffModifier;
			_teamTeamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockClearer = teamBlockClearer;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IDayOffTemplate dayOffTemplate
			)
		{
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			schedulingOptions.DayOffTemplate = dayOffTemplate;
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList);
				if (teamInfo != null)
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			// find a random selected TeamInfo/matrix
			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

			while (remainingInfoList.Count > 0)
			{
				IEnumerable<ITeamInfo> teamInfosToRemove;
				if(optimizationPreferences.Extra.KeepSameDaysOffInTeam)
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
					rollbackService.ClearModificationCollection();
					bool failed = false;
					foreach (var matrix in teamInfo.MatrixesForGroup())
					{
						var success = runOneMatrixOnly(optimizationPreferences, rollbackService, matrix, schedulingOptions, teamInfo,
												   selectedPeriod, selectedPersons, allPersonMatrixList);
						if (!success)
							failed = true;
					}

					double currentPeriodValue =
							_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

					if (currentPeriodValue >= previousPeriodValue || failed)
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
				bool anySuccess = false;

				if (teamInfo.GroupPerson.GroupMembers.Any())
				{
					foreach (IScheduleMatrixPro matrix in teamInfo.MatrixesForGroupMember(0))
					{
						rollbackService.ClearModificationCollection();
						var success = runOneTeam(optimizationPreferences, rollbackService, schedulingOptions, matrix, teamInfo,
						                           selectedPeriod, selectedPersons, allPersonMatrixList);

						double currentPeriodValue =
							_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

						if (currentPeriodValue >= previousPeriodValue || !success)
						{
							_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
							currentPeriodValue =
								_periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
						}
						else
						{
							anySuccess = true;
						}
						previousPeriodValue = currentPeriodValue;

						OnReportProgress(Resources.OptimizingDaysOff + Resources.Colon + "(" + totalLiveTeamInfos.ToString("####") + ")(" +
						                 currentTeamInfoCounter.ToString("####") + ") " +
						                 StringHelper.DisplayString(teamInfo.GroupPerson.Name.ToString(), 20) + " (" + currentPeriodValue +
						                 ")");
						if (_cancelMe)
							break;
					}
				}
				if (!anySuccess)
				{
					teamInfosToRemove.Add(teamInfo);
				}
				
			}

			return teamInfosToRemove;
		}

		private bool runOneMatrixOnly(IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			bool considerWeekBefore = optimizationPreferences.DaysOff.ConsiderWeekBefore;
			bool considerWeekAfter = optimizationPreferences.DaysOff.ConsiderWeekAfter;
			ILockableBitArray originalArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
																						 matrix);
			ILockableBitArray resultingArray = tryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return false;

			// find out what have changed, Does the predictor beleve in this? depends on how many members
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
																						considerWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray, matrix,
																							considerWeekBefore);

			IPerson person = matrix.Person;
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForMember(rollbackService, person, dateOnly);
			}

			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForMember(rollbackService, person, dateOnly, schedulingOptions.DayOffTemplate, true);
			}

			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, selectedPeriod, selectedPersons, removedDaysOff, rollbackService, allPersonMatrixList);
			if (!success)
				return false;

			var isMaxSeatRuleViolated = addedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
										 removedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions));
			if (isMaxSeatRuleViolated || !_restrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(addedDaysOff, teamInfo);
				lockDaysInMatrixes(removedDaysOff, teamInfo);

				return true;
			}

			return true;
		}

		private bool runOneTeam(IOptimizationPreferences optimizationPreferences,
		                          ISchedulePartModifyAndRollbackService rollbackService,
		                          ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix,
		                          ITeamInfo teamInfo, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons
			, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			bool considerWeekBefore = optimizationPreferences.DaysOff.ConsiderWeekBefore;
			bool considerWeekAfter = optimizationPreferences.DaysOff.ConsiderWeekAfter;
			ILockableBitArray originalArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
			                                                                             matrix);
			ILockableBitArray resultingArray = tryFindMoves(matrix, originalArray, optimizationPreferences);

			if (resultingArray.Equals(originalArray))
				return false;

			// find out what have changed, Does the predictor beleve in this? depends on how many members
			IList<DateOnly> addedDaysOff = _lockableBitArrayChangesTracker.DaysOffAdded(resultingArray, originalArray, matrix,
			                                                                            considerWeekBefore);
			IList<DateOnly> removedDaysOff = _lockableBitArrayChangesTracker.DaysOffRemoved(resultingArray, originalArray, matrix,
			                                                                                considerWeekBefore);

			removeAllDecidedDaysOff(rollbackService, teamInfo, removedDaysOff);
			addAllDecidedDaysOf(rollbackService, schedulingOptions, teamInfo, addedDaysOff);
			bool success = reScheduleAllMovedDaysOff(schedulingOptions, teamInfo, selectedPeriod, selectedPersons, removedDaysOff, rollbackService, allPersonMatrixList);
			if (!success)
				return false;

			var isMaxSeatRuleViolated = addedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions)) ||
										 removedDaysOff.Any(x => !_teamBlockMaxSeatChecker.CheckMaxSeat(x, schedulingOptions));
			if (isMaxSeatRuleViolated || !_restrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences))
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				lockDaysInMatrixes(addedDaysOff, teamInfo);
				lockDaysInMatrixes(removedDaysOff, teamInfo);

				return true;
			}

			return true;
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
                bool singleAgentTeam = schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                                           schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";

                ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly,
				                                                                        schedulingOptions
                                                                                            .BlockFinderTypeForAdvanceScheduling, 
																							singleAgentTeam,
																							allPersonMatrixList);
				if (teamBlockInfo == null) continue;
				if (!_teamTeamBlockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, schedulingOptions))
					_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);

				bool success = _teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, selectedPeriod, selectedPersons);
				if (!success)
					return false;
			}

			return true;
		}

		private void addAllDecidedDaysOf(ISchedulePartModifyAndRollbackService rollbackService,
		                                 ISchedulingOptions schedulingOptions, ITeamInfo teamInfo,
		                                 IEnumerable<DateOnly> addedDaysOff)
		{
			foreach (DateOnly dateOnly in addedDaysOff)
			{
				_teamDayOffModifier.AddDayOffForTeamAndResourceCalculate(rollbackService, teamInfo, dateOnly, schedulingOptions.DayOffTemplate);
			}
		}

		private void removeAllDecidedDaysOff(ISchedulePartModifyAndRollbackService rollbackService,
		                                     ITeamInfo teamInfo,
		                                     IEnumerable<DateOnly> removedDaysOff)
		{
			foreach (DateOnly dateOnly in removedDaysOff)
			{
				_teamDayOffModifier.RemoveDayOffForTeam(rollbackService, teamInfo, dateOnly);
			}
		}

		//to new class
		private ILockableBitArray tryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
		                                       IOptimizationPreferences optimizationPreferences)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers =
				_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences);
			foreach (IDayOffDecisionMaker dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray) originalArray.Clone();
				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}
	}
}