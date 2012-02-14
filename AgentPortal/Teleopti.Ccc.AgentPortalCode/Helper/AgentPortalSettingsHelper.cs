using System;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public static class AgentPortalSettingsHelper
    {
        public static int Resolution
        { 
            get 
            { 
                var settings = SdkServiceHelper.InternalService.GetAgentPortalSettingsByQuery(new GetAgentPortalSettingsQueryDto());
                return settings[0].Resolution;
            } 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SaveSettings(SaveAgentPortalSettingsCommandDto commandDto)
        {
            try
            {
                SdkServiceHelper.InternalService.ExecuteCommand(commandDto);
            }
            catch (Exception) // if we can't save for some reason we just ignore it
            {
                
            }
            
        }
    }
}