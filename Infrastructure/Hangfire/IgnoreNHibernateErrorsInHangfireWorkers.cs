using log4net.Core;
using log4net.Filter;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class IgnoreNHibernateErrorsInHangfireWorkers : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Error &&
				loggingEvent.LoggerName.StartsWith("NHibernate") &&
				loggingEvent.ThreadName.Contains("Worker #"))
				return FilterDecision.Deny;
			return FilterDecision.Neutral;
		}
	}
}