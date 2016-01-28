namespace Stardust.Manager.Constants
{
    public static class ManagerRouteConstants
    {
        public const string JobIdOptionalParameter = "{jobId}";

        public const string StartJob = "manager/job";

        public const string CancelJob = "manager/job/" + JobIdOptionalParameter;

        public const string JobHasBeenCanceled = "manager/status/cancel/" + JobIdOptionalParameter;

        public const string JobDone = "manager/status/done/" + JobIdOptionalParameter;

        public const string JobFailed = "manager/status/fail/" + JobIdOptionalParameter;

        public const string JobProgress = "manager/status/progress";

        public const string NodeHasBeenInitialized = "manager/nodeinit";

        public const string Heartbeat = "manager/heartbeat";

        public const string GetJobHistory = "manager/job/" + JobIdOptionalParameter;
    }
}