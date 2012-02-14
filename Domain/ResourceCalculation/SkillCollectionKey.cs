using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// A key in a dictionary that is a contruction of one or more <see cref="Skill" />
    /// </summary>
    public class SkillCollectionKey : IEquatable<SkillCollectionKey>
    {
        private readonly IList<ISkill> _skillCollection;
        private ISkillStaffPeriod _virtualSkillStaffPeriod;
        private double _sumTraff;
        private double _sumOccWeight;
        private int _hashCode;

        private SkillCollectionKey()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillCollectionKey"/> struct.
        /// </summary>
        /// <param name="skillCollection">The skill collection.</param>
        public SkillCollectionKey(IList<ISkill> skillCollection):this()
        {
            _skillCollection = skillCollection;
            CalculateHash();
        }

        private void CalculateHash()
        {
            int hashCode = 0;
            foreach (ISkill skill in _skillCollection)
            {
                hashCode = hashCode ^ skill.GetHashCode();
            }
            _hashCode = hashCode;
        }

        /// <summary>
        /// Gets or sets the skill collection.
        /// </summary>
        /// <value>The skill collection.</value>
        public IEnumerable<ISkill> SkillCollection
        {
            get { return _skillCollection; }
        }

        /// <summary>
        /// Gets or sets the virtual skill staff period. 
        /// The combined SkillStaffPeriods that corresponds to the skills in SkillCollection.
        /// </summary>
        /// <value>The virtual skill staff period.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-07
        /// </remarks>
        public ISkillStaffPeriod VirtualSkillStaffPeriod
        {
            get { return _virtualSkillStaffPeriod; }
            set { _virtualSkillStaffPeriod = value; }
        }

        /// <summary>
        /// Gets or sets the sum traff.
        /// </summary>
        /// <value>The sum traff.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-07
        /// </remarks>
        public double SumTraff
        {
            get { return _sumTraff; }
            set { _sumTraff = value; }
        }

        /// <summary>
        /// Gets or sets the sum occ weight.
        /// </summary>
        /// <value>The sum occ weight.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-07
        /// </remarks>
        public double SumOccWeight
        {
            get { return _sumOccWeight; }
            set { _sumOccWeight = value; }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-06
        /// </remarks>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (ISkill skill in _skillCollection)
            {
                builder.Append(skill.Name);
            }
            return builder.ToString();
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
        public bool Equals(SkillCollectionKey other)
        {

            return (GetHashCode() == other.GetHashCode());
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
            if (obj == null || !(obj is SkillCollectionKey))
            {
                return false;
            }
            return Equals((SkillCollectionKey)obj);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="key1">The key1.</param>
        /// <param name="key2">The key2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SkillCollectionKey key1, SkillCollectionKey key2)
        {
            return key1.Equals(key2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="key1">The key1.</param>
        /// <param name="key2">The key2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SkillCollectionKey key1, SkillCollectionKey key2)
        {
            return !key1.Equals(key2);
        }

        #endregion
    }
}
