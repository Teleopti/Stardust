using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class TeamProvider : ITeamProvider
	{
		private readonly ITeamRepository _repository;
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly IPermissionProvider _permissionProvider;

		public TeamProvider(ITeamRepository repository, IPermissionProvider permissionProvider, IPeopleSearchProvider searchProvider)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_searchProvider = searchProvider;
		}

		public IEnumerable<ITeam> GetPermittedTeams(DateOnly date, string functionPath)
		{
			var teams = _repository.FindAllTeamByDescription() ?? new ITeam[] { };
			return (from t in teams
					where _permissionProvider.HasTeamPermission(functionPath, date, t)
					select t).ToArray();
		}

		public IEnumerable<ITeam> GetPermittedNotEmptyTeams(DateOnly date, string functionPath)
		{
			var teams = _repository.FindAllTeamByDescription() ?? new ITeam[] { };
			return (from t in teams
				where _permissionProvider.HasTeamPermission(functionPath, date, t) && hasTeamMembers(t, date)
				select t).ToArray();
		}

		private bool hasTeamMembers(ITeam team, DateOnly date)
		{
			return _searchProvider.FindPersonIds(date, new [] { team.Id.Value }, null).Count > 0;
		}
	}
}