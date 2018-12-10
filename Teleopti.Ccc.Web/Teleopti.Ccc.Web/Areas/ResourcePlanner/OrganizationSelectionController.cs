using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;


namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class OrganizationHierarchyController : ApiController
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISiteRepository _siteRepository;
		private readonly IPermissionProvider _permissionProvider;
		private const string SiteType = "Site";
		private const string TeamType = "Team";
		private const string BusinessUnitType = "BusinessUnit";

		public OrganizationHierarchyController(ICurrentBusinessUnit currentBusinessUnit, ISiteRepository siteRepository, IPermissionProvider permissionProvider)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_siteRepository = siteRepository;
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork, HttpGet, Route("api/ResourcePlanner/OrganizationSelection")]
		public virtual object GetOrganizationSelection()
		{
			var businessUnit = _currentBusinessUnit.Current();
			var sites = _siteRepository.LoadAll()
				.Select(buildSite)
				.ToArray();
			return new
			{
				BusinessUnit = new
				{
					businessUnit.Name,
					Id = businessUnit.Id.GetValueOrDefault(),
					Type = BusinessUnitType,
					ChildNodes = sites,
					Choosable = true
				}
			};
		}

		private object buildSite(ISite s)
		{
			var teams = s.TeamCollection.Where(t => t.IsChoosable)
				.Select(t => new {Type = TeamType, t.Description.Name, Id = t.Id.GetValueOrDefault(), Choosable = permissionFor(t)})
				.ToArray();
			return new
			{
				s.Description.Name,
				Id = s.Id.GetValueOrDefault(),
				Type = SiteType,
				Choosable = teams.Any(x => x.Choosable),
				ChildNodes = teams
			};
		}

		private bool permissionFor(ITeam team)
		{
			return _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.CopySchedule, DateOnly.Today,
				team);
		}
	}
}