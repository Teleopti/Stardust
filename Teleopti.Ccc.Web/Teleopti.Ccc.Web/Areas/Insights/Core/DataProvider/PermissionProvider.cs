using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class PermissionProvider : IPermissionProvider
	{
		private readonly ICurrentAuthorization _authorization;

		public PermissionProvider(ICurrentAuthorization authorization)
		{
			_authorization = authorization;
		}

		public InsightsPermission GetInsightsPermission(IPerson person, DateOnly? date)
		{
			return new InsightsPermission
			{
				CanViewReport = isPermitted(DefinedRaptorApplicationFunctionPaths.ViewInsightsReport, date, person),
				CanEditReport = isPermitted(DefinedRaptorApplicationFunctionPaths.EditInsightsReport, date, person)
			};
		}

		private bool isPermitted(string permission, DateOnly? date, IPerson person)
		{
			return date == null
				? _authorization.Current().IsPermitted(permission)
				: _authorization.Current().IsPermitted(permission, date.Value, person);
		}
	}
}
