using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// MultisitePeriod
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-01
    /// </remarks>
    public interface IMultisitePeriod : IMultisiteData, IAggregateEntity, ICloneableEntity<IMultisitePeriod>
    {
    }

    /// <summary>
    /// Share data for multisite information.
    /// </summary>
    public interface IMultisiteData : IPeriodized
    {
        /// <summary>
        /// Gets the distribution.
        /// </summary>
        /// <value>The distribution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        IDictionary<IChildSkill, Percent> Distribution { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        bool IsValid { get; }

        /// <summary>
        /// Sets the percentage.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <param name="percentage">The percentage.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        void SetPercentage(IChildSkill childSkill, Percent percentage);   
    }
}