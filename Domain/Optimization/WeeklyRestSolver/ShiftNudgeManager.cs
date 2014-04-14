using System.Collections.Generic;
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
		bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith, ITeamBlockGenerator teamBlockGenerator, IList<IScheduleMatrixPro> allPersonMatrixList, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions);

	    bool RollbackLastScheduledWeek(ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class ShiftNudgeManager : IShiftNudgeManager
	{
		private readonly IShiftNudgeEarlier _shiftNudgeEarlier;
		private readonly IShiftNudgeLater _shiftNudgeLater;
		private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private readonly ITeamBlockScheduleCloner _teamBlockScheduleCloner;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
	        private IList<IScheduleDay> _clonedSchedules;

	    public ShiftNudgeManager(IShiftNudgeEarlier shiftNudgeEarlier, IShiftNudgeLater shiftNudgeLater,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			ITeamBlockScheduleCloner teamBlockScheduleCloner, IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
			ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator, ISchedulingOptionsCreator schedulingOptionsCreator)
		{
			_shiftNudgeEarlier = shiftNudgeEarlier;
			_shiftNudgeLater = shiftNudgeLater;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
			_teamBlockScheduleCloner = teamBlockScheduleCloner;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
			_schedulingOptionsCreator = schedulingOptionsCreator;
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

			var leftTeamBlock =
				teamBlockGenerator.Generate(allPersonMatrixList, new DateOnlyPeriod(leftDate, leftDate), new List<IPerson> {person},
					schedulingOptions).First();
			var rightTeamBlock =
				teamBlockGenerator.Generate(allPersonMatrixList, new DateOnlyPeriod(rightDate, rightDate),
					new List<IPerson> {person}, schedulingOptions).First();

			var fullySelectedBlocks =
				_filterForTeamBlockInSelection.Filter(new List<ITeamBlockInfo> {leftTeamBlock, rightTeamBlock}, selectedPersons,
					selectedPeriod);
			if (fullySelectedBlocks.Count != 2)
				return false;

			//first clone the left and right teamblocks if we have to roll back
			_clonedSchedules = cloneSchedules(leftTeamBlock, rightTeamBlock);

			var weeklyRestTime = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
			var personRange = leftTeamBlock.TeamInfo.MatrixForMemberAndDate(person, leftDate).ActiveScheduleRange;
			bool restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			bool leftNudgeSuccess = true;
			bool rightNudgeSuccess = true;
			while (!restTimeEnsured)
			{
				var leftScheduleDay = personRange.ScheduledDay(leftDate);
				var rightScheduleDay = personRange.ScheduledDay(rightDate);
				if (leftNudgeSuccess)
				{
					leftNudgeSuccess = _shiftNudgeEarlier.Nudge(leftScheduleDay, rollbackService, schedulingOptions,
						resourceCalculateDelayer, leftTeamBlock,
						schedulingResultStateHolder, selectedPeriod, selectedPersons);
					restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
				}

				if (rightNudgeSuccess && !restTimeEnsured)
				{
					rightNudgeSuccess = _shiftNudgeLater.Nudge(rightScheduleDay, rollbackService, schedulingOptions,
						resourceCalculateDelayer, rightTeamBlock, schedulingResultStateHolder, selectedPeriod, selectedPersons);
					restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
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
				leftOk = _teamBlockRestrictionOverLimitValidator.Validate(leftTeamBlock, optimizationPreferences);
				rightOk = _teamBlockRestrictionOverLimitValidator.Validate(rightTeamBlock, optimizationPreferences);
			}
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

		
	}
}