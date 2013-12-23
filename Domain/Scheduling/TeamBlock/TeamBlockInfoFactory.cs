

using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfoFactory
	{
		ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType, bool singleAgentTeam, IList<IScheduleMatrixPro> allPersonMatrixList);
	}

	public class TeamBlockInfoFactory : ITeamBlockInfoFactory
	{
		private readonly IDynamicBlockFinder _dynamicBlockFinder;
		private readonly ITeamInfoFactory _teamInfoFactory;

		public TeamBlockInfoFactory(IDynamicBlockFinder dynamicBlockFinder, ITeamInfoFactory teamInfoFactory)
		{
			_dynamicBlockFinder = dynamicBlockFinder;
			_teamInfoFactory = teamInfoFactory;
		}

		public ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType,bool  singleAgentTeam, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			if (teamInfo == null)
				return null;

			IBlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(dateOnPeriod, teamInfo, blockType,singleAgentTeam);
			
			if (blockInfo == null)
				return null;

			var teamInfoForBlockPeriod = _teamInfoFactory.CreateTeamInfo(teamInfo.GroupMembers.First(), blockInfo.BlockPeriod, allPersonMatrixList);
			if (teamInfoForBlockPeriod == null)
			{
				return null;
			}

			return new TeamBlockInfo(teamInfoForBlockPeriod, blockInfo);
		}
	}
}