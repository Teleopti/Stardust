(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaNamesFormatService', rtaNamesFormatService);

	function rtaNamesFormatService() {
		var service = {
			getSelectedFieldText: getSelectedFieldText
		}
		return service;

		function getSelectedFieldText(sites,siteIds, teamIds, siteString, teamString) {
			siteString = siteString || "Sites: ";
			teamString = teamString || "Teams: ";
			var selectedOrg = getSelectionArray(sites,siteIds, teamIds);
			var selectedFieldText = '';
			if (selectedOrg['siteNames'].length > 0 || selectedOrg['teamNames'].length > 0) {
				if (selectedOrg['siteNames'].length > 0) {
					selectedFieldText = siteString;
					selectedOrg['siteNames'].forEach(function (siteName) {
						selectedFieldText = selectedFieldText + siteName + ", ";
					});
					selectedFieldText = selectedFieldText + selectedOrg['sitesLimitStr'];
				}
				
				if (selectedOrg['teamNames'].length > 0) {
					{
						selectedFieldText = selectedFieldText + teamString;
						selectedOrg['teamNames'].forEach(function (teamName) {
						selectedFieldText = selectedFieldText + teamName + ", ";
						})
					}
					if (selectedOrg['teamsLimitStr'].length == 0)
						selectedFieldText = selectedFieldText.slice(0, -2);
					else
						selectedFieldText = selectedFieldText + selectedOrg['teamsLimitStr'];
				}
				else {
					if (selectedOrg['sitesLimitStr'].length == 0)
						selectedFieldText = selectedFieldText.slice(0, -2);
				}
			}
			return selectedFieldText;
		}

		function getSelectionArray(sites,siteIds, teamIds) {
			var countTeams = 1;
			var countSites = 1;
			var maxCount = 2;
			var siteOrTeamNames = [];
			siteOrTeamNames['siteNames'] = []
			siteOrTeamNames['teamNames'] = []
			if ((siteIds.length > 0 || teamIds.length > 0)) {

				sites.forEach(function (site) {
					if (siteIds.indexOf(site.Id) > -1 && countSites <= maxCount) {
						++countSites;
						siteOrTeamNames['siteNames'] = siteOrTeamNames['siteNames'].concat([site.Name]);
					}
					if (teamIds.length > 0 && countTeams <= maxCount) {
						site.Teams.forEach(function (t) {

							if (teamIds.indexOf(t.Id) > -1 && countTeams <= maxCount) {
								++countTeams;
								siteOrTeamNames['teamNames'] = siteOrTeamNames['teamNames'].concat([t.Name]);
							}
						})
					}
				});
			}
			siteOrTeamNames['sitesLimitStr'] = (siteIds.length > maxCount && teamIds.length === 0) ? "..." 
			: ((siteIds.length > maxCount) ? "..., " : "");
			siteOrTeamNames['teamsLimitStr'] = teamIds.length > maxCount ? "..." : "";
			return siteOrTeamNames;
		}
	};
})();
