using Teleopti.Ccc.Sdk.Client;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class SdkServiceHelper
    {
        private static readonly SdkServiceClient Instance = new SdkServiceClient();

        public static ITeleoptiCccLogOnService LogOnServiceClient { get { return Instance.LogOnServiceClient; } }
        public static ITeleoptiSchedulingService SchedulingService { get { return Instance.SchedulingService; } }
        public static ITeleoptiOrganizationService OrganizationService { get { return Instance.OrganizationService; } }
        public static ITeleoptiCccSdkInternal InternalService { get { return Instance.TeleoptiInternalService; } }
        
        public static void Dispose()
        {
            Instance.Dispose();
        }
    }
}
