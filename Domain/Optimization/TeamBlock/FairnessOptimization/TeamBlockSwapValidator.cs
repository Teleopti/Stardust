using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockSwapValidator
	{
		bool ValidateCanSwap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamBlockSwapValidator : ITeamBlockSwapValidator
	{
		private readonly IList<IPerson> _selectedPersons;
		private readonly DateOnlyPeriod _dateOnlyPeriod;
		private readonly ITeamSelectionValidator _teamSelectionValidator;
		private readonly ITeamMemberCountValidator _teamMemberCountValidator;
		private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private readonly ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		
		public TeamBlockSwapValidator(IList<IPerson> selectedPersons, DateOnlyPeriod dateOnlyPeriod, ITeamSelectionValidator teamSelectionValidator, ITeamMemberCountValidator teamMemberCountValidator, ITeamBlockPeriodValidator teamBlockPeriodValidator, ITeamBlockContractTimeValidator teamBlockContractTimeValidator)
		{
			_selectedPersons = selectedPersons;
			_dateOnlyPeriod = dateOnlyPeriod;
			_teamSelectionValidator = teamSelectionValidator;
			_teamMemberCountValidator = teamMemberCountValidator;
			_teamBlockPeriodValidator = teamBlockPeriodValidator;
			_teamBlockContractTimeValidator = teamBlockContractTimeValidator;
		}


		public bool ValidateCanSwap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var result = _teamSelectionValidator.ValidateSelection(_selectedPersons, _dateOnlyPeriod);
			if (!result) return false;

			result = _teamMemberCountValidator.ValidateMemberCount(teamBlockInfo1, teamBlockInfo2);
			if (!result) return false;

			result = _teamBlockPeriodValidator.ValidatePeriod(teamBlockInfo1, teamBlockInfo2);
			if (!result) return false;

			return _teamBlockContractTimeValidator.ValidateContractTime(teamBlockInfo1, teamBlockInfo2);
		}
	}
}
