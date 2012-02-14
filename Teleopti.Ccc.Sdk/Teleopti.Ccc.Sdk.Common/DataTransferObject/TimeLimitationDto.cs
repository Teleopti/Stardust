using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public struct TimeLimitationDto
    {
        /// <summary>
        /// Gets or sets the flexibility for min time.
        /// </summary>
        /// <remarks>Null value equals full flexibility.</remarks>
        [DataMember]
        public TimeSpan? MinTime { get; set; }

        /// <summary>
        /// Gets or sets the flexibility for max time.
        /// </summary>
        /// <remarks>Null value equals full flexiblity.</remarks>
        [DataMember]
        public TimeSpan? MaxTime { get; set; }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TimeLimitationDto per1, TimeLimitationDto per2)
        {
            return per1.Equals(per2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="per1">The per1.</param>
        /// <param name="per2">The per2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TimeLimitationDto per1, TimeLimitationDto per2)
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
            return MinTime.GetHashCode() - MaxTime.GetHashCode();
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(TimeLimitationDto other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TimeLimitationDto))
            {
                return false;
            }
            return Equals((TimeLimitationDto)obj);
        }
    }
}
