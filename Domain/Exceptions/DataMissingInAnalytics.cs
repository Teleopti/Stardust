using System;

namespace Teleopti.Ccc.Domain.Exceptions
{
	public class DataMissingInAnalytics : Exception
	{
		public DataMissingInAnalytics()
		{ }

		public DataMissingInAnalytics(string entityMissing) : base(getMessage(entityMissing))
		{ }

		public DataMissingInAnalytics(string entityMissing, Exception inner) : base(getMessage(entityMissing), inner)
		{ }

		private static string getMessage(string entityMissing)
		{
			return $"{entityMissing} is missing in analytics";
		}
	}
}
