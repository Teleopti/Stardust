using System;
using System.Data;
using Teleopti.Ccc.Infrastructure.Foundation;
using log4net;
using NHibernate.Connection;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
    /// <summary>
    /// An IDriverConnectionProvider impl for nhibernate.
    /// Will try to grab a dbconn <see cref="NumberOfRetries"/> times before throwing.
    /// Solves some customer problems because of dead connections in pool due to bad networks.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-03-25
    /// </remarks>
    public class TeleoptiDriverConnectionProvider : DriverConnectionProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TeleoptiDriverConnectionProvider));

        public const int NumberOfRetries = 3;

        public override IDbConnection GetConnection()
        {
            var counter = 0;
            do
            {
                counter++;
                try
                {
                    return base.GetConnection();
                }
                catch (Exception e)
                {
                    log.Warn("Failed to get connection (" + counter + "). Retrying...");
                    if (counter >= NumberOfRetries)
                        throw new DataSourceException("Failed to get connection (" + counter + ").", e);
                }
            } while (true);
        }
    }
}