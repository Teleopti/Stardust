using System;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using NHibernate.SqlAzure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
    public class SqlAzureClientDriverWithLogRetries : DefaultReliableSql2008ClientDriver<SqlTransientErrorDetectionStrategyWithTimeouts>
	{
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SqlAzureClientDriverWithLogRetries));
        protected override EventHandler<RetryingEventArgs> RetryEventHandler()
        {
            return logRetry;
        }
        
        private void logRetry(object sender, RetryingEventArgs e)
        {
            Logger.Warn(string.Format("Connection lost. CurrentRetryCount: {0}", e.CurrentRetryCount), e.LastException);
        }
    }
}