using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSwapValidator
	{
		bool ValidateCanSwap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamBlockSwapValidator : ITeamBlockSwapValidator
	{
		
		private readonly ITeamMemberCountValidator _teamMemberCountValidator;
		private readonly ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		
		public TeamBlockSwapValidator(ITeamMemberCountValidator teamMemberCountValidator,  ITeamBlockContractTimeValidator teamBlockContractTimeValidator)
		{
			_teamMemberCountValidator = teamMemberCountValidator;
			_teamBlockContractTimeValidator = teamBlockContractTimeValidator;
		}

		public bool ValidateCanSwap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var result = _teamMemberCountValidator.ValidateMemberCount(teamBlockInfo1, teamBlockInfo2);
			if (!result) return false;

			return _teamBlockContractTimeValidator.ValidateContractTime(teamBlockInfo1, teamBlockInfo2);
		}
	}
}
