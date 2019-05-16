namespace Stardust.Node.Constants
{
	public static class NodeRouteConstants
	{
		public const string JobIdOptionalParameter = "{jobId}";

		public const string Job = "job";

		public const string CancelJob = "job/" + JobIdOptionalParameter;

		public const string UpdateJob = "job/" + JobIdOptionalParameter;

		public const string IsAlive = "ping";

		public const string IsWorking = "isworking";

		public const string IsIdle = "isIdle";
	}
}