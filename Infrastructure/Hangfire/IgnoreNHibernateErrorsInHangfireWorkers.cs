using log4net.Core;
using log4net.Filter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class IgnoreNHibernateErrorsInHangfireWorkers : FilterSkeleton
	{
		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level != Level.Error)
				return FilterDecision.Neutral;
			if (!loggingEvent.ThreadName.Contains("Worker #"))
				return FilterDecision.Neutral;


			if (loggingEvent.LoggerName.StartsWith("NHibernate"))
				return FilterDecision.Deny;

			if (loggingEvent.LoggerName.Equals(typeof(NHibernateUnitOfWork).FullName))
				return FilterDecision.Deny;
			if (loggingEvent.LoggerName.Equals(typeof(AnalyticsUnitOfWork).FullName))
				return FilterDecision.Deny;


			return FilterDecision.Neutral;
		}
	}
}