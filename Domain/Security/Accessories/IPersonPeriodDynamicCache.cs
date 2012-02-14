using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{
    /// <summary>
    /// Caches the temporary PersonPeriod list for a person and a period for peformance reasons.
    /// </summary>
    public interface IPersonPeriodDynamicCache
    {
        /// <summary>
        /// Gets the persons periods for the specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        IList<IPersonPeriod> PersonPeriods(IPerson person, DateTimePeriod period);

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PersonPeriodDynamicCache"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled
        { get; set; }

        /// <summary>
        /// Deletes the cached data.
        /// </summary>
        void DeleteCache();
    }
}