using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ApplicationController : ApiController
	{
		private readonly IPermissionProvider _permissionProvider;

		private static readonly IEnumerable<AreaWithPermissionPath> _areaWithPermissionPaths = new List<AreaWithPermissionPath>
		{
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage,() => UserTexts.Resources.OpenForecaster,"forecasting"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenSchedulePage,() => UserTexts.Resources.OpenSchedulePage,"resourceplanner", new Link{href = "api/ResourcePlanner/Filter",rel = "filters"}),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage,() => UserTexts.Resources.OpenPermissionPage,"permissions"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage,() => "Outbound","outbound"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,() => "People","people"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => UserTexts.Resources.SeatPlan,"seatPlan"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => UserTexts.Resources.SeatMap,"seatMap"),
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
					.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() });
		}
	}
}