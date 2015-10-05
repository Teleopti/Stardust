(function () {
	'use strict';

	var rtaModule = angular.module('wfm.rta');
	rtaModule.service('RtaOrganizationService', [
	   '$filter', '$q', 'RtaService', function ($filter, $q, RtaService) {

	  	var service = {};
			service.sites = RtaService.getSites.query();
			service.teams = [];
			var agents = [];


	   	service.getSiteName = function (siteIdParam) {
				var deferred = $q.defer();
				service.sites.$promise.then(function(){
					var siteName = null;
		   		var result = $filter('filter')(service.sites, { Id: siteIdParam });

		   		if (result.length > 0) {
		   			siteName = result[0].Name;
		   		}
		   		deferred.resolve(siteName);
				})
	   		return deferred.promise;
	   	};

	   	service.getTeamName = function (teamIdParam, siteIdParam) {
				var deferred = $q.defer();
				if(service.teams.length === 0) service.teams = RtaService.getTeams.query({siteId: siteIdParam});
				service.teams.$promise.then(function(){
					var teamName = null;
					var result = $filter('filter')(service.teams, { Id: teamIdParam });
			
		   		if (result.length > 0) {
		   			teamName = result[0].Name;
		   		}
					deferred.resolve(teamName);
				})
	   		return deferred.promise;
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
	   		return service.sites;
	   	};

	   	service.getTeams = function (siteId) {
			service.teams = RtaService.getTeams.query({ siteId: siteId });
			return service.teams;
	   	};

		  service.getAgents = function (teamId) {
			agents = RtaService.getAgents.query({ teamId: teamId });
			return agents;
			};

	   	return service;
	   }]);
})();
