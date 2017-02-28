using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Structure with information about work time directive
    /// </summary>
    public struct WorkTimeDirective : IEquatable<WorkTimeDirective>
    {
        #region Fields

	    private TimeSpan _minTimePerWeek;
	    private TimeSpan _maxTimePerWeek;
        private TimeSpan _nightlyRest;
        private TimeSpan _weeklyRest;
        private const int _limitWeeklyRest = 24 * 7;
        private const int _limitNightlyRest = 24;
        private const int _limitMaxTimePerWeek = 24 * 7;

        #endregion

        #region Static

        /// <summary>
        /// Gets the default WorkTimeDirective value.
        /// </summary>
        /// <value>The default work time directive.</value>
        public static WorkTimeDirective DefaultWorkTimeDirective
        {
			get { return new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(48, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(36, 0, 0)); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the maximal work time per week.
        /// </summary>
        public TimeSpan MaxTimePerWeek
        {
            get { return _maxTimePerWeek; }
        }

        /// <summary>
        /// Gets the minimum work time per week.
        /// </summary>
        public TimeSpan MinTimePerWeek
        {
            get { return _minTimePerWeek; }
        }

        /// <summary>
        /// Gets the minimal nightly rest
        /// </summary>
        public TimeSpan NightlyRest
        {
            get { return _nightlyRest; }
        }

        /// <summary>
        /// Gets the minimal weekly rest
        /// </summary>
        public TimeSpan WeeklyRest
        {
            get { return _weeklyRest; }
        }

        #endregion

	    /// <summary>
	    /// Creates a struct with values
	    /// </summary>
	    /// <param name="minTimePerWeek"></param>
	    /// <param name="maxTimePerWeek">Maximal work time per week.</param>
	    /// <param name="nightlyRest">Minimal nightly rest</param>
	    /// <param name="weeklyRest">Minimal weekly rest</param>
	    public WorkTimeDirective(TimeSpan minTimePerWeek, TimeSpan maxTimePerWeek, TimeSpan nightlyRest, TimeSpan weeklyRest)
        {
            InParameter.CheckTimeLimit("maxTimePerWeek", maxTimePerWeek, _limitMaxTimePerWeek);
            InParameter.CheckTimeLimit("minTimePerWeek", minTimePerWeek, _limitMaxTimePerWeek);
            InParameter.CheckTimeLimit("nightlyRest", nightlyRest, _limitNightlyRest);
            InParameter.CheckTimeLimit("weeklyRest", weeklyRest, _limitWeeklyRest);

	        _minTimePerWeek = minTimePerWeek;
            _maxTimePerWeek = maxTimePerWeek;
            _nightlyRest = nightlyRest;
            _weeklyRest = weeklyRest;
        }

        #region IEquatable<WorkTimeDirective> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(WorkTimeDirective other)
        {
            return other._maxTimePerWeek == _maxTimePerWeek &&
					other._minTimePerWeek == _minTimePerWeek &&
                   other._nightlyRest == _nightlyRest &&
                   other._weeklyRest == _weeklyRest;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is WorkTimeDirective))
            {
                return false;
            }
            else
            {
                return Equals((WorkTimeDirective)obj);
            }
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="wt1">The work time directive 1.</param>
        /// <param name="wt2">The work time directive 2.</param>
        /// <returns></returns>
        public static bool operator ==(WorkTimeDirective wt1, WorkTimeDirective wt2)
        {
            return wt1.Equals(wt2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="wt1">The work time directive 1.</param>
        /// <param name="wt2">The work time directive 2.</param>
        /// <returns></returns>
        public static bool operator !=(WorkTimeDirective wt1, WorkTimeDirective wt2)
        {
            return !wt1.Equals(wt2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
			return _minTimePerWeek.GetHashCode() ^ _maxTimePerWeek.GetHashCode() ^ _nightlyRest.GetHashCode() ^ _weeklyRest.GetHashCode();
        }

        #endregion
    }
}