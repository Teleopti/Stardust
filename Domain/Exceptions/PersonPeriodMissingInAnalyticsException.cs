namespace Teleopti.Ccc.Domain.Exceptions
{
	public class PersonPeriodMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public PersonPeriodMissingInAnalyticsException() : base("PersonPeriod")
		{
		}
	}
}