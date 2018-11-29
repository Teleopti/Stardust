using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Structure with information about working time
    /// </summary>
    public struct WorkTime : IEquatable<WorkTime>
    {
        #region Fields

        private TimeSpan _avgWorkTimePerDay;
        private const int _limitAvgWorkTimePerDay = 24;

        #endregion

        #region Static

        /// <summary>
        /// Gets the default WorkTime value.
        /// </summary>
        public static WorkTime DefaultWorkTime
        {
            get
            {
                return new WorkTime(new TimeSpan(8, 0, 0));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the average work time per day.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Avg")]
		public TimeSpan AvgWorkTimePerDay
        {
            get { return _avgWorkTimePerDay; }
        }

        #endregion

        /// <summary>
        /// Creates a new work time info struct
        /// </summary>
        /// <param name="averageWorkTimePerDay">Average work time per day</param>
        public WorkTime(TimeSpan averageWorkTimePerDay)
        {
            InParameter.CheckTimeLimit("averageWorkTimePerDay", averageWorkTimePerDay, _limitAvgWorkTimePerDay);

            _avgWorkTimePerDay = averageWorkTimePerDay;
        }

        #region IEquatable<WorkTime> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(WorkTime other)
        {
            return other._avgWorkTimePerDay == _avgWorkTimePerDay;
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
			return obj is WorkTime time && Equals(time);
		}

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="wt1">The work time 1.</param>
        /// <param name="wt2">The work time 2.</param>
        /// <returns></returns>
        public static bool operator ==(WorkTime wt1, WorkTime wt2)
        {
            return wt1.Equals(wt2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="wt1">The work time 1.</param>
        /// <param name="wt2">The work time 2.</param>
        /// <returns></returns>
        public static bool operator !=(WorkTime wt1, WorkTime wt2)
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
            return _avgWorkTimePerDay.GetHashCode();
        }

        #endregion
    }
}