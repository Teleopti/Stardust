

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface ITeamBlockSwapper
	{
		bool TrySwap(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2,
		                          ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary);
	}

	public class TeamBlockSwapper : ITeamBlockSwapper
	{
		private readonly ISwapServiceNew _swapServiceNew;

		public TeamBlockSwapper(ISwapServiceNew swapServiceNew)
		{
			_swapServiceNew = swapServiceNew;
		}

		public bool TrySwap(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2,
		                 ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary)
		{
			List<IScheduleDay> totalModifyList = new List<IScheduleDay>();
			var teamBlock1GroupMembers = teamBlock1.TeamInfo.GroupPerson.GroupMembers.ToList();
			for (int i = 0; i < teamBlock1GroupMembers.Count(); i++)
			{
				foreach (var dateOnly in teamBlock1.BlockInfo.BlockPeriod.DayCollection())
				{
					var person1 = teamBlock1GroupMembers[i];
					var person2 = teamBlock2.TeamInfo.GroupPerson.GroupMembers.ToList()[i];
					var day1 = scheduleDictionary[person1].ScheduledDay(dateOnly);
					var day2 = scheduleDictionary[person2].ScheduledDay(dateOnly);
					totalModifyList.AddRange(_swapServiceNew.Swap(new List<IScheduleDay> {day1, day2}, scheduleDictionary));
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
	}
}