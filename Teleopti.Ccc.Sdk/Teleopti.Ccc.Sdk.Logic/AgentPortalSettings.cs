using System;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Sdk.Logic
{
    [Serializable]
    public class AgentPortalSettings : SettingValue
    {
        //Resolution
        public int Resolution { get; set; }
    }
}