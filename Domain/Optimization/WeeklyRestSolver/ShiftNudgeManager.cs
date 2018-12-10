using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IShiftNudgeManager
	{
		bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith, ITeamBlockGenerator teamBlockGenerator,
			IEnumerable<IScheduleMatrixPro> allPersonMatrixList, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);

		bool RollbackLastScheduledWeek(ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class ShiftNudgeManager : IShiftNudgeManager
	{
		private readonly ShiftNudgeEarlier _shiftNudgeEarlier;
		private readonly ShiftNudgeLater _shiftNudgeLater;
		private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private readonly TeamBlockScheduleCloner _teamBlockScheduleCloner;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private IList<IScheduleDay> _clonedSchedules;

		public ShiftNudgeManager(ShiftNudgeEarlier shiftNudgeEarlier, ShiftNudgeLater shiftNudgeLater,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			TeamBlockScheduleCloner teamBlockScheduleCloner, IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits, ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator, IScheduleDayIsLockedSpecification scheduleDayIsLockedSpecification,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_shiftNudgeEarlier = shiftNudgeEarlier;
			_shiftNudgeLater = shiftNudgeLater;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
			_teamBlockScheduleCloner = teamBlockScheduleCloner;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_scheduleDayIsLockedSpecification = scheduleDayIsLockedSpecification;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith,
			ITeamBlockGenerator teamBlockGenerator, IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (optimizationPreferences != null)
				schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

			var teamSchedulingOptions = new TeamBlockSchedulingOptions();
			if (teamSchedulingOptions.IsBlockScheduling(schedulingOptions) &&
			    schedulingOptions.BlockFinderTypeForAdvanceScheduling == BlockFinderType.SchedulePeriod)
				return false;

			var person = personWeek.Person;
			var leftDate = dayOffDateToWorkWith.AddDays(-1);
			var rightDate = dayOffDateToWorkWith.AddDays(1);

			var possibleLeftTeamBlocks = teamBlockGenerator.Generate(schedulingResultStateHolder.LoadedAgents, allPersonMatrixList, new DateOnlyPeriod(leftDate, leftDate),
				new List<IPerson> {person},
				schedulingOptions);
			var filteredPossibleLeftTeamBlocks = new List<ITeamBlockInfo>();
			foreach (var possibleLeftTeamBlock in possibleLeftTeamBlocks)
			{
				if (_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(possibleLeftTeamBlock, schedulingOptions))
					filteredPossibleLeftTeamBlocks.Add(possibleLeftTeamBlock);

			}
			ITeamBlockInfo  leftTeamBlock =null;
			if (filteredPossibleLeftTeamBlocks.Any())
			{
				leftTeamBlock = filteredPossibleLeftTeamBlocks.First();
				lockUnSelectedInTeamBlock(leftTeamBlock, selectedPersons, selectedPeriod);
			}

			var possibleRightTeamBlocks = teamBlockGenerator.Generate(schedulingResultStateHolder.LoadedAgents, allPersonMatrixList,
				new DateOnlyPeriod(rightDate, rightDate),
				new List<IPerson> {person}, schedulingOptions);
			ITeamBlockInfo rightTeamBlock = null;
			var filteredPossibleRightTeamBlocks = new List<ITeamBlockInfo>();
			foreach (var possibleRightTeamBlock in possibleRightTeamBlocks)
			{
				if(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(possibleRightTeamBlock, schedulingOptions))
					filteredPossibleRightTeamBlocks.Add(possibleRightTeamBlock);
			}

			if (filteredPossibleRightTeamBlocks.Any())
			{
				rightTeamBlock = filteredPossibleRightTeamBlocks.First();
				lockUnSelectedInTeamBlock(rightTeamBlock, selectedPersons, selectedPeriod);
			}

			if (leftTeamBlock == null || rightTeamBlock == null) return false;
			var fullySelectedBlocks =
				_filterForTeamBlockInSelection.Filter(new List<ITeamBlockInfo> {leftTeamBlock, rightTeamBlock}, selectedPersons,
					selectedPeriod);
			if (fullySelectedBlocks.Count == 0)
				return false;

			//first clone the left and right teamblocks if we have to roll back
			_clonedSchedules = cloneSchedules(leftTeamBlock, rightTeamBlock);

			var weeklyRestTime = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
			var personRange = leftTeamBlock.TeamInfo.MatrixForMemberAndDate(person, leftDate).ActiveScheduleRange;
			var personMatrix = leftTeamBlock.TeamInfo.MatrixForMemberAndDate(person, leftDate);

			bool restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			bool leftNudgeSuccess = true;
			bool rightNudgeSuccess = true;
			var firstLeftNudge = true;
			var firstRightNudge = true;
			var rollbackServiceKeep = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));

			while (!restTimeEnsured)
			{
				var leftScheduleDay = personRange.ScheduledDay(leftDate);
				if (isDaysLocked(leftScheduleDay, personMatrix))
					leftNudgeSuccess = false;

				if (leftNudgeSuccess)
				{
					leftNudgeSuccess = _shiftNudgeEarlier.Nudge(leftScheduleDay, rollbackService, schedulingOptions,
						leftTeamBlock, schedulingResultStateHolder, optimizationPreferences, firstLeftNudge);
					firstLeftNudge = false;
					restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
				}

				if (rightNudgeSuccess && !restTimeEnsured)
				{
					var rightScheduleDay = personRange.ScheduledDay(rightDate);
					if (isDaysLocked(rightScheduleDay, personMatrix))
						rightNudgeSuccess = false;

					if (rightNudgeSuccess)
					{
						rightNudgeSuccess = _shiftNudgeLater.Nudge(rightScheduleDay, rollbackService, 
							schedulingOptions, rightTeamBlock, schedulingResultStateHolder, optimizationPreferences, firstRightNudge);
						firstRightNudge = false;
						restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
					}
				}

				if (!leftNudgeSuccess && !rightNudgeSuccess)
					break;
			}
			bool success = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			if (!success)
			{
				rollBackAndResourceCalculate(rollbackServiceKeep, resourceCalculateDelayer, _clonedSchedules);
				return false;
			}

			bool leftOk = true;
			bool rightOk = true;
			if (optimizationPreferences != null)
			{
				leftOk = _teamBlockOptimizationLimits.Validate(leftTeamBlock, optimizationPreferences, dayOffOptimizationPreferenceProvider);
				rightOk = _teamBlockOptimizationLimits.Validate(rightTeamBlock, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			}
			if (!(leftOk && rightOk))
			{
				rollBackAndResourceCalculate(rollbackServiceKeep, resourceCalculateDelayer, _clonedSchedules);
				return false;
			}

			leftOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(leftTeamBlock);
			rightOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(rightTeamBlock);

			if (!(leftOk && rightOk))
			{
				rollBackAndResourceCalculate(rollbackServiceKeep, resourceCalculateDelayer, _clonedSchedules);
				return false;
			}

			foreach (var date in leftTeamBlock.BlockInfo.BlockPeriod.DayCollection())
			{
				resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
			}

			if(!firstRightNudge)
			{
				foreach (var date in rightTeamBlock.BlockInfo.BlockPeriod.DayCollection())
				{
					resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
				}
			}

			return true;
		}

		public  bool RollbackLastScheduledWeek(ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
	    {
	        if (_clonedSchedules != null && _clonedSchedules.Count > 0)
	        {
                rollBackAndResourceCalculate(rollbackService, resourceCalculateDelayer, _clonedSchedules);
                return true;
	        }
            return false;
	    }

		private void lockUnSelectedInTeamBlock(ITeamBlockInfo teamBlockInfo, IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod)
		{
			var blockInfo = teamBlockInfo.BlockInfo;
			blockInfo.ClearLocks();
			var teamInfo = teamBlockInfo.TeamInfo;
			teamInfo.ClearLocks();
			foreach (var dateOnly in blockInfo.BlockPeriod.DayCollection())
			{
				
				if(!selectedPeriod.Contains(dateOnly))
					blockInfo.LockDate(dateOnly);
			}
			foreach (var groupMember in teamInfo.GroupMembers)
			{
				if(!selectedPersons.Contains(groupMember))
					teamInfo.LockMember(selectedPeriod, groupMember);
			}
		}

		private static void rollBackAndResourceCalculate(ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleDay> clonedSchedules)
		{
			rollbackService.ModifyParts(clonedSchedules);
			rollbackService.ClearModificationCollection();
			var dateList = clonedSchedules
				.SelectMany(c => new[] {c.DateOnlyAsPeriod.DateOnly, c.DateOnlyAsPeriod.DateOnly.AddDays(1)}).Distinct();
			foreach (var dateOnly in dateList)
			{
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
			}
		}

		private IList<IScheduleDay> cloneSchedules(ITeamBlockInfo leftTeamBlock, ITeamBlockInfo rightTeamBlock)
		{
			var clonedSchedules = new List<IScheduleDay>();
			clonedSchedules.AddRange(_teamBlockScheduleCloner.CloneSchedules(leftTeamBlock));
			clonedSchedules.AddRange(_teamBlockScheduleCloner.CloneSchedules(rightTeamBlock));

			return clonedSchedules;
		}

		private bool isDaysLocked(IScheduleDay scheduleDay, IScheduleMatrixPro scheduleMatrixPro)
		{
			return _scheduleDayIsLockedSpecification.IsSatisfy(scheduleDay, scheduleMatrixPro);
		}
		
	}
}