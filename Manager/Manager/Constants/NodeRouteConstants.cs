namespace Stardust.Manager.Constants
{
	public static class NodeRouteConstants
	{
		public const string JobIdOptionalParameter = "{jobId}";

		public const string Job = "job";

		public const string CancelJobByJobId = "job/" + JobIdOptionalParameter;

		public const string UpdateJobByJobId = "job/" + JobIdOptionalParameter;

		public const string IsAlive = "ping";
	}
}