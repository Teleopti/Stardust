using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Queue adjustment figures used when calculating statistics from the queue data
    /// </summary>
    public struct QueueAdjustment : IEquatable<QueueAdjustment>
    {
        ///<summary>
        /// The percentage of offered tasks to use
        ///</summary>
        public Percent OfferedTasks { get; set; }

        ///<summary>
        /// The percentage of overflow in tasks to use
        ///</summary>
        public Percent OverflowIn { get; set; }

        ///<summary>
        /// The percentage of overflow out tasks to use
        ///</summary>
        public Percent OverflowOut { get; set; }

        ///<summary>
        /// The percentage of abandoned short tasks to use
        ///</summary>
        public Percent AbandonedShort { get; set; }

        ///<summary>
        /// The percentage of abandoned tasks within service level to use
        ///</summary>
        public Percent AbandonedWithinServiceLevel { get; set; }

        ///<summary>
        /// The percentage of abandoned tasks after service level to use
        ///</summary>
        public Percent AbandonedAfterServiceLevel { get; set; }

        ///<summary>
        /// The percentage of abandoned tasks to use (defaults to -100% to exclude all)
        ///</summary>
        public Percent Abandoned { get; set; }

        #region IEquatable<QueueAdjustment> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(QueueAdjustment other)
        {
            return OfferedTasks==other.OfferedTasks &&
                   OverflowIn == other.OverflowIn &&
                   OverflowOut == other.OverflowOut &&
                   Abandoned == other.Abandoned &&
                   AbandonedShort == other.AbandonedShort &&
                   AbandonedWithinServiceLevel == other.AbandonedWithinServiceLevel &&
                   AbandonedAfterServiceLevel == other.AbandonedAfterServiceLevel;
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
			return obj is QueueAdjustment adjustment && Equals(adjustment);
		}

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return 417 ^ OfferedTasks.GetHashCode() ^ OverflowIn.GetHashCode() ^ OverflowOut.GetHashCode() ^
                Abandoned.GetHashCode() ^ AbandonedShort.GetHashCode() ^ AbandonedWithinServiceLevel.GetHashCode() ^
                   AbandonedAfterServiceLevel.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="queueAdjustment1">The queue adjustment1.</param>
        /// <param name="queueAdjustment2">The queue adjustment2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(QueueAdjustment queueAdjustment1, QueueAdjustment queueAdjustment2)
        {
            return queueAdjustment1.Equals(queueAdjustment2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="queueAdjustment1">The queue adjustment1.</param>
        /// <param name="queueAdjustment2">The queue adjustment2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(QueueAdjustment queueAdjustment1, QueueAdjustment queueAdjustment2)
        {
            return !queueAdjustment1.Equals(queueAdjustment2);
        }

        #endregion
    }
}