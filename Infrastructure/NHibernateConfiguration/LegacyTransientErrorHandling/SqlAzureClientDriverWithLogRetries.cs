using System;
using log4net;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public class SqlAzureClientDriverWithLogRetries : DefaultReliableSql2008ClientDriver<SqlTransientErrorDetectionStrategyWithTimeouts>
	{
        private static readonly ILog Logger = LogManager.GetLogger(typeof (SqlAzureClientDriverWithLogRetries));

        protected override EventHandler<RetryingEventArgs> RetryEventHandler()
        {
            return logRetry;
        }
        
        private void logRetry(object sender, RetryingEventArgs e)
        {
            Logger.Warn($"Connection lost. CurrentRetryCount: {e.CurrentRetryCount}", e.LastException);
        }
    }
}