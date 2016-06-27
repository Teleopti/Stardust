namespace Teleopti.Ccc.Domain.Exceptions
{
	public class DayOffMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public DayOffMissingInAnalyticsException() : base("DayOff")
		{
		}
	}
}