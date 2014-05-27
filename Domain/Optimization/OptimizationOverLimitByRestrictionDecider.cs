using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IOptimizationOverLimitByRestrictionDecider
    {
        bool MoveMaxDaysOverLimit();
	    OverLimitResults OverLimitsCounts();
	    bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts);
    }

    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitByRestrictionDecider
    {
	    private readonly IScheduleMatrixPro _matrix;
	    private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixPro matrix,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences,
            IScheduleMatrixOriginalStateContainer originalStateContainer
            )
        {
	        _matrix = matrix;
	        _optimizationPreferences = optimizationPreferences;
            _originalStateContainer = originalStateContainer;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(restrictionChecker);
        }

	    public bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts)
	    {
		    var current = OverLimitsCounts();
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

		public OverLimitResults OverLimitsCounts()
		{
			var overallResults = new OverLimitResults(preferencesOverLimit().Count, mustHavesOverLimit().Count,
				rotationOverLimit().Count, availabilitiesOverLimit().Count, studentAvailabilitiesOverLimit().Count);

		    return overallResults;
	    }

        public bool MoveMaxDaysOverLimit()
        {
            if (_optimizationPreferences.Shifts.KeepShifts && _optimizationPreferences.Shifts.KeepShiftsValue > 1 - _originalStateContainer.ChangedWorkShiftsPercent())
                return true;

			if (_optimizationPreferences.DaysOff.UseKeepExistingDaysOff && _optimizationPreferences.DaysOff.KeepExistingDaysOffValue > 1 - _originalStateContainer.ChangedDayOffsPercent())
                return true;

            return false;
        }

        private IList<DateOnly> preferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(_optimizationPreferences.General.PreferencesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> mustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(_optimizationPreferences.General.MustHavesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> rotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.RotationOverLimit(new Percent(_optimizationPreferences.General.RotationsValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> availabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.AvailabilitiesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> studentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.StudentAvailabilitiesValue), _matrix).BrokenDays;
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
