using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Used for pairing a 66 unit and employmenttype as one entity
    /// </summary>
    public class UnitEmploymentType :IEquatable<UnitEmploymentType>
    {
        private readonly Unit _unit;
        private readonly EmploymentType _employmentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitEmploymentType"/> class.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="employmentType">Type of the employment.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public UnitEmploymentType(Unit unit, EmploymentType employmentType)
        {
            _unit = unit;
            _employmentType = employmentType;
        }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        /// <value>The unit.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public Unit Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// Gets the type of the employment.
        /// </summary>
        /// <value>The type of the employment.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public EmploymentType EmploymentType
        {
            get { return _employmentType; }
        }

        #region IEquatable<UnitEmploymentType> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(UnitEmploymentType other)
        {
            if(this.Unit.Id == other.Unit.Id && this.EmploymentType.Id == other.EmploymentType.Id)
                return true;

            return false;
        }

        #endregion

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            UnitEmploymentType convObj = obj as UnitEmploymentType;
            if (convObj == null)
            {
                return false;
            }
            else
            {
                return Equals(convObj);
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
            return Unit.GetHashCode() ^ EmploymentType.GetHashCode();
        }

    }
}
