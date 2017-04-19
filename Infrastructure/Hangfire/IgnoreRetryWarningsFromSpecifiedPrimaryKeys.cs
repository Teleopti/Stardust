using System.Data.SqlClient;
using Hangfire;
using log4net.Core;
using log4net.Filter;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class IgnoreRetryWarningsFromSpecifiedPrimaryKeys : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level != Level.Warn)
				return FilterDecision.Neutral;
			if (!loggingEvent.LoggerName.Equals(typeof(AutomaticRetryAttribute).FullName))
				return FilterDecision.Neutral;
			var sqlException = loggingEvent.ExceptionObject?.InnerException as SqlException;
			if (sqlException?.Number != 2627) //PK violation
				return FilterDecision.Neutral;

			if (sqlException.Message.Contains("PK_AdherencePercentage"))
				return FilterDecision.Deny;

			return FilterDecision.Neutral;
		}
	}
}