(function () {
	'use strict';

	var rtaModule = angular.module('wfm.rta');
	rtaModule.service('RtaOrganizationService', [
	   '$filter', '$q', 'RtaService', function ($filter, $q, RtaService) {

	  	var service = {};
			service.sites = RtaService.getSites ? RtaService.getSites() : null;
			service.teams = undefined;

	   	service.getSiteName = function (siteId) {
				var deferred = $q.defer();
				service.sites.then(function(data){
					var siteName = null;
					var result = $filter('filter')(data, { Id: siteId });
		   		if (result.length > 0) {
		   			siteName = result[0].Name;
		   		}
		   		deferred.resolve(siteName);
				})
	   		return deferred.promise;
	   	};

	   	service.getTeamName = function (teamId) {
				var deferred = $q.defer();
				service.teams = service.teams;
				service.teams.$promise.then(function(data){
					var teamName = null;
					var result = $filter('filter')(data, { Id: teamId });
					if (result.length > 0) {
		   			teamName = result[0].Name;
		   		}
					deferred.resolve(teamName);
				})
	   		return deferred.promise;
	   	};

	   	return service;
	   }]);
})();
