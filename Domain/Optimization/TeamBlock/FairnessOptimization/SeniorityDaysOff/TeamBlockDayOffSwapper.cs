using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ITeamBlockDayOffSwapper
	{
		bool TrySwap(ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary);
	}
	public class TeamBlockDayOffSwapper : ITeamBlockDayOffSwapper
	{
		private readonly ISwapServiceNew _swapServiceNew;

		public TeamBlockDayOffSwapper(ISwapServiceNew swapServiceNew)
		{
			_swapServiceNew = swapServiceNew;
		}

		public bool TrySwap(ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
						 ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary)
		{
			var totalModifyList = new List<IScheduleDay>();
			var teamBlockSeniorGroupMembers = teamBlockSenior.TeamInfo.GroupMembers.ToList();
			var teamBlockJuniorGroupMembers = teamBlockJunior.TeamInfo.GroupMembers.ToList();
			for (int i = 0; i < teamBlockSeniorGroupMembers.Count(); i++)
			{
				foreach (var dateOnly in teamBlockSenior.BlockInfo.BlockPeriod.DayCollection())
				{
					var personSenior = teamBlockSeniorGroupMembers[i];
					var personJunior = teamBlockJuniorGroupMembers[i];
					var day1 = scheduleDictionary[personSenior].ScheduledDay(dateOnly);
					var day2 = scheduleDictionary[personJunior].ScheduledDay(dateOnly);
					if (isOneDayOff(day1, day2))
						totalModifyList.AddRange(_swapServiceNew.Swap(new List<IScheduleDay> { day1, day2 }, scheduleDictionary));
				}
			}

			rollbackService.ClearModificationCollection();
			var modifyResults = rollbackService.ModifyParts(totalModifyList);
			if (modifyResults.Any())
			{
				rollbackService.Rollback();
				return false;
			}
			rollbackService.ClearModificationCollection();

			return true;
		}

		private static bool isOneDayOff(IScheduleDay day1, IScheduleDay day2)
		{
			var significantPartOfFirstDay = day1.SignificantPart();
			var significantPartOfSecondDay = day2.SignificantPart();
			return significantPartOfFirstDay == SchedulePartView.DayOff && significantPartOfSecondDay != SchedulePartView.DayOff ||
				   significantPartOfFirstDay != SchedulePartView.DayOff && significantPartOfSecondDay == SchedulePartView.DayOff;
		}
	}
}
