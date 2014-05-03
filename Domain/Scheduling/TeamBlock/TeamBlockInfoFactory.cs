using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockInfoFactory
	{
		ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType,
			bool singleAgentTeam);
	}

	public class TeamBlockInfoFactory : ITeamBlockInfoFactory
	{
		private readonly IDynamicBlockFinder _dynamicBlockFinder;
        private readonly ITeamMemberTerminationOnBlockSpecification _teamMemberTerminationOnBlockSpecification;

	    public TeamBlockInfoFactory(IDynamicBlockFinder dynamicBlockFinder, ITeamMemberTerminationOnBlockSpecification teamMemberTerminationOnBlockSpecification)
		{
			_dynamicBlockFinder = dynamicBlockFinder;
	        _teamMemberTerminationOnBlockSpecification = teamMemberTerminationOnBlockSpecification;
		}

		public ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType,
			bool singleAgentTeam)
		{
			if (teamInfo == null)
				return null;

			IBlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(dateOnPeriod, teamInfo, blockType, singleAgentTeam);

			if (blockInfo == null)
				return null;

			if (!_teamMemberTerminationOnBlockSpecification.IsSatisfy(teamInfo, blockInfo))
				return null;

			return new TeamBlockInfo(teamInfo, blockInfo);
		}
	}
}