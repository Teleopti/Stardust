namespace Teleopti.Ccc.Domain.Exceptions
{
	public class ScenarioMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public ScenarioMissingInAnalyticsException() : base("Scenario")
		{
		}
	}
}