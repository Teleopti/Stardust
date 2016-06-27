namespace Teleopti.Ccc.Domain.Exceptions
{
	public class WorkloadMissingInAnalyticsException : DataMissingInAnalyticsException
	{
		public WorkloadMissingInAnalyticsException() : base("Workload")
		{
		}
	}
}