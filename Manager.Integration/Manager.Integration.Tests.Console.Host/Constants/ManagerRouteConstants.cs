namespace Manager.IntegrationTest.Console.Host.Constants
{
	public class ManagerRouteConstants
	{
		public const string JobIdOptionalParameter = "{jobId}";

		public const string Job = "job";

		public const string CancelJob = "job/" + JobIdOptionalParameter;

		public const string GetJobHistory = "job/" + JobIdOptionalParameter;
	}
}