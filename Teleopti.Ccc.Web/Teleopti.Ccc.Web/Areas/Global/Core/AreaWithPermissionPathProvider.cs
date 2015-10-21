using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{
	public class AreaWithPermissionPathProvider : IAreaWithPermissionPathProvider
	{

		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;

		private static readonly IEnumerable<AreaWithPermissionPath> wfmAreaWithPermissionPaths = new List<AreaWithPermissionPath>
		{
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebForecasts,() => Resources.Forecasts,"forecasting"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebSchedules,() => Resources.OpenSchedulePage,"resourceplanner", new Link{href = "api/ResourcePlanner/Filter",rel = "filters"}),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPermissions,() => Resources.OpenPermissionPage,"permissions"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.Outbound,() => Resources.Outbound,"outbound"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPeople,() => Resources.People,"people"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => Resources.SeatPlan,"seatPlan"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner,() => Resources.SeatMap,"seatMap")
		};


		public AreaWithPermissionPathProvider(IPermissionProvider permissionProvider, IToggleManager toggleManager)
		{
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
		}

		public IEnumerable<AreaWithPermissionPath> GetWfmAreasWithPermissions()
		{
			return wfmAreaWithPermissionPaths
				.Where(a => _permissionProvider.HasApplicationFunctionPermission(a.Path) && isPathEnabled(a.Path));
		}

		public IEnumerable<object> GetAreasWithPermissions()
		{
			var areas = new List<object>();

			tryToAddArea("MyTime", DefinedRaptorApplicationFunctionPaths.MyTimeWeb, areas);
			tryToAddArea("Anywhere", DefinedRaptorApplicationFunctionPaths.Anywhere, areas);
			tryToAddWfmArea(areas);
			tryToAddArea("HealthCheck", string.Empty, areas);
			tryToAddArea("Messages", string.Empty, areas);
			tryToAddArea("Reporting", string.Empty, areas);
			return areas;
		}

		private void tryToAddArea(string areaName, string functionPath, List<object> areas)
		{
			if (_permissionProvider.HasApplicationFunctionPermission(functionPath) || functionPath.IsEmpty())
			{
				areas.Add(new { Name = areaName });
			}
		}

		private void tryToAddWfmArea(List<object> areas)
		{
			var wfmAreas = GetWfmAreasWithPermissions().ToList();
			if (wfmAreas.Count > 0)
			{
				areas.Add(new
				{
					Name = "WFM",
					SubAreas = wfmAreas.Select(area => new { Name = area.Name() }).ToList()
				});
			}
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