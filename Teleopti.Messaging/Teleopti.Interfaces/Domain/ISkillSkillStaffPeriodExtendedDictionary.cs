using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Dictionary of skill, dictionary of datetimeperiod, ISkillStaffPeriod
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface ISkillSkillStaffPeriodExtendedDictionary
    {
        /// <summary>
        /// Periods this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-10
        /// </remarks>
        DateTimePeriod? Period();

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        void Add(ISkill key, ISkillStaffPeriodDictionary value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        void Clear();

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        bool ContainsKey(ISkill key);

        /// <summary>
        /// Determines whether the specified value contains value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value contains value; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        bool ContainsValue(ISkillStaffPeriodDictionary value);

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        Dictionary<ISkill, ISkillStaffPeriodDictionary>.Enumerator GetEnumerator();

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void GetObjectData(SerializationInfo info, StreamingContext context);

        /// <summary>
        /// Called when [deserialization].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        void OnDeserialization(object sender);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        bool Remove(ISkill key);

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        bool TryGetValue(ISkill key, out ISkillStaffPeriodDictionary value);

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        /// <value>The comparer.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        IEqualityComparer<ISkill> Comparer { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        int Count { get; }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        Dictionary<ISkill, ISkillStaffPeriodDictionary>.KeyCollection Keys { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        Dictionary<ISkill, ISkillStaffPeriodDictionary>.ValueCollection Values { get; }

        /// <summary>
        /// Gets or sets the <see cref="Teleopti.Interfaces.Domain.ISkillStaffPeriodDictionary"/> with the specified key.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-25
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        ISkillStaffPeriodDictionary this[ISkill key] { get; set; }

    }
}
