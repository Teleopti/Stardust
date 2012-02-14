using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    public static class AppConfigHelper
    {
        public static IDictionary<string,string> LoadSettings()
        {
            ArrayOfKeyValueOfstringstringKeyValueOfstringstring[] appSettingsKeyValues = SdkServiceHelper.LogOnServiceClient.GetAppSettings();
            IDictionary<string, string> appSettings = new Dictionary<string, string>();

            foreach (var pair in appSettingsKeyValues)
            {
                appSettings.Add(pair.Key, Encryption.DecryptStringFromBase64(pair.Value, EncryptionConstants.Image1, EncryptionConstants.Image2));
            }

            return appSettings;
        }
    }
}
