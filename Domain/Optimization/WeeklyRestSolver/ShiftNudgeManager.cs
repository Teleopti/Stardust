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
		bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith,
			ITeamBlockGenerator teamBlockGenerator, IList<IScheduleMatrixPro> allPersonMatrixList,
			ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);
	}

	public class ShiftNudgeManager : IShiftNudgeManager
	{
		private readonly IShiftNudgeEarlier _shiftNudgeEarlier;
		private readonly IShiftNudgeLater _shiftNudgeLater;
		private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private readonly ITeamBlockScheduleCloner _teamBlockScheduleCloner;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;

		public ShiftNudgeManager(IShiftNudgeEarlier shiftNudgeEarlier, IShiftNudgeLater shiftNudgeLater,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			ITeamBlockScheduleCloner teamBlockScheduleCloner, IFilterForTeamBlockInSelection filterForTeamBlockInSelection)
		{
			_shiftNudgeEarlier = shiftNudgeEarlier;
			_shiftNudgeLater = shiftNudgeLater;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
			_teamBlockScheduleCloner = teamBlockScheduleCloner;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
		}

		public bool TrySolveForDayOff(PersonWeek personWeek, DateOnly dayOffDateToWorkWith,
			ITeamBlockGenerator teamBlockGenerator, IList<IScheduleMatrixPro> allPersonMatrixList,
			ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
		{
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
			var clonedSchedules = cloneSchedules(leftTeamBlock, rightTeamBlock);

			var weeklyRestTime = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
			var personRange = leftTeamBlock.TeamInfo.MatrixForMemberAndDate(person, leftDate).ActiveScheduleRange;
			var leftScheduleDay = personRange.ScheduledDay(leftDate);
			var rightScheduleDay = personRange.ScheduledDay(rightDate);
			bool restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			bool leftNudgeSuccess = true;
			bool rightNudgeSuccess = true;
			while (!restTimeEnsured)
			{
				if(leftNudgeSuccess)
				{
					leftNudgeSuccess =_shiftNudgeEarlier.Nudge(leftScheduleDay, rollbackService, schedulingOptions, resourceCalculateDelayer, leftTeamBlock,
						schedulingResultStateHolder, selectedPeriod, selectedPersons);
					restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
				}

				if (rightNudgeSuccess && !restTimeEnsured)
				{
					rightNudgeSuccess = _shiftNudgeLater.Nudge(rightScheduleDay, rollbackService, schedulingOptions,
						resourceCalculateDelayer, rightTeamBlock, schedulingResultStateHolder, selectedPeriod, selectedPersons);
					restTimeEnsured = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
				}

				if(!leftNudgeSuccess && !rightNudgeSuccess)
					break;
			}

			bool success = _ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personRange, weeklyRestTime);
			if (!success)
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
				return false;
			}

			//validate broken restrictions

			return true;
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