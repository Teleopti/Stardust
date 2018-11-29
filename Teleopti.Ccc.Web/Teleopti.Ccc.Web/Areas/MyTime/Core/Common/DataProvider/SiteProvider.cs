using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class SiteProvider : ISiteProvider
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;

		public SiteProvider(IPermissionProvider permissionProvider, ITeamRepository teamRepository)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
		}

		public IEnumerable<ISite> GetShowListSites(DateOnly date, string functionPath)
		{
			var allTeams = _teamRepository.FindAllTeamByDescription() ?? new ITeam[] { };
			return allTeams.Where(team => _permissionProvider.HasTeamPermission(functionPath, date, team)).Select(t => t.Site)
				.Distinct();
		}

		public IEnumerable<ITeam> GetPermittedTeamsUnderSite(Guid siteId, DateOnly date, string functionPath)
		{
			var teams = _teamRepository.FindTeamsForSite(siteId) ?? new ITeam[] {};
			return (from t in teams
				where _permissionProvider.HasTeamPermission(functionPath, date, t)
				select t).ToArray();
		} 
	}
}