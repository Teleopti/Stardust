

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockClearer
	{
		void ClearTeamBlock(ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			ITeamBlockInfo teamBlock);

		void ClearTeamBlockWithNoResourceCalculation(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlock);
	}

	

	public class TeamBlockClearer : ITeamBlockClearer
	{
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

		public TeamBlockClearer(IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
		{
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
		}

		public void ClearTeamBlock(ISchedulingOptions schedulingOptions,
		                           ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                           ITeamBlockInfo teamBlock)
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();

			var selectedTeamMembers = teamBlock.TeamInfo.UnLockedMembers();
			var unlockedDates = teamBlock.BlockInfo.UnLockedDates();

			foreach (var person in selectedTeamMembers)
			{
				addDaysToRemove(teamBlock, unlockedDates, person, toRemove);
			}

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(toRemove,
			                                                                 schedulePartModifyAndRollbackService,
			                                                                 schedulingOptions.ConsiderShortBreaks);
		}

		public void ClearTeamBlockWithNoResourceCalculation(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlock)
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();

			var selectedTeamMembers = teamBlock.TeamInfo.UnLockedMembers();
			var unlockedDates = teamBlock.BlockInfo.UnLockedDates();
			foreach (var person in selectedTeamMembers)
			{
				addDaysToRemove(teamBlock, unlockedDates, person, toRemove);
			}

			_deleteAndResourceCalculateService.DeleteWithoutResourceCalculation(toRemove,
				schedulePartModifyAndRollbackService);
		}

		private static void addDaysToRemove(ITeamBlockInfo teamBlock, IList<DateOnly> unlockedDates , IPerson person, IList<IScheduleDay> toRemove)
		{
			foreach (var dateOnly in unlockedDates)
			{
				IScheduleMatrixPro matrix = teamBlock.TeamInfo.MatrixForMemberAndDate(person, dateOnly);
				if (matrix == null)
					continue;

				IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
				if (!matrix.UnlockedDays.Contains(scheduleDayPro))
					continue;

				IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
				SchedulePartView significant = scheduleDay.SignificantPart();
				if(significant == SchedulePartView.MainShift)
					toRemove.Add(scheduleDay);
			}
		}
	}
}
