

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfoFactory
	{
        ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType, bool singleAgentTeam);
	}

	public class TeamBlockInfoFactory : ITeamBlockInfoFactory
	{
		private readonly IDynamicBlockFinder _dynamicBlockFinder;

		public TeamBlockInfoFactory(IDynamicBlockFinder dynamicBlockFinder)
		{
			_dynamicBlockFinder = dynamicBlockFinder;
		}

		public ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType,bool  singleAgentTeam)
		{
			if (teamInfo == null)
				return null;

			IBlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(dateOnPeriod, teamInfo, blockType,singleAgentTeam);
			
			if (blockInfo == null)
				return null;

			return new TeamBlockInfo(teamInfo, blockInfo);
		}
	}
}