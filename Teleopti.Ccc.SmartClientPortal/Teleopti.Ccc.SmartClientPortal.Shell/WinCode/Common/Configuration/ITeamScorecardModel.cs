using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public interface ITeamScorecardModel
    {
        IScorecard Scorecard { get; set; }
        string SiteAndTeam { get; }
    }
}