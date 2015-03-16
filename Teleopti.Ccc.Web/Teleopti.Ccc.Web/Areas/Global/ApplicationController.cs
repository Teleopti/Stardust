using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ApplicationController : ApiController
	{
		private readonly IPermissionProvider _permissionProvider;

		private static readonly IEnumerable<AreaWithPermissionPath> _areaWithPermissionPaths = new List<AreaWithPermissionPath>
		{
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage,() => UserTexts.Resources.OpenForecaster,"Forecaster"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenSchedulePage,() => UserTexts.Resources.OpenSchedulePage,"Schedules"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage,() => UserTexts.Resources.OpenPermissionPage,"Permissions"),
		};

		public ApplicationController(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Global/Application/Areas")]
		public virtual IEnumerable<object> GetAreas()
		{
			return
				_areaWithPermissionPaths.Where(a => _permissionProvider.HasApplicationFunctionPermission(a.Path))
					.Select(a => new { Name = a.Name(), a.InternalName });
		}
	}
}