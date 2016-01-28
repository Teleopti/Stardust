namespace Stardust.Manager.Constants
{
    public static class ManagerRouteConstants
    {
        public const string StartJob = "manager/job";

        public const string CancelJob = "manager/job/{jobId}";

        public const string JobHasBeenCanceled = "manager/status/cancel/{jobId}";

        public const string JobDone = "manager/status/done/{jobId}";

        public const string JobFailed = "manager/status/fail/{jobId}";

        public const string JobProgress = "manager/status/progress";

        public const string NodeHasBeenInitialized = "manager/nodeinit";

        public const string Heartbeat = "manager/heartbeat";

        public const string GetJobHistory = "manager/job/{jobId}";
    }
}