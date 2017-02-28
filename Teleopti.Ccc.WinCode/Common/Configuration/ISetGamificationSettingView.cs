using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface ISetGamificationSettingView
    {
        void SetSites(IEnumerable<ISite> sites);
        void SetGamificationSettings(IEnumerable<IGamificationSetting> gamificationSettings);
        void SetSelectedSite(ISite site);
        void SetTeams(IList<TeamGamificationSettingModel> models);
        bool InvokeRequired { get; }
        IAsyncResult BeginInvoke(Delegate method,params object[] args);
	}
}