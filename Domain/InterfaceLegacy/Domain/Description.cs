using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Description struct
    /// </summary>
    [Serializable]
    public struct Description
    {
        private string _name;
        private string _shortName;
        private const int _nameLength = 50;
        private const int _shortNameLength = 25;

        /// <summary>
        /// Initializes a new instance of the <see cref="Description"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shortName">The short name.</param>
        public Description(string name, string shortName)
        {
            ValidateName(name, shortName);

            _name = name;
            _shortName = shortName;
        }



		/// <summary>
		/// Initializes a new instance of the <see cref="Description"/> struct.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-26
		/// </remarks>
		public Description(string name) : this(name,null)
        {
        }

        /// <summary>
        /// Validates the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shortName">The short name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        private static void ValidateName(string name, string shortName)
        {
            InParameter.NotStringEmptyOrWhiteSpace(nameof(name), name);

            if (name.Length > _nameLength)
                throw new ArgumentOutOfRangeException(nameof(name), "String too long.");
            if (shortName!=null && shortName.Length > _shortNameLength)
                throw new ArgumentOutOfRangeException(nameof(shortName), "String too long.");
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                if (_name == null)
                    _name = string.Empty;
                return _name.Trim();
            }
        }

        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name.</value>
        public string ShortName
        {
            get
            {
                if (_shortName == null)
                    _shortName = string.Empty;
                return _shortName.Trim();
            }
        }

        /// <summary>
        /// Gets the max length of the short name.
        /// </summary>
        /// <value>The max length of the short name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-12
        /// </remarks>
        public static int MaxLengthOfShortName => _shortNameLength;

	    /// <summary>
        /// Gets the max length of the name.
        /// </summary>
        /// <value>The max length of the name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-12
        /// </remarks>
        public static int MaxLengthOfName => _nameLength;

	    /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return ShortName;
            if (string.IsNullOrEmpty(ShortName))
                return Name;
            return string.Concat(ShortName, ", ", Name);
        }

        #region IEquatable<Description> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(Description other)
        {
            return (Name == other.Name && ShortName == other.ShortName);
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
			return obj is Description description && Equals(description);
		}

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ ShortName.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="description1">The description1.</param>
        /// <param name="description2">The description2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Description description1, Description description2)
        {
            return description1.Equals(description2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="description1">The description1.</param>
        /// <param name="description2">The description2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Description description1, Description description2)
        {
            return !description1.Equals(description2);
        }

        #endregion
    }
}