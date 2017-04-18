using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public interface ISetScorecardView
    {
        void SetSites(IEnumerable<ISite> sites);
        void SetScorecards(IEnumerable<IScorecard> scorecards);
        void SetSelectedSite(ISite site);
        void SetTeams(IList<ITeamScorecardModel> models);
        bool InvokeRequired { get; }
        IAsyncResult BeginInvoke(Delegate method,params object[] args);
    }
}