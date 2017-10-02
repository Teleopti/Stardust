using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions)]
	public class OrganizationSelectionController : ApiController
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISiteRepository _siteRepository;
		private const string SiteType = "Site";
		private const string TeamType = "Team";
		private const string BusinessUnitType = "BusinessUnit";

		public OrganizationSelectionController(ICurrentBusinessUnit currentBusinessUnit, ISiteRepository siteRepository)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_siteRepository = siteRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/OrganizationSelection")]
		public virtual object GetOrganizationSelection()
		{
			var businessUnit = _currentBusinessUnit.Current();
			return
				new
				{
					BusinessUnit =
						new
						{
							businessUnit.Name,
							Id = businessUnit.Id.GetValueOrDefault(),
							Type = BusinessUnitType,
							ChildNodes =
								_siteRepository.LoadAll()
									.Select(
										s =>
											new
											{
												s.Description.Name,
												Id = s.Id.GetValueOrDefault(),
												Type = SiteType,
												ChildNodes =
													s.TeamCollection.Where(t => t.IsChoosable)
														.Select(t => new { Type = TeamType, t.Description.Name, Id = t.Id.GetValueOrDefault() }).ToArray()
											})
									.ToArray()
						},
					DynamicOptions =
						Enum.GetValues(typeof(AvailableDataRangeOption)).OfType<AvailableDataRangeOption>()
							.Select(o => new { RangeOption = o, Name = Enum.GetName(typeof(AvailableDataRangeOption), o) }).ToArray()
				};
		}
	}
}