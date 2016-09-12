using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
			var sites = _repository.LoadAllOrderByName() ?? new ISite[] { };
			return (from t in sites
					  where _permissionProvider.HasSitePermission(functionPath, date, t)
					  select t).ToArray();
		}

		public IEnumerable<Guid> GetTeamIdsUnderSite(Guid siteId)
		{
			var teams = _teamRepository.FindTeamsForSite(siteId);

			return teams.Select(team => team.Id.Value).ToList();
		} 
	}
}