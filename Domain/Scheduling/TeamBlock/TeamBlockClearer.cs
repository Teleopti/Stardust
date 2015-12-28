

using System.Collections.Generic;
using System.Linq;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void ClearTeamBlock(ISchedulingOptions schedulingOptions,
		                           ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                           ITeamBlockInfo teamBlock)
		{
			var selectedTeamMembers = teamBlock.TeamInfo.UnLockedMembers();
			var toRemove = selectedTeamMembers.SelectMany(s => addDaysToRemove(teamBlock, s)).ToList();

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(toRemove,
			                                                                 schedulePartModifyAndRollbackService,
			                                                                 schedulingOptions.ConsiderShortBreaks);
		}

		public void ClearTeamBlockWithNoResourceCalculation(
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ITeamBlockInfo teamBlock)
		{
			var selectedTeamMembers = teamBlock.TeamInfo.UnLockedMembers();
			var toRemove = selectedTeamMembers.SelectMany(s => addDaysToRemove(teamBlock, s)).ToList();
			
			_deleteAndResourceCalculateService.DeleteWithoutResourceCalculation(toRemove,
				schedulePartModifyAndRollbackService);
		}

		private IEnumerable<IScheduleDay> addDaysToRemove(ITeamBlockInfo teamBlock, IPerson person)
		{
			foreach (var dateOnly in teamBlock.BlockInfo.UnLockedDates())
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
					yield return scheduleDay;
			}
		}
	}
}
