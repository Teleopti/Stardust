

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
	}

	public class TeamBlockClearer : ITeamBlockClearer
	{
		private readonly ISchedulingResultStateHolder _stateHolder;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

		public TeamBlockClearer(ISchedulingResultStateHolder stateHolder,
		                        IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
		{
			_stateHolder = stateHolder;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
		}

		public void ClearTeamBlock(ISchedulingOptions schedulingOptions,
		                           ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                           ITeamBlockInfo teamBlock)
		{
			IList<IScheduleDay> toRemove = new List<IScheduleDay>();

			foreach (var person in teamBlock.TeamInfo.GroupPerson.GroupMembers)
			{
				foreach (var dateOnly in teamBlock.BlockInfo.BlockPeriod.DayCollection())
				{
					IScheduleMatrixPro matrix = teamBlock.TeamInfo.MatrixesForMemberAndDate(person, dateOnly);
					if (matrix == null)
						continue;

					IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(dateOnly);
					if(!matrix.UnlockedDays.Contains(scheduleDayPro))
						continue;

					IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
					SchedulePartView significant = scheduleDay.SignificantPart();
					if (significant != SchedulePartView.FullDayAbsence && significant != SchedulePartView.DayOff &&
					    significant != SchedulePartView.ContractDayOff)
						toRemove.Add(scheduleDay);
				}
			}

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(toRemove,
			                                                                 schedulePartModifyAndRollbackService,
			                                                                 schedulingOptions.ConsiderShortBreaks);
		}
	}
}
