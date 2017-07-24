using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class AreaWithPermissionPathProvider : IAreaWithPermissionPathProvider
	{

		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;
		private readonly ILicenseActivatorProvider _licenseActivatorProvider;
		private readonly IApplicationFunctionsToggleFilter _applicationFunctionsToggleFilter;

		private static readonly IEnumerable<AreaWithPermissionPath> wfmAreaWithPermissionPaths = new List<AreaWithPermissionPath>
		{
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebForecasts, () => Resources.Forecasts, "forecasting"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPlans, () => Resources.OpenPlansPage, "resourceplanner", new Link {href = "api/ResourcePlanner/Filter", rel = "filters"}),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPermissions, () => Resources.OpenPermissionPage, "permissions"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.Outbound, () => Resources.Outbound, "outbound"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPeople, () => Resources.People, "people"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebRequests, () => Resources.Requests, "requests"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner, () => Resources.SeatPlan, "seatPlan"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner, () => Resources.SeatMap, "seatMap"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, () => Resources.RealTimeAdherence, "rta"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebIntraday, () => Resources.Intraday, "intraday"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, () => Resources.TeamsModule, "teams"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.AccessToReports, () => Resources.Reports, "reports"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebStaffing, () => Resources.WebStaffing, "staffing"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.MyTimeWeb, () => Resources.MyTimeWeb, "myTime"),
		};

		public AreaWithPermissionPathProvider(IPermissionProvider permissionProvider, IToggleManager toggleManager, ILicenseActivatorProvider licenseActivatorProvider, IApplicationFunctionsToggleFilter applicationFunctionsToggleFilter)
		{
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
			_licenseActivatorProvider = licenseActivatorProvider;
			_applicationFunctionsToggleFilter = applicationFunctionsToggleFilter;
		}

		public IEnumerable<AreaWithPermissionPath> GetWfmAreasWithPermissions()
		{
			var systemFunctions = _applicationFunctionsToggleFilter.FilteredFunctions();
			var result = wfmAreaWithPermissionPaths
				.Where(a => _permissionProvider.HasApplicationFunctionPermission(a.Path) && isPathEnabled(a.Path) 
				&& isPathLicensed(systemFunctions, a.Path));

			return result;
		}

		public IEnumerable<object> GetAreasWithPermissions()
		{
			var areas = new List<object>();

			tryToAddArea("MyTime", DefinedRaptorApplicationFunctionPaths.MyTimeWeb, areas);
			tryToAddArea("Anywhere", DefinedRaptorApplicationFunctionPaths.Anywhere, areas);
			tryToAddWfmArea(areas);
			tryToAddArea("HealthCheck", DefinedRaptorApplicationFunctionPaths.OpenPermissionPage, areas);
			tryToAddAreaWithLicense("Messages", DefinedLicenseOptionPaths.TeleoptiCccSmsLink, areas);
			tryToAddArea("Reporting", DefinedRaptorApplicationFunctionPaths.AccessToReports, areas);
			return areas;
		}

		private void tryToAddArea(string areaName, string functionPath, List<object> areas)
		{
			if (functionPath.IsEmpty() || _permissionProvider.HasApplicationFunctionPermission(functionPath))
			{
				areas.Add(new { Name = areaName });
			}
		}

		private void tryToAddAreaWithLicense(string areaName, string licensePath, List<object> areas)
		{
			if (licensePath.IsEmpty() || _licenseActivatorProvider.Current().EnabledLicenseOptionPaths.Contains(licensePath))
			{
				areas.Add(new { Name = areaName });
			}
		}

		private void tryToAddWfmArea(List<object> areas)
		{
			var wfmAreas = GetWfmAreasWithPermissions().ToArray();
			if (wfmAreas.Any() && _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.Anywhere))
			{
				areas.Add(new
				{
					Name = "WFM",
					SubAreas = wfmAreas.Select(area => new { Name = area.Name() }).ToArray()
				});
			}
		}

		private bool isPathEnabled(string path)
		{
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.WebPlans))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_ResourcePlanner_32892);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning))
			{
				return _toggleManager.IsEnabled(Toggles.WfmIntraday_MonitorActualvsForecasted_35176);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.WebRequests))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_Requests_Basic_35986);
			}
			if(path.Equals(DefinedRaptorApplicationFunctionPaths.WebIntraday))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_Intraday_38074);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.AccessToReports))
			{
				return _toggleManager.IsEnabled(Toggles.WfmReportPortal_Basic_38825);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.WebStaffing))
			{
				return _toggleManager.IsEnabled(Toggles.WfmStaffing_AllowActions_42524);
			}
			
			return true;
		}

		private bool isPathLicensed(AllFunctions systemFunctions, string path)
		{
			var systemFunction = systemFunctions.FindByFunctionPath(path);
			return systemFunction != null && systemFunction.IsLicensed;
		}
	}
}