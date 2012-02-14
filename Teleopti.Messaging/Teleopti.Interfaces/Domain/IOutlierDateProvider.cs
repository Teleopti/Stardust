using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A date provider for outliers (special events)
    /// </summary>
    public interface IOutlierDateProvider : IAggregateEntity, ICloneableEntity<IOutlierDateProvider>
    {
        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-14
        /// </remarks>
        IList<DateOnly> GetDates(DateOnlyPeriod period);

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-15
        /// </remarks>
        string Name { get; set; }
    }
}