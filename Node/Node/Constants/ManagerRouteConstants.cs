namespace Stardust.Node.Constants
{
    public static class ManagerRouteConstants
    {
        public const string NodeHasBeenInitialized = "manager/nodeinit";

        public const string Heartbeat = "manager/heartbeat";

        public const string JobHasBeenCanceled = "manager/jobcanceled/{jobId}";

        public const string JobDone = "manager/jobdone/{jobId}";

        public const string JobFailed = "manager/jobfailed/{jobId}";

        public const string JobProgress = "manager/jobprogress";
    }
}