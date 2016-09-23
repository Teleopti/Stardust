using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRestrictionOverLimitValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
		bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	public class TeamBlockRestrictionOverLimitValidator : ITeamBlockRestrictionOverLimitValidator
	{
		private readonly IMaxMovedDaysOverLimitValidator _maxMovedDaysOverLimitValidator;
		private readonly RestrictionOverLimitValidator _restrictionOverLimitValidator;

		public TeamBlockRestrictionOverLimitValidator(IMaxMovedDaysOverLimitValidator maxMovedDaysOverLimitValidator,
			RestrictionOverLimitValidator restrictionOverLimitValidator)
		{
			_maxMovedDaysOverLimitValidator = maxMovedDaysOverLimitValidator;
			_restrictionOverLimitValidator = restrictionOverLimitValidator;
		}

		public bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			return validateMatrixes(optimizationPreferences, dayOffOptimizationPreferenceProvider, teamInfo.MatrixesForGroup());
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			return validateMatrixes(optimizationPreferences, dayOffOptimizationPreferenceProvider, teamBlockInfo.MatrixesForGroupAndBlock());
		}

		private bool validateMatrixes(IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, IEnumerable<IScheduleMatrixPro> matrixes)
		{
			foreach (var matrix in matrixes)
			{
				var dayOffOptimizePreference = dayOffOptimizationPreferenceProvider.ForAgent(matrix.Person,
					matrix.EffectivePeriodDays.First().Day);

				if (!_maxMovedDaysOverLimitValidator.ValidateMatrix(matrix, optimizationPreferences, dayOffOptimizePreference))
				{
					return false;
				}
			}
			return _restrictionOverLimitValidator.Validate(matrixes, optimizationPreferences);
		}
	}
}