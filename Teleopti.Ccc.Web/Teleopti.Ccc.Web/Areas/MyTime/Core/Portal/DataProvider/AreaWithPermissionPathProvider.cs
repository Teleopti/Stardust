﻿using System.Collections.Generic;
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
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebForecasts, () => Resources.Forecasts, "forecast"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPlans, () => Resources.OpenPlansPage, "resourceplanner", new Link {href = "api/ResourcePlanner/Filter", rel = "filters"}),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebPermissions, () => Resources.OpenPermissionPage, "permissions"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.Outbound, () => Resources.Outbound, "outbound"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.Gamification, () => Resources.Gamification, "gamification"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.PeopleAccess, () => Resources.People, "people"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebRequests, () => Resources.Requests, "requests"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner, () => Resources.SeatPlan, "seatPlan"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.SeatPlanner, () => Resources.SeatMap, "seatMap"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, () => Resources.RealTimeAdherence, "rta"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebIntraday, () => Resources.Intraday, "intraday"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, () => Resources.TeamsModule, "teams"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.AccessToReports, () => Resources.Reports, "reports"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.WebStaffing, () => Resources.WebStaffing, "staffingModule"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.MyTimeWeb, () => Resources.MyTimeWeb, "myTime"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.BpoExchange, () => Resources.BpoExchange, "bpo"),
			new AreaWithPermissionPath(DefinedRaptorApplicationFunctionPaths.All, () => "API access", "apiaccess")
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

		public IEnumerable<AreaWithPermissionPath> GetWfmAreasList()
		{
			return wfmAreaWithPermissionPaths;
		}

		public IEnumerable<object> GetAreasWithPermissions()
		{
			var areas = new List<object>();

			tryToAddArea("MyTime", DefinedRaptorApplicationFunctionPaths.MyTimeWeb, areas);
			tryToAddArea("Anywhere", DefinedRaptorApplicationFunctionPaths.Anywhere, areas);
			tryToAddWfmArea(areas);
			tryToAddArea("HealthCheck", DefinedRaptorApplicationFunctionPaths.WebPermissions, areas);
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
				return _toggleManager.IsEnabled(Toggles.Wfm_WebPlan_Pilot_46815);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning))
			{
				return _toggleManager.IsEnabled(Toggles.WfmIntraday_MonitorActualvsForecasted_35176);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.WebForecasts))
			{
				return _toggleManager.IsEnabled(Toggles.WFM_Forecaster_Preview_74801);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.MyTimeWeb))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_AddMyTimeLink_45088);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.PeopleAccess))
			{
				return _toggleManager.IsEnabled(Toggles.Wfm_PeopleWeb_PrepareForRelease_47766);
			}
			if (path.Equals(DefinedRaptorApplicationFunctionPaths.Gamification))
			{
				return _toggleManager.IsEnabled(Toggles.WFM_Gamification_Permission_76546);
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