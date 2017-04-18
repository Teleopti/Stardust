using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public class TeamScorecardModel : ITeamScorecardModel
    {
        private readonly ITeam _team;
        
        public TeamScorecardModel(ITeam team)
        {
            _team = team;
        }

        public IScorecard Scorecard
        {
            get
            {
                if (_team.Scorecard == null) return ScorecardProvider.NullScorecard;
                return _team.Scorecard;
            }
            set
            {
                if (ScorecardProvider.NullScorecard.Equals(value))
                    value = null;

                _team.Scorecard = value;
            }
        }

        public string SiteAndTeam
        {
            get { return _team.SiteAndTeam; }
        }
    }
}