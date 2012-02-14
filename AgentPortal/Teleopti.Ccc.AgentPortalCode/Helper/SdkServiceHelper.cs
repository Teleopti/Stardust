using System.Configuration;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.Sdk.Client;
using TeleoptiOrganizationService = Teleopti.Ccc.Sdk.Client.SdkServiceReference.TeleoptiOrganizationService;
using TeleoptiSchedulingService = Teleopti.Ccc.Sdk.Client.SdkServiceReference.TeleoptiSchedulingService;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class SdkServiceHelper
    {
        private static readonly SdkServiceClient Instance = new SdkServiceClient(new SessionStateProvider(), GetServiceUrl());

        public static TeleoptiCccLogOnService LogOnServiceClient { get { return Instance.LogOnServiceClient; } }
        public static TeleoptiSchedulingService SchedulingService { get { return Instance.SchedulingService; } }
        public static TeleoptiOrganizationService OrganizationService { get { return Instance.OrganizationService; } }
        public static TeleoptiOrganizationService1 InternalService { get { return Instance.TeleoptiInternalService; } }
        public static TeleoptiCccSdkService TeleoptiSdkService { get { return Instance.TeleoptiSdkService; } }

        private static string GetServiceUrl()
        {
            var agentPortalSettings =
                (ClientSettingsSection)
                ConfigurationManager.GetSection("applicationSettings/Teleopti.Ccc.AgentPortal.Properties.Settings");
            return
                agentPortalSettings.Settings.Get("Teleopti_Ccc_AgentPortal_SdkServiceReference_TeleoptiCccSdkService").Value.
                    ValueXml.InnerText;
        }

        public static void Dispose()
        {
            Instance.Dispose();
        }
    }
}
