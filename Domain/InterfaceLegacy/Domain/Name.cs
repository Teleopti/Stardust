
using System.Globalization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Name struct
    /// </summary>
    public struct Name
    {
        private string _firstName;
        private string _lastName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Name"/> class.
        /// </summary>
        /// <param name="firstName">Name of the first.</param>
        /// <param name="lastName">Name of the last.</param>
        public Name(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
        }

        /// <summary>
        /// Gets the firstname.
        /// </summary>
        /// <value>The firstname.</value>
        public string FirstName
        {
            get
            {
                if (_firstName == null)
                    _firstName = string.Empty;
                return _firstName;
            }
        }

        /// <summary>
        /// Gets the lastname
        /// </summary>
        /// <value>The lastname.</value>
        public string LastName
        {
            get
            {
                if (_lastName == null)
                    _lastName = string.Empty;
                return _lastName;
            }
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return ToString(NameOrderOption.FirstNameLastName);
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> containing a fully qualified type name.
        /// </returns>
        public string ToString(NameOrderOption nameOrder)
        {
            if (string.IsNullOrEmpty(FirstName))
                return LastName;
            if (string.IsNullOrEmpty(LastName))
                return FirstName;
            if(nameOrder == NameOrderOption.FirstNameLastName)
                return string.Concat(FirstName, " ", LastName);
            else
                return string.Concat(LastName, ", ", FirstName);
        }


	    #region IEquatable<Name> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(Name other)
        {
            return (FirstName == other.FirstName && LastName == other.LastName);
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
            if (obj == null || !(obj is Name))
            {
                return false;
            }
            else
            {
                return Equals((Name) obj);
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return FirstName.GetHashCode() ^ LastName.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="name1">The name1.</param>
        /// <param name="name2">The name2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Name name1, Name name2)
        {
            return name1.Equals(name2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="name1">The name1.</param>
        /// <param name="name2">The name2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Name name1, Name name2)
        {
            return !name1.Equals(name2);
        }

        #endregion
    }
}