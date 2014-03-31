

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
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
        private readonly ITeamMemberTerminationOnBlockSpecification _teamMemberTerminationOnBlockSpecification;

	    public TeamBlockInfoFactory(IDynamicBlockFinder dynamicBlockFinder, ITeamInfoFactory teamInfoFactory, ITeamMemberTerminationOnBlockSpecification teamMemberTerminationOnBlockSpecification)
		{
			_dynamicBlockFinder = dynamicBlockFinder;
			_teamInfoFactory = teamInfoFactory;
	        _teamMemberTerminationOnBlockSpecification = teamMemberTerminationOnBlockSpecification;
		}

		public ITeamBlockInfo CreateTeamBlockInfo(ITeamInfo teamInfo, DateOnly dateOnPeriod, BlockFinderType blockType,bool  singleAgentTeam, IList<IScheduleMatrixPro> allPersonMatrixList)
		{
			if (teamInfo == null)
				return null;

			IBlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(dateOnPeriod, teamInfo, blockType,singleAgentTeam);
			
            if (blockInfo == null)
				return null;

		    if (!_teamMemberTerminationOnBlockSpecification.IsSatisfy(teamInfo, blockInfo)) return null;

			var teamInfoForBlockPeriod = _teamInfoFactory.CreateTeamInfo(teamInfo.GroupPerson.GroupMembers.First(), blockInfo.BlockPeriod, allPersonMatrixList);
			if (teamInfoForBlockPeriod == null)
			{
				return null;
			}

			return new TeamBlockInfo(teamInfoForBlockPeriod, blockInfo);
		}
	}
}