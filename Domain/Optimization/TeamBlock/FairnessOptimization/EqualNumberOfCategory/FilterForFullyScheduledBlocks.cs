﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForFullyScheduledBlocks
	{
		IList<ITeamBlockInfo> IsFullyScheduled(IList<ITeamBlockInfo> teamBlockList, IScheduleDictionary scheduleDictionary);
	}

	public class FilterForFullyScheduledBlocks : IFilterForFullyScheduledBlocks
	{
		public IList<ITeamBlockInfo> IsFullyScheduled(IList<ITeamBlockInfo> teamBlockList,
		                                              IScheduleDictionary scheduleDictionary)
		{
			var returnList = new List<ITeamBlockInfo>();
			foreach (var teamBlock in teamBlockList)
			{
				bool unscheduledFound = false;
				foreach (var dateOnly in teamBlock.BlockInfo.BlockPeriod.DayCollection())
				{
					foreach (var groupMember in teamBlock.TeamInfo.GroupPerson.GroupMembers)
					{
						if (scheduleDictionary[groupMember].ScheduledDay(dateOnly).SignificantPart() != SchedulePartView.MainShift)
						{
							unscheduledFound = true;
							break;
						}

					}

					if (unscheduledFound)
					{
						break;
					}
				}

				if(!unscheduledFound)
					returnList.Add(teamBlock);
			}

			return returnList;
		}
	}
}