using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Holds a time period and a segment used
    /// to "slide" the period
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-05
    /// </remarks>
    public struct TimePeriodWithSegment
    {
        private TimePeriod _period;
        private TimeSpan _segment;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriodWithSegment"/> struct.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="segment">The segment.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public TimePeriodWithSegment(TimePeriod period, TimeSpan segment)
        {
            InParameter.CheckTimeSpanAtLeastOneTick(nameof(segment),segment);
            InParameter.TimeSpanCannotBeNegative(nameof(period.StartTime), period.StartTime);
            
            _period = period;
            _segment = segment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriodWithSegment"/> struct.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="startMinutes">The start minutes.</param>
        /// <param name="endHour">The end hour.</param>
        /// <param name="endMinutes">The end minutes.</param>
        /// <param name="segmentMinutes">The segment minutes.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public TimePeriodWithSegment(int startHour, 
                                     int startMinutes,
                                     int endHour,
                                     int endMinutes,
                                     int segmentMinutes) 
            : this(
                new TimePeriod(startHour, startMinutes, endHour, endMinutes), 
                new TimeSpan(0,segmentMinutes,0)
                )
        {
        }
		
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public TimePeriod Period => _period;

	    /// <summary>
        /// Gets the segment.
        /// </summary>
        /// <value>The segment.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-05
        /// </remarks>
        public TimeSpan Segment => _segment;

	    #region IEquatable<TimePeriod> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(TimePeriodWithSegment other)
        {
            return _period == other.Period && _segment == other.Segment;
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
			return obj is TimePeriodWithSegment segment && Equals(segment);
		}

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(TimePeriodWithSegment per1, TimePeriodWithSegment per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(TimePeriodWithSegment per1, TimePeriodWithSegment per2)
        {
            return !per1.Equals(per2);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return _period.GetHashCode() ^ _segment.GetHashCode();
        }
        #endregion
    }
}