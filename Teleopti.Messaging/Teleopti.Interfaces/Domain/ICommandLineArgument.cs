using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-28
    /// </remarks>
    public interface ICommandLineArgument
    {
        /// <summary>
        /// Gets the source server.
        /// </summary>
        /// <value>The source server.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string SourceServer { get; }
        /// <summary>
        /// Gets the source database.
        /// </summary>
        /// <value>The source database.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string SourceDatabase { get; }
        /// <summary>
        /// Gets the name of the source user.
        /// </summary>
        /// <value>The name of the source user.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string SourceUserName { get; }
        /// <summary>
        /// Gets the source password.
        /// </summary>
        /// <value>The source password.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string SourcePassword { get; }
        /// <summary>
        /// Gets the destination server.
        /// </summary>
        /// <value>The destination server.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string DestinationServer { get; }
        /// <summary>
        /// Gets the destination database.
        /// </summary>
        /// <value>The destination database.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string DestinationDatabase { get; }
        /// <summary>
        /// Gets the name of the destination user.
        /// </summary>
        /// <value>The name of the destination user.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string DestinationUserName { get; }
        /// <summary>
        /// Gets the destination password.
        /// </summary>
        /// <value>The destination password.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string DestinationPassword { get; }
        /// <summary>
        /// Gets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        TimeZoneInfo TimeZone { get; }
        /// <summary>
        /// Gets from date.
        /// </summary>
        /// <value>From date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        DateTime FromDate { get; }
        /// <summary>
        /// Gets to date.
        /// </summary>
        /// <value>To date.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        DateTime ToDate { get; }
        /// <summary>
        /// Gets the business unit.
        /// </summary>
        /// <value>The business unit.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string BusinessUnit { get; }
        /// <summary>
        /// Gets the source connection string.
        /// </summary>
        /// <value>The source connection string.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string SourceConnectionString { get; }
        /// <summary>
        /// Gets the destination connection string.
        /// </summary>
        /// <value>The destination connection string.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        string DestinationConnectionString { get; }
        /// <summary>
        /// Gets the culture info.
        /// </summary>
        /// <value>The culture info.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        CultureInfo CultureInfo { get; }
        /// <summary>
        /// Gets the default resolution.
        /// </summary>
        /// <value>The default resolution.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-01-28
        /// </remarks>
        int DefaultResolution { get; }
        /// <summary>
        /// Gets a value indicating whether [only run merge default resolution].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [only run merge default resolution]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Henryg
        /// Created date: 2009-11-16
        /// </remarks>
        bool OnlyRunMergeDefaultResolution { get; }

    	///<summary>
    	/// The user name for the new admin user.
    	///</summary>
    	string NewUserName { get; }

    	///<summary>
    	/// The password for the new admin user.
    	///</summary>
    	string NewUserPassword { get; }
    }
}