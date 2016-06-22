namespace Teleopti.Ccc.Domain.Exceptions
{
	public class BusinessUnitMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public BusinessUnitMissingInAnalyticsException() : base("Business Unit")
		{
		}
	}
}
