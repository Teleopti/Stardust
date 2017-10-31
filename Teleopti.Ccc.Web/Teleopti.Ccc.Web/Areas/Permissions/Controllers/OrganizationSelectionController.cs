using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions)]
	public class OrganizationSelectionController : ApiController
	{
		private readonly OrganizationSelectionProvider _organizationSelectionProvider;

		public OrganizationSelectionController(OrganizationSelectionProvider organizationSelectionProvider)
		{
			_organizationSelectionProvider = organizationSelectionProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/OrganizationSelection")]
		public virtual object GetOrganizationSelection()
		{
			return _organizationSelectionProvider.Provide(true);
		}
	}
}