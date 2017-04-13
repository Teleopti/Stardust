using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface ITeamScorecardModel
    {
        IScorecard Scorecard { get; set; }
        string SiteAndTeam { get; }
    }
}