using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IOptimizationOverLimitByRestrictionDecider
    {
        bool MoveMaxDaysOverLimit();
	    OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix);
		bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix);
    }

    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitByRestrictionDecider
    {
	    private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
	    private readonly IDaysOffPreferences _daysOffPreferences;
	    private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
			IDaysOffPreferences daysOffPreferences
            )
        {
	        _optimizationPreferences = optimizationPreferences;
            _originalStateContainer = originalStateContainer;
	        _daysOffPreferences = daysOffPreferences;
	        _restrictionOverLimitDecider = new RestrictionOverLimitDecider(restrictionChecker);
        }

	 
	    public bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix)
	    {
		    var current = OverLimitsCounts(matrix);
			if (current.PreferencesOverLimit > lastOverLimitCounts.PreferencesOverLimit)
			    return true;

			if (current.MustHavesOverLimit > lastOverLimitCounts.MustHavesOverLimit)
				return true;

			if (current.RotationsOverLimit > lastOverLimitCounts.RotationsOverLimit)
				return true;

			if (current.AvailabilitiesOverLimit > lastOverLimitCounts.AvailabilitiesOverLimit)
				return true;

			if (current.StudentAvailabilitiesOverLimit > lastOverLimitCounts.StudentAvailabilitiesOverLimit)
				return true;

			return false;
	    }

	    public OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix)
		{
			var overallResults = new OverLimitResults(preferencesOverLimit(matrix).Count, mustHavesOverLimit(matrix).Count,
				rotationOverLimit(matrix).Count, availabilitiesOverLimit(matrix).Count, studentAvailabilitiesOverLimit(matrix).Count);

		    return overallResults;
	    }

        public bool MoveMaxDaysOverLimit()
        {
			if (_daysOffPreferences.UseKeepExistingDaysOff && _daysOffPreferences.KeepExistingDaysOffValue > 1 - _originalStateContainer.ChangedDayOffsPercent())
                return true;

            return false;
        }

        private IList<DateOnly> preferencesOverLimit(IScheduleMatrixPro matrix)
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(_optimizationPreferences.General.PreferencesValue), matrix).BrokenDays;
        }

        private IList<DateOnly> mustHavesOverLimit(IScheduleMatrixPro matrix)
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(_optimizationPreferences.General.MustHavesValue), matrix).BrokenDays;
        }

        private IList<DateOnly> rotationOverLimit(IScheduleMatrixPro matrix)
        {
            if (!_optimizationPreferences.General.UseRotations)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.RotationOverLimit(new Percent(_optimizationPreferences.General.RotationsValue), matrix).BrokenDays;
        }

        private IList<DateOnly> availabilitiesOverLimit(IScheduleMatrixPro matrix)
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.AvailabilitiesValue), matrix).BrokenDays;
        }

        private IList<DateOnly> studentAvailabilitiesOverLimit(IScheduleMatrixPro matrix)
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.StudentAvailabilitiesValue), matrix).BrokenDays;
        }
    }

	public class OverLimitResults
	{
		private readonly int _preferencesOverLimit;
		private readonly int _rotationsOverLimit;
		private readonly int _availabilitiesOverLimit;
		private readonly int _studentAvailabilitiesOverLimit;
		private readonly int _mustHavesOverLimit;

		public OverLimitResults(int preferencesOverLimit, int mustHavesOverLimit, int rotationsOverLimit, int availabilitiesOverLimit,
			int studentAvailabilitiesOverLimit)
		{
			_preferencesOverLimit = preferencesOverLimit;
			_rotationsOverLimit = rotationsOverLimit;
			_availabilitiesOverLimit = availabilitiesOverLimit;
			_studentAvailabilitiesOverLimit = studentAvailabilitiesOverLimit;
			_mustHavesOverLimit = mustHavesOverLimit;
		}

		public int PreferencesOverLimit
		{
			get { return _preferencesOverLimit; }
		}

		public int RotationsOverLimit
		{
			get { return _rotationsOverLimit; }
		}

		public int AvailabilitiesOverLimit
		{
			get { return _availabilitiesOverLimit; }
		}

		public int StudentAvailabilitiesOverLimit
		{
			get { return _studentAvailabilitiesOverLimit; }
		}

		public int MustHavesOverLimit
		{
			get { return _mustHavesOverLimit; }
		}
	}
}
