﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterForTeamBlockInSelection
	{
		IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockList, IList<IPerson> selectedPersons,
		                             DateOnlyPeriod selectedPeriod);
	}

	public class FilterForTeamBlockInSelection : IFilterForTeamBlockInSelection
	{
		public IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockList, IList<IPerson> selectedPersons, 
		                                    DateOnlyPeriod selectedPeriod)
		{
			var teamBlockListInSelection = new List<ITeamBlockInfo>();
			foreach (var teamBlockInfo in teamBlockList)
			{
				if(!selectedPeriod.Contains(teamBlockInfo.BlockInfo.BlockPeriod))
					continue;

				var memberNotFound = false;
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupMembers)
				{
					if (!selectedPersons.Contains(groupMember))
						memberNotFound = true;
				}

				if(memberNotFound)
					continue;

				teamBlockListInSelection.Add(teamBlockInfo);
			}

			return teamBlockListInSelection;
		}
	}
}