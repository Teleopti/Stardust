namespace Stardust.Node.Constants
{
	public class ManagerRouteConstants
	{
		public const string JobIdOptionalParameter = "{jobId}";

		public const string NodeHasBeenInitialized = "nodeinit";

		public const string Heartbeat = "heartbeat";

		public const string JobHasBeenCanceled = "status/cancel/" + JobIdOptionalParameter;

		public const string JobDone = "status/done/" + JobIdOptionalParameter;

		public const string JobFailed = "status/fail";

		public const string JobProgress = "status/progress";
	}
}