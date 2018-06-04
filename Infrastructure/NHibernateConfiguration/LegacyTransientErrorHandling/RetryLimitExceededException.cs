using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public sealed class RetryLimitExceededException : Exception
	{
		public RetryLimitExceededException()
			: this("RetryLimitExceeded")
		{
		}

		public RetryLimitExceededException(string message)
			: base(message)
		{
		}

		public RetryLimitExceededException(Exception innerException)
			: base(innerException != null ? innerException.Message : "RetryLimitExceeded", innerException)
		{
		}

		public RetryLimitExceededException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}