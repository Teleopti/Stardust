namespace Manager.Integration.Test.Constants
{
    public static class ManagerRouteConstants
    {
        public const string JobIdOptionalParameter = "{jobId}";


        public const string Job = "manager/job";

        public const string CancelJob = "manager/job/" + JobIdOptionalParameter;

        public const string GetJobHistory = "manager/job/" + JobIdOptionalParameter;
    }
}