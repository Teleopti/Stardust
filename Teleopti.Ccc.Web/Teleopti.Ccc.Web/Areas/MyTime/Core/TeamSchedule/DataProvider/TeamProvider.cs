using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamProvider : ITeamProvider
	{
		private readonly ITeamRepository _repository;
		private readonly IPermissionProvider _permissionProvider;

		public TeamProvider(ITeamRepository repository, IPermissionProvider permissionProvider)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<ITeam> GetPermittedTeams(DateOnly date)
		{
			var teams = _repository.FindAllTeamByDescription() ?? new ITeam[] { };
			return (from t in teams
					where _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, date, t)
					select t).ToArray();
		}
	}
}