using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
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