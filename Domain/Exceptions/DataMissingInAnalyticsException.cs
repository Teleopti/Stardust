using System;

namespace Teleopti.Ccc.Domain.Exceptions
{
	public class DataMissingInAnalyticsException : Exception
	{
		public DataMissingInAnalyticsException()
		{ }

		public DataMissingInAnalyticsException(string entityMissing) : base(getMessage(entityMissing))
		{ }

		public DataMissingInAnalyticsException(string entityMissing, Exception inner) : base(getMessage(entityMissing), inner)
		{ }

		private static string getMessage(string entityMissing)
		{
			return $"{entityMissing} is missing in analytics";
		}
	}
}
