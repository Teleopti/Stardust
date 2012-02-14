namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// ACD Login from Matrix
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-08-15
    /// </remarks>
    public interface IExternalLogOn : IAggregateRoot
    {
        /// <summary>
        /// Gets the log on id.
        /// </summary>
        /// <value>The log on id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        int AcdLogOnMartId { get; set; }

        /// <summary>
        /// Gets the log on agg id.
        /// </summary>
        /// <value>The log on agg id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        int AcdLogOnAggId { get; set; }

        /// <summary>
        /// Gets the log on code.
        /// </summary>
        /// <value>The log on code.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        string AcdLogOnOriginalId { get; set; }

        /// <summary>
        /// Gets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        string AcdLogOnName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IExternalLogOn"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        bool Active { get; set; }

        /// <summary>
        /// Gets the data source id.
        /// </summary>
        /// <value>The data source id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        int DataSourceId { get; set; }
    }
}