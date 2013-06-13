using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// What databases are used by Raptor?
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-17
    /// </remarks>
    public interface IDataSource : IDisposable
    {
        /// <summary>
        /// Gets the statistic data source.
        /// </summary>
        /// <value>The statistic.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-17
        /// </remarks>
        IUnitOfWorkFactory Statistic { get; }

        /// <summary>
        /// Gets the application data source.
        /// </summary>
        /// <value>The application.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-17
        /// </remarks>
        IUnitOfWorkFactory Application { get; }

        /// <summary>
        /// Gets the authentication settings.
        /// </summary>
        /// <value>The authentication settings.</value>
        IAuthenticationSettings AuthenticationSettings { get; }

    	///<summary>
    	/// Gets the datasource name (application db name)
    	///</summary>
    	string DataSourceName { get; }

    	/// <summary>
        /// Resets the statistic.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-25
        /// </remarks>
        void ResetStatistic();

		/// <summary>
		/// Gets or sets the original file name for this data source.
		/// </summary>
		string OriginalFileName { get; set; }

		/// <summary>
		/// Gets or sets the available authentication type option for this data source.
		/// </summary>
		AuthenticationTypeOption AuthenticationTypeOption { get; set; }
    }
}