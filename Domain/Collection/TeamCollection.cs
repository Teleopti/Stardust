using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

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
				var authorization = PrincipalAuthorization.Current();
				return _allTeams.Distinct().Where(t => authorization.IsPermitted(_functionPath, _queryDate, t)).ToArray();
			}
        }

        public IEnumerable<ISite> AllPermittedSites
        {
            get
            {
				var authorization = PrincipalAuthorization.Current();
				return _allTeams.Select(t => t.Site).Distinct().Where(s => authorization.IsPermitted(_functionPath, _queryDate, s))
					.ToArray();
            }
        }
    }
}