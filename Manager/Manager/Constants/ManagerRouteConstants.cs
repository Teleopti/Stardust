namespace Stardust.Manager.Constants
{
    public static class ManagerRouteConstants
    {
        public const string StartJob = "manager/job";

        public const string Heartbeat = "manager/heartbeat";

        public const string CancelJob = "manager/job/{jobId}";

        public const string JobDone = "manager/jobdone/{jobId}";

        public const string JobFailed = "manager/jobfailed/{jobId}";

        public const string JobHasBeenCanceled = "manager/jobcanceled/{jobId}";

        public const string JobProgress = "manager/jobprogress";

        public const string NodeHasBeenInitialized = "manager/nodeinit";

        public const string GetJobHistory = "manager/job/{jobId}";
    }
}