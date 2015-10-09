using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IShiftNudgeManager
	{
		bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith, ITeamBlockGenerator teamBlockGenerator,
			IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions);

		bool RollbackLastScheduledWeek(ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class ShiftNudgeManager : IShiftNudgeManager
	{
		private readonly IShiftNudgeEarlier _shiftNudgeEarlier;
		private readonly IShiftNudgeLater _shiftNudgeLater;
		private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private readonly ITeamBlockScheduleCloner _teamBlockScheduleCloner;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly IScheduleDayIsLockedSpecification _scheduleDayIsLockedSpecification;
		private IList<IScheduleDay> _clonedSchedules;

		public ShiftNudgeManager(IShiftNudgeEarlier shiftNudgeEarlier, IShiftNudgeLater shiftNudgeLater,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			ITeamBlockScheduleCloner teamBlockScheduleCloner, IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
			ITeamBlockOptimizationLimits teamBlockOptimizationLimits, ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator, IScheduleDayIsLockedSpecification scheduleDayIsLockedSpecification)
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
		}

		public bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith,
			ITeamBlockGenerator teamBlockGenerator, IList<IScheduleMatrixPro> allPersonMatrixList,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder, DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
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

			var possibleLeftTeamBlocks = teamBlockGenerator.Generate(allPersonMatrixList, new DateOnlyPeriod(leftDate, leftDate),
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

			var possibleRightTeamBlocks = teamBlockGenerator.Generate(allPersonMatrixList,
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
			while (!restTimeEnsured)
			{
				var leftScheduleDay = personRange.ScheduledDay(leftDate);
				if (isDaysLocked(leftScheduleDay, personMatrix))
					leftNudgeSuccess = false;

				if (leftNudgeSuccess)
				{
					leftNudgeSuccess = _shiftNudgeEarlier.Nudge(leftScheduleDay, rollbackService, schedulingOptions, resourceCalculateDelayer, leftTeamBlock, schedulingResultStateHolder, optimizationPreferences);
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
							schedulingOptions, resourceCalculateDelayer, 
							rightTeamBlock, schedulingResultStateHolder, optimizationPreferences);
						restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
					}
				}

				if (!leftNudgeSuccess && !rightNudgeSuccess)
					break;
			}

			bool success = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			if (!success)
			{
				rollBackAndResourceCalculate(rollbackService, resourceCalculateDelayer, _clonedSchedules);
				return false;
			}

			bool leftOk = true;
			bool rightOk = true;
			if (optimizationPreferences != null)
			{
				leftOk = _teamBlockOptimizationLimits.Validate(leftTeamBlock, optimizationPreferences);
				rightOk = _teamBlockOptimizationLimits.Validate(rightTeamBlock, optimizationPreferences);
			}
			if (!(leftOk && rightOk))
			{
				rollBackAndResourceCalculate(rollbackService, resourceCalculateDelayer, _clonedSchedules);
				return false;
			}

			leftOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(leftTeamBlock);
			rightOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(rightTeamBlock);

			if (!(leftOk && rightOk))
			{
				rollBackAndResourceCalculate(rollbackService, resourceCalculateDelayer, _clonedSchedules);
				return false;
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

		private void lockUnSelectedInTeamBlock(ITeamBlockInfo teamBlockInfo, IList<IPerson> selectedPersons,
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
					teamInfo.LockMember(groupMember);
			}
		}

		private static void rollBackAndResourceCalculate(ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleDay> clonedSchedules)
		{
			rollbackService.ModifyParts(clonedSchedules);
			rollbackService.ClearModificationCollection();
			var dateList = new HashSet<DateOnly>();
			foreach (var cloneSchedule in clonedSchedules)
			{
				var dateOnly = cloneSchedule.DateOnlyAsPeriod.DateOnly;
				dateList.Add(dateOnly);
				dateList.Add(dateOnly.AddDays(1));
			}
			foreach (var dateOnly in dateList)
			{
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
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