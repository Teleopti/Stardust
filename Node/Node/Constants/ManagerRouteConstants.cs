namespace Stardust.Node.Constants
{
    public static class ManagerRouteConstants
    {
        public const string NodeHasBeenInitialized = "manager/nodeinit";

        public const string Heartbeat = "manager/heartbeat";

        public const string JobHasBeenCanceled = "manager/status/cancel/{jobId}";

        public const string JobDone = "manager/status/done/{jobId}";

        public const string JobFailed = "manager/status/fail/{jobId}";

        public const string JobProgress = "manager/status/progress";
    }
}