using System;
using System.Xml.Serialization;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds two values, one representing low value another representing high value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-01
    /// </remarks>
    [Serializable]
    [XmlRoot(ElementName = "MinMax", IsNullable = false)]
    public struct MinMax<T> where T : struct, IComparable<T>
    {
        private readonly T _minimum;
        private readonly T _maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMax&lt;T&gt;"/> struct.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public MinMax(T minimum, T maximum)
        {
            if (minimum.CompareTo(maximum) > 0)
            {
                var message =
                    $"[{nameof(minimum)}] value \"{minimum}\" can not be higher than [{nameof(maximum)}] value \"{maximum}\"";
                throw new ArgumentOutOfRangeException(nameof(minimum), message);
            }

            _minimum = minimum;
            _maximum = maximum;
        }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>The minimum.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public T Minimum => _minimum;

		/// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>The maximum.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public T Maximum => _maximum;

		#region Equals and GetHashCode stuff


        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator ==(MinMax<T> obj1, MinMax<T> obj2)
        {
            return obj1.Equals(obj2);
        }


        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator !=(MinMax<T> obj1, MinMax<T> obj2)
        {
            return !obj1.Equals(obj2);
        }


        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public override bool Equals(object obj)
        {
			if (obj is MinMax<T> max) return Equals(max);
			return false;
		}

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(MinMax<T> other)
        {
            return _minimum.Equals(other.Minimum) && _maximum.Equals(other.Maximum);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public override int GetHashCode()
        {
            return (_minimum.GetHashCode() * 397) ^ _maximum.GetHashCode();
        }

        #endregion

        #region methods

        /// <summary>
        /// Returns true if parameter is within min max
        /// </summary>
        /// <param name="other">Value to compare.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-05-20
        /// </remarks>
        public bool Contains(T other)
        {
            if (_minimum.CompareTo(other) > 0 || other.CompareTo(_maximum) > 0) return false;
            return true;
        }

        #endregion

		/// <summary>
		/// The to string string
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// </remarks>
		public override string ToString()
		{
			return $"{Minimum}-{Maximum}";
		}
    }
}