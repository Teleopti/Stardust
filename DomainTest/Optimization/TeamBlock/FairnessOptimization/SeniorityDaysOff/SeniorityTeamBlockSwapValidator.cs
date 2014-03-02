

using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
	}

	public class SeniorityTeamBlockSwapValidator : ISeniorityTeamBlockSwapValidator
	{
		private readonly IDayOffRulesValidator _dayOffRulesValidator;

		public SeniorityTeamBlockSwapValidator(IDayOffRulesValidator dayOffRulesValidator)
		{
			_dayOffRulesValidator = dayOffRulesValidator;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
		{
			foreach (var matrix in teamBlockInfo.MatrixesForGroupAndBlock())
			{
				bool valid = _dayOffRulesValidator.Validate(matrix, optimizationPreferences);
				if (!valid)
					return false;
			}
			return true;
		}
	}
}