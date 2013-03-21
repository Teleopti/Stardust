

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfoFactory
	{
		TeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType);
	}

	public class TeamBlockInfoFactory : ITeamBlockInfoFactory
	{
		private readonly IDynamicBlockFinder _dynamicBlockFinder;

		public TeamBlockInfoFactory(IDynamicBlockFinder dynamicBlockFinder)
		{
			_dynamicBlockFinder = dynamicBlockFinder;
		}

		public TeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType)
		{
			if (teamInfo == null)
				return null;

			IBlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(dateOnPeriod, teamInfo, blockType);
			
			if (blockInfo == null)
				return null;

			return new TeamBlockInfo(teamInfo, blockInfo);
		}
	}
}