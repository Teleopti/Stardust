using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
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

		public IEnumerable<ITeam> GetPermittedTeams(DateOnly date, string functionPath)
		{
			var teams = _repository.FindAllTeamByDescription() ?? new ITeam[] { };
			return (from t in teams
					where _permissionProvider.HasTeamPermission(functionPath, date, t)
					select t).ToArray();
		}
	}
}