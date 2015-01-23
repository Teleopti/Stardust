using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockOptimizationLimits
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
		bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences);
		bool ValidateMinWorkTimePerWeek(ITeamBlockInfo teamBlockInfo);
		bool ValidateMinWorkTimePerWeek(ITeamInfo teamInfo);
	}

	public class TeamBlockOptimizationLimits : ITeamBlockOptimizationLimits
	{
		private readonly ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;

		public TeamBlockOptimizationLimits(ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator, IMinWeekWorkTimeRule minWeekWorkTimeRule)
		{
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
		{
			return _teamBlockRestrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences);
		}

		public bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences)
		{
			return _teamBlockRestrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences);
		}

		public bool ValidateMinWorkTimePerWeek(ITeamBlockInfo teamBlockInfo)
		{
			return validateMatrixesMinWeekWorkTime(teamBlockInfo.MatrixesForGroupAndBlock());
		}

		public bool ValidateMinWorkTimePerWeek(ITeamInfo teamInfo)
		{
			return validateMatrixesMinWeekWorkTime(teamInfo.MatrixesForGroup());
		}

		private bool validateMatrixesMinWeekWorkTime(IEnumerable<IScheduleMatrixPro> matrixes)
		{
			foreach (var matrix in matrixes)
			{
				var dictionary = new Dictionary<IPerson, IScheduleRange> { { matrix.Person, matrix.ActiveScheduleRange } };

				foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
				{
					var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
					 if(!_minWeekWorkTimeRule.Validate(dictionary, scheduleDays).IsEmpty()) return false;
				}	
			}

			return true;
		}
	}
}
