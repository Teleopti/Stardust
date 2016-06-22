namespace Teleopti.Ccc.Domain.Exceptions
{
	public class TimeZoneMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public TimeZoneMissingInAnalyticsException() : base("Time Zone")
		{
		}
	}
}