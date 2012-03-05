using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IOptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitDecider, IRestrictionsOverLimitDecider{}

    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitByRestrictionDecider
    {
        private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences
            )
        {
            _optimizationPreferences = optimizationPreferences;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(matrixOriginalStateContainer, restrictionChecker);
        }

        public bool OverLimit()
        {
            bool overallResult = PreferencesOverLimit();
            if (overallResult)
                return true;

            overallResult = MustHavesOverLimit();
            if (overallResult)
                return true;

            overallResult = RotationOverLimit();
            if (overallResult)
                return true;

            overallResult = AvailabilitiesOverLimit();
            if (overallResult)
                return true;

            overallResult = StudentAvailabilitiesOverLimit();
            if (overallResult)
                return true;

            return false;
        }

        public bool PreferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return false;
            return _restrictionOverLimitDecider.PreferencesOverLimit(_optimizationPreferences.General.PreferencesValue);
        }

        public bool MustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return false;
            return _restrictionOverLimitDecider.MustHavesOverLimit(_optimizationPreferences.General.MustHavesValue);
        }

        public bool RotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return false;
            return _restrictionOverLimitDecider.RotationOverLimit(_optimizationPreferences.General.RotationsValue);
        }

        public bool AvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return false;
            return _restrictionOverLimitDecider.AvailabilitiesOverLimit(_optimizationPreferences.General.AvailabilitiesValue);
        }

        public bool StudentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return false;
            return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(_optimizationPreferences.General.StudentAvailabilitiesValue);
        }
    }
}
