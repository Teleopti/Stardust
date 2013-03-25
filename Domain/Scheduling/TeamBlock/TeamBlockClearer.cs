

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
				IScheduleRange range = _stateHolder.Schedules[person];
				foreach (var dateOnly in teamBlock.BlockInfo.BlockPeriod.DayCollection())
				{
					IScheduleDay scheduleDay = range.ScheduledDay(dateOnly);
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
