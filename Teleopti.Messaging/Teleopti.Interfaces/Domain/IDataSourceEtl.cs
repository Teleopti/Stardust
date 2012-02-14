namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Handles datasources (log objects) in ETL Tool
    /// </summary>
    /// /// <remarks>
    /// Created by: jonas n
    /// Created date: 2009-12-04
    /// </remarks>
    public interface IDataSourceEtl
    {
        /// <summary>
        /// Gets the data source data mart id.
        /// </summary>
        /// <value>The data source id.</value>
        int DataSourceId { get; }

        /// <summary>
        /// Gets the name of the data source.
        /// </summary>
        /// <value>The name of the data source.</value>
        string DataSourceName { get; }

        /// <summary>
        /// Gets or sets the time zone data mart id.
        /// </summary>
        /// <value>The time zone id.</value>
        int TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the time zone code. Equals to 'TimeZoneInfo.Id'.
        /// </summary>
        /// <value>The time zone code.</value>
        string TimeZoneCode { get; set; }

        /// <summary>
        /// Gets the length of the interval used by this log object.
        /// </summary>
        /// <value>The length of the interval.</value>
        int IntervalLength { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IDataSourceEtl"/> is inactive. 
        /// Inactive means that the data source is not in use by the ETL Tool.
        /// </summary>
        /// <value><c>true</c> if inactive; otherwise, <c>false</c>.</value>
        bool Inactive { get; set; }
    }
}