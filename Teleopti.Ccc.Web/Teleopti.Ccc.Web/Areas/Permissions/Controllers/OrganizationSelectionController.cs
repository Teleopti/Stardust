using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class OrganizationSelectionController : ApiController
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private const string SiteType = "Site";
		private const string TeamType = "Team";
		private const string BusinessUnitType = "BusinessUnit";

		public OrganizationSelectionController(ICurrentBusinessUnit currentBusinessUnit, ISiteRepository siteRepository, ICurrentHttpContext currentHttpContext, IBusinessUnitRepository businessUnitRepository)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_siteRepository = siteRepository;
			_currentHttpContext = currentHttpContext;
			_businessUnitRepository = businessUnitRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/OrganizationSelection")]
		public virtual object GetOrganizationSelection()
		{
			IBusinessUnit businessUnit = null;
			if (_currentHttpContext != null)
			{
				var buid = UnitOfWorkAspect.BusinessUnitIdForRequest(_currentHttpContext);
				if (buid.HasValue) businessUnit = _businessUnitRepository.Load(buid.Value);
			}

			if (businessUnit == null) businessUnit = _currentBusinessUnit.Current();
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