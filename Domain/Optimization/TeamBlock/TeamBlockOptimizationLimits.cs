﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockOptimizationLimits
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
		bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
		bool ValidateMinWorkTimePerWeek(ITeamBlockInfo teamBlockInfo);
		bool ValidateMinWorkTimePerWeek(ITeamInfo teamInfo);
	}

	public class TeamBlockOptimizationLimits : ITeamBlockOptimizationLimits
	{
		private readonly TeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;

		public TeamBlockOptimizationLimits(TeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			return _teamBlockRestrictionOverLimitValidator.Validate(teamBlockInfo, optimizationPreferences, dayOffOptimizationPreferenceProvider);
		}

		public bool Validate(ITeamInfo teamInfo, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			return _teamBlockRestrictionOverLimitValidator.Validate(teamInfo, optimizationPreferences, dayOffOptimizationPreferenceProvider);
		}

		public bool ValidateMinWorkTimePerWeek(ITeamBlockInfo teamBlockInfo)
		{
			return validateMatrixesMinWeekWorkTime(teamBlockInfo.MatrixesForGroupAndBlock());
		}

		public bool ValidateMinWorkTimePerWeek(ITeamInfo teamInfo)
		{
			return validateMatrixesMinWeekWorkTime(teamInfo.MatrixesForGroup());
		}

		private static bool validateMatrixesMinWeekWorkTime(IEnumerable<IScheduleMatrixPro> matrixes)
		{
			var minWeekWorkTimeRule = new MinWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor());
			foreach (var matrix in matrixes)
			{
				var dictionary = new Dictionary<IPerson, IScheduleRange> { { matrix.Person, matrix.ActiveScheduleRange } };

				foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
				{
					var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
					 if(!minWeekWorkTimeRule.Validate(dictionary, scheduleDays).IsEmpty()) return false;
				}	
			}

			return true;
		}
	}
}
