using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public class TeamCollection : ITeamCollection
    {
        private readonly string _functionPath;
        private readonly IEnumerable<ITeam> _allTeams;
        private readonly DateOnly _queryDate;

        public TeamCollection(string functionPath, IEnumerable<ITeam> allTeams, DateOnly queryDate)
        {
            _functionPath = functionPath;
            _allTeams = allTeams;
            _queryDate = queryDate;
        }

        public IEnumerable<ITeam> AllPermittedTeams
        {
            get
            {
                HashSet<ITeam> tempList = new HashSet<ITeam>();
                foreach (var team in _allTeams)
                {
                    if (TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(_functionPath, _queryDate, team))
                        tempList.Add(team);
                }

                return new ReadOnlyCollection<ITeam>(tempList.ToList());
            }
        }

        public IEnumerable<ISite> AllPermittedSites
        {
            get
            {
                HashSet<ISite> ret = new HashSet<ISite>();
                foreach (var site in _allTeams.Select(t=>t.Site).Distinct())
                {
                    if (TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(_functionPath, _queryDate, site))
                        ret.Add(site);
                }

                return new ReadOnlyCollection<ISite>(ret.ToList());
            }
        }
    }
}