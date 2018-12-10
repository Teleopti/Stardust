using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class RestrictionOverLimitValidator
	{
		private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;

		public RestrictionOverLimitValidator(IRestrictionOverLimitDecider restrictionOverLimitDecider)
		{
			_restrictionOverLimitDecider = restrictionOverLimitDecider;
		}
		
		public bool Validate(IEnumerable<IScheduleMatrixPro> matrixes, IOptimizationPreferences optimizationPreferences)
		{
			foreach (var matrix in matrixes)
			{
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