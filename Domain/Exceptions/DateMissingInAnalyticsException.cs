using System;

namespace Teleopti.Ccc.Domain.Exceptions
{
	public class DateMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public DateMissingInAnalyticsException(DateTime date) : base($"Date '{date.ToShortDateString()}'")
		{
		}
	}
}