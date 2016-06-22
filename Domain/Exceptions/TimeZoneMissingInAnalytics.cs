namespace Teleopti.Ccc.Domain.Exceptions
{
	public class TimeZoneMissingInAnalytics : DataMissingInAnalytics
	{
		public TimeZoneMissingInAnalytics() : base("Time Zone")
		{
		}
	}
}