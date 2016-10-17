using log4net.Core;
using log4net.Filter;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class IgnoreNHibernateErrorsInHangfireWorkers : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Error && loggingEvent.ThreadName.Contains("Worker #"))
			{
				if (loggingEvent.LoggerName.StartsWith("NHibernate"))
					return FilterDecision.Deny;
				if (loggingEvent.LoggerName.Contains(".NHibernateUnitOfWork"))
					return FilterDecision.Deny;
				if (loggingEvent.LoggerName.Contains(".AnalyticsUnitOfWork"))
					return FilterDecision.Deny;
			}

			return FilterDecision.Neutral;
		}
	}
}