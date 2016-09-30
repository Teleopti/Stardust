using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class SiteProvider : ISiteProvider
	{
		private readonly ISiteRepository _repository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;

		public SiteProvider(ISiteRepository repository, IPermissionProvider permissionProvider, ITeamRepository teamRepository)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
		}

		public IEnumerable<ISite> GetPermittedSites(DateOnly date, string functionPath)
		{
			var permittedSite = new List<ISite>();
			var sites = _repository.LoadAllOrderByName() ?? new ISite[] { };
			 foreach (var site in sites)
			{
				var teams = _teamRepository.FindTeamsForSite(site.Id.Value);
				if (teams.Any(team => _permissionProvider.HasTeamPermission(functionPath, date, team)))
				{
					permittedSite.Add(site);
				}
			}

			return permittedSite;
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