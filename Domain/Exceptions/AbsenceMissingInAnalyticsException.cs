namespace Teleopti.Ccc.Domain.Exceptions
{
	public class AbsenceMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public AbsenceMissingInAnalyticsException() : base("Absence")
		{
		}
	}
}