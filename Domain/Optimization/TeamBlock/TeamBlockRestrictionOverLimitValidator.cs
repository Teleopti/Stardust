﻿using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRestrictionOverLimitValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
	}

	public class TeamBlockRestrictionOverLimitValidator : ITeamBlockRestrictionOverLimitValidator
	{
		private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private readonly IMaxMovedDaysOverLimitValidator _maxMovedDaysOverLimitValidator;

		public TeamBlockRestrictionOverLimitValidator(IRestrictionOverLimitDecider restrictionOverLimitDecider, 
			IMaxMovedDaysOverLimitValidator maxMovedDaysOverLimitValidator)
		{
			_restrictionOverLimitDecider = restrictionOverLimitDecider;
			_maxMovedDaysOverLimitValidator = maxMovedDaysOverLimitValidator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
		{
			foreach (var matrix in teamBlockInfo.MatrixesForGroupAndBlock())
			{
				if (!_maxMovedDaysOverLimitValidator.ValidateMatrix(matrix, optimizationPreferences))
					return false;

				if (preferencesOverLimit(matrix, optimizationPreferences))
					return false;

				if (mustHavesOverLimit(matrix, optimizationPreferences))
					return false;

				if (rotationOverLimit(matrix, optimizationPreferences))
					return false;

				if (availabilitiesOverLimit(matrix, optimizationPreferences))
					return false;

				if (studentAvailabilitiesOverLimit(matrix, optimizationPreferences))
					return false;
			}

			return true;
		}

		// class
		

		private bool preferencesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UsePreferences)
				return false;
			return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(optimizationPreferences.General.PreferencesValue), matrix).BrokenDays.Any();
		}

		private bool mustHavesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseMustHaves)
				return false;
			return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(optimizationPreferences.General.MustHavesValue), matrix).BrokenDays.Any();
		}

		private bool rotationOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseRotations)
				return false;
			return _restrictionOverLimitDecider.RotationOverLimit(new Percent(optimizationPreferences.General.RotationsValue), matrix).BrokenDays.Any();
		}

		private bool availabilitiesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseAvailabilities)
				return false;
			return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(optimizationPreferences.General.AvailabilitiesValue), matrix).BrokenDays.Any();
		}

		private bool studentAvailabilitiesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseStudentAvailabilities)
				return false;
			return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(optimizationPreferences.General.StudentAvailabilitiesValue), matrix).BrokenDays.Any();
		}
	}
}