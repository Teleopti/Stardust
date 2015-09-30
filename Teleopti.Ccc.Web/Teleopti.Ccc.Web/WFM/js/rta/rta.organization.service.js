(function () {
	'use strict';

	var rtaModule = angular.module('wfm.rta');
	rtaModule.service('RtaOrganizationService', [
	   '$filter', 'RtaService', function ($filter, RtaService) {
	   	var service = {};
		var sites = [];
		var teams = [];
		var agents = [];

	   	service.getSiteName = function (siteIdParam) {
	   		var siteName = null;
	   		var result = $filter('filter')(sites, { Id: siteIdParam });

	   		if (result.length > 0) {
	   			siteName = result[0].Name;
	   		}
	   		return siteName;
	   	};

	   	service.getTeamName = function (teamIdParam) {
	   		var teamName = null;
			var result = $filter('filter')(teams, { Id: teamIdParam });

	   		if (result.length > 0) {
	   			teamName = result[0].Name;
	   		}
	   		return teamName;
	   	};

		//TODO
	   	service.getAgentsForSelectedTeams = function (selectedTeams) {

	   	};

        //TODO get all the teams for site with one server call with new backend method.
        service.getTeamsForSelectedSites = function (selectedSites) {
        	var selectedSitesTeams = [];
        	console.log(selectedSites);
            //selectedSitesTeams = RtaService.getTeamsForSelectedSites.query({selectedSites: selectedSites});
            return selectedSitesTeams;
        };

	   	service.getSites = function () {
			sites = RtaService.getSites.query();
	   		return sites;
	   	};

	   	service.getTeams = function (siteId) {
			teams = RtaService.getTeams.query({ siteId: siteId });
			return teams;
	   	};

		service.getAgents = function (teamId) {
			agents = RtaService.getAgents.query({ teamId: teamId });
			return agents;
		};

	   	return service;
	   }]);
})();