namespace Manager.IntegrationTest.Constants
{
	public static class ManagerRouteConstants
	{
		public const string JobIdOptionalParameter = "{jobId}";

		public const string Job = "job";

		public const string CancelJob = "job/" + JobIdOptionalParameter;

		public const string GetJobHistory = "job/" + JobIdOptionalParameter;

		public const string Nodes = "node/";

		public const string Ping = "ping/";
	}
}