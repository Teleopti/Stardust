(function () {
	'use strict';

	var rtaModule = angular.module('wfm.rta');
	rtaModule.service('RtaOrganizationService', [
	   '$filter', '$q', 'RtaService', function ($filter, $q, RtaService) {

	  	var service = {};
			service.sites = RtaService.getSites.query();
			service.teams = [];
			var agents = [];


	   	service.getSiteName = function (siteIds) {
				var deferred = $q.defer();
				service.sites.$promise.then(function(data){
					var siteName = null;
					var result = $filter('filter')(data, { Id: siteIds[0] });
		   		if (result.length > 0) {
		   			siteName = data[0].Name;
		   		}
		   		deferred.resolve(siteName);
				})
	   		return deferred.promise;
	   	};

	   	service.getTeamName = function (teamIdParam, siteIdParam) {
				var deferred = $q.defer();
				if (service.teams.length === 0) service.teams = RtaService.getTeams.query({siteId: siteIdParam});
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

	   	service.getSites = function () {
	   		return service.sites;
	   	};

	   	service.getTeams = function (selectedSites) {
			service.teams =  RtaService.getTeamsForSelectedSites.query({siteIds: selectedSites});
			return service.teams;
	   	};

		  service.getAgents = function (teamId) {
			agents = RtaService.getAgents.query({ teamId: teamId });
			return agents;
			};

	   	return service;
	   }]);
})();
