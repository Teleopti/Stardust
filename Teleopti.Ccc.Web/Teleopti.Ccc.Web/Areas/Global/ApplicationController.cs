using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ApplicationController : ApiController
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;

		private static readonly IEnumerable<AreaWithPermissionPath> _areaWithPermissionPaths = new List<AreaWithPermissionPath>
		{
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebForecasts,() => UserTexts.Resources.Forecasts,"forecasting"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebSchedules,() => UserTexts.Resources.OpenSchedulePage,"resourceplanner", new Link{href = "api/ResourcePlanner/Filter",rel = "filters"}),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPermissions,() => UserTexts.Resources.OpenPermissionPage,"permissions"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.Outbound,() => UserTexts.Resources.Outbound,"outbound"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPeople,() => UserTexts.Resources.People,"people"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => UserTexts.Resources.SeatPlan,"seatPlan"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => UserTexts.Resources.SeatMap,"seatMap")
		};

		public ApplicationController(IPermissionProvider permissionProvider, IToggleManager toggleManager)
		{
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
		}

		[UnitOfWork, HttpGet, Route("api/Global/Application/Areas")]
		public virtual IEnumerable<object> GetAreas()
		{
			return 
				_areaWithPermissionPaths.Where(a => _permissionProvider.HasApplicationFunctionPermission(a.Path) && isPathEnabled(a.Path))
					.Select(a => new { Name = a.Name(), a.InternalName, _links = a.Links.ToArray() }).ToArray();
		}

		private bool isPathEnabled(string path)
		{
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.WebSchedules))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_ResourcePlanner_32892);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.Outbound))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_Outbound_Campaign_32696);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.SeatPlanner))
			{
				return _toggleManager.IsEnabled(Toggles.SeatPlanner_Logon_32003);
			}
				
			return true;
		}
	}
}