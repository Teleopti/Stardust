namespace Stardust.Manager.Constants
{
    public static class NodeRouteConstants
    {
        public const string JobIdOptionalParameter = "{jobId}";

        public const string Job = "node/job";

        public const string CancelJob = "node/job/" + JobIdOptionalParameter;

        public const string IsAlive = "node/ping";
    }
}