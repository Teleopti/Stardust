using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class OrganizationSelectionController : ApiController
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISiteRepository _siteRepository;

		public OrganizationSelectionController(ICurrentBusinessUnit currentBusinessUnit, ISiteRepository siteRepository)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_siteRepository = siteRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/OrganizationSelection")]
		public virtual object GetOrganizationSelection()
		{
			var currentBusinessUnit = _currentBusinessUnit.Current();
			return
				new
				{
					BusinessUnit =
						new
						{
							currentBusinessUnit.Name,
							Id = currentBusinessUnit.Id.GetValueOrDefault(),
							Sites =
								_siteRepository.LoadAll()
									.Select(
										s =>
											new
											{
												s.Description.Name,
												Id = s.Id.GetValueOrDefault(),
												Teams =
													s.TeamCollection.Where(t => t.IsChoosable)
														.Select(t => new { t.Description.Name, Id = t.Id.GetValueOrDefault() }).ToArray()
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