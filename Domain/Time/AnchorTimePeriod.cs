using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// AnchorTimePeriod struct
    /// </summary>
    public struct AnchorTimePeriod : IEquatable<AnchorTimePeriod>
    {
        #region Fields

        private TimeSpan _anchor;
        private TimeSpan _targetLength;
        private Percent _flexibility;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the flexibility.
        /// </summary>
        public Percent Flexibility
        {
            get { return _flexibility; }
        }

        /// <summary>
        /// Gets the length (duration).
        /// </summary>
        public TimeSpan TargetLength
        {
            get { return _targetLength; }
        }

        /// <summary>
        /// Gets the anchor point relative to the start of the parent interval.
        /// </summary>
        public TimeSpan Anchor
        {
            get { return _anchor; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorTimePeriod"/> struct.
        /// </summary>
        /// <param name="anchor">Centered position of time..</param>
        /// <param name="targetLength">Length of anchored time.</param>
        /// <param name="flexibility">Flexibility (percentage of time block length)</param>
        public AnchorTimePeriod(TimeSpan anchor, TimeSpan targetLength, Percent flexibility)
        {
            //Check flexibility parameter
            InParameter.BetweenOneAndHundredPercent("flexibility", flexibility);

            _anchor = anchor;
            _targetLength = targetLength;
            _flexibility = flexibility;
        }

        #region IEquatable<TimePeriod> Implementation

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(AnchorTimePeriod other)
        {
            return other._anchor == _anchor &&
                   other._targetLength == _targetLength &&
                   other._flexibility == _flexibility;
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
            if (obj == null || !(obj is AnchorTimePeriod))
            {
                return false;
            }
            else
            {
                return Equals((AnchorTimePeriod) obj);
            }
        }

        /// <summary>
        /// Operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator ==(AnchorTimePeriod per1, AnchorTimePeriod per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns></returns>
        public static bool operator !=(AnchorTimePeriod per1, AnchorTimePeriod per2)
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
            return _anchor.GetHashCode() ^ _targetLength.GetHashCode() ^ _flexibility.GetHashCode();
        }

        #endregion
    }
}