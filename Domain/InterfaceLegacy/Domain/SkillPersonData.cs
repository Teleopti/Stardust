using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// SkillPersonData class
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>
    public struct SkillPersonData : IEquatable<SkillPersonData> 
    {
	    private int _minimumPersons;
        private int _maximumPersons;

	    /// <summary>
	    /// Initializes a new instance of the <see cref="SkillPersonData"/> struct.
	    /// </summary>
	    /// <param name="minimumPersons">The minimum persons.</param>
	    /// <param name="maximumPersons">The maximum persons.</param>
	    /// <remarks>
	    /// Created by: robink
	    /// Created date: 2008-03-17
	    /// </remarks>
	    public SkillPersonData(int minimumPersons, int maximumPersons)
	    {
		    _minimumPersons = minimumPersons;
		    _maximumPersons = maximumPersons;
	    }

		/// <summary>
		/// Gets or sets the minimum persons.
		/// </summary>
		/// <value>The minimum persons.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-03-17
		/// </remarks>
		public int MinimumPersons
        {
            get { return _minimumPersons; }
            set { _minimumPersons = value; }
        }

        /// <summary>
        /// Gets or sets the maximum persons.
        /// </summary>
        /// <value>The maximum persons.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public int MaximumPersons
        {
            get { return _maximumPersons; }
            set { _maximumPersons = value; }
        }

	    /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public bool IsValid => (_minimumPersons <= _maximumPersons || _maximumPersons == 0) &&
							   _minimumPersons >= 0 &&
							   _maximumPersons >= 0;

	    /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public bool Equals(SkillPersonData other)
        {
            return other.MinimumPersons == _minimumPersons &&
                   other.MaximumPersons == _maximumPersons;
        }


        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (!(obj is SkillPersonData))
            {
                return false;
            }
            return Equals((SkillPersonData)obj);
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public override int GetHashCode()
        {
            return _minimumPersons.GetHashCode() ^ _maximumPersons.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="sk1">The SK1.</param>
        /// <param name="sk2">The SK2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public static bool operator ==(SkillPersonData sk1, SkillPersonData sk2)
        {
            return sk1.Equals(sk2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="sk1">The SK1.</param>
        /// <param name="sk2">The SK2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-22
        /// </remarks>
        public static bool operator !=(SkillPersonData sk1, SkillPersonData sk2)
        {
            return !sk1.Equals(sk2);
        }
    }
}