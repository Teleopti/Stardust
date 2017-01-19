(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaBreadCrumbService', rtaBreadCrumbService);

	rtaBreadCrumbService.$inject = ['rtaRouteService'];

	function rtaBreadCrumbService(rtaRouteService) {

		var service = {
			getBreadCrumb: getBreadCrumb
		}

		return service;

		function getBreadCrumb(info) {
			var result = {};
			result.goBackToRootWithUrl = rtaRouteService.urlForRootInBreadcrumbs(info);
			if (info.siteIds.length > 1 && info.teamIds.length === 0) {
				result.siteName = "Multiple Sites";
			} else if (
				(info.teamIds.length > 0 && info.siteIds.length > 0) ||
				(info.teamIds.length > 1 || info.siteIds.length > 0)) {
				result.teamName = "Multiple Teams";
			} else if (info.agentsInfo.length > 0) {
				result.siteName = info.agentsInfo[0].SiteName;
				result.teamName = info.agentsInfo[0].TeamName;
				result.goBackToTeamsWithUrl = rtaRouteService.urlForTeamsInBreadcrumbs(info, info.agentsInfo[0].SiteId);
				result.showPath = true;
			} else {
				info.organization.forEach(function (site) {
					site.Teams.forEach(function (team) {
						if (team.Id === info.teamIds[0]) {
							result.siteName = site.Name;
							result.teamName = team.Name;
							result.goBackToTeamsWithUrl = rtaRouteService.urlForTeamsInBreadcrumbs(info, site.Id);
						}
					})
				})
			}
			return result;
		}
	};
})();
