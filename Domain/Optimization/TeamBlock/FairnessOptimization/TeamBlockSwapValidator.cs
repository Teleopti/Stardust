using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public class TeamBlockSwapValidator
	{
		private readonly ITeamSelectionValidator _teamSelectionValidator;
		private readonly ITeamMemberCountValidator _teamMemberCountValidator;
		private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private readonly ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		
		public TeamBlockSwapValidator(ITeamSelectionValidator teamSelectionValidator, ITeamMemberCountValidator teamMemberCountValidator, ITeamBlockPeriodValidator teamBlockPeriodValidator, ITeamBlockContractTimeValidator teamBlockContractTimeValidator)
		{
			_teamSelectionValidator = teamSelectionValidator;
			_teamMemberCountValidator = teamMemberCountValidator;
			_teamBlockPeriodValidator = teamBlockPeriodValidator;
			_teamBlockContractTimeValidator = teamBlockContractTimeValidator;
		}


		public bool ValidateCanSwap(List<IPerson> selectedPersonList, DateOnlyPeriod selectedPeriod, ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var result = _teamSelectionValidator.ValidateSelection(selectedPersonList, selectedPeriod);
			if (!result) return false;

			result = _teamMemberCountValidator.ValidateMemberCount(teamBlockInfo1, teamBlockInfo2);
			if (!result) return false;

			result = _teamBlockPeriodValidator.ValidatePeriod(teamBlockInfo1, teamBlockInfo2);
			if (!result) return false;

			return _teamBlockContractTimeValidator.ValidateContractTime(teamBlockInfo1, teamBlockInfo2);
		}
	}
}
