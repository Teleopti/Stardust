using System;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    /// <summary>
    /// Stores a date period in local time for the Multiple Date handling.
    /// </summary>
    /// <remarks>
    /// Created by: jonas n
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IJobMultipleDateItem
    {
        /// <summary>
        /// Gets or sets the end date local.
        /// </summary>
        /// <value>The end date local.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-13
        /// </remarks>
        DateTime EndDateLocal
        {
            get;
        }

        /// <summary>
        /// Gets or sets the start date local.
        /// </summary>
        /// <value>The start date local.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-13
        /// </remarks>
        DateTime StartDateLocal
        {
            get;
        }

        /// <summary>
        /// Gets the start date in UTC.
        /// </summary>
        /// <value>The start date in UTC.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-16
        /// </remarks>
        DateTime StartDateUtc
        {
            get;
        }

        /// <summary>
        /// Gets the end date in UTC.
        /// </summary>
        /// <value>The end date in UTC.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-16
        /// </remarks>
        DateTime EndDateUtc
        {
            get;
        }

        /// <summary>
        /// Gets the start date UTC without timepart.
        /// </summary>
        /// <value>The start date UTC floor.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-16
        /// </remarks>
        DateTime StartDateUtcFloor { get; }

        /// <summary>
        /// Gets the end date UTC ceiling. Including time part 23:59:59.999.
        /// </summary>
        /// <value>The end date UTC ceiling.</value>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-16
        /// </remarks>
        DateTime EndDateUtcCeiling { get; }
    }
}
