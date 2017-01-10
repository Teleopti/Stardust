(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaOrganizationService', rtaOrganizationService);

	rtaOrganizationService.$inject = ['$filter', '$q', 'rtaService'];

	function rtaOrganizationService($filter, $q, rtaService) {
		var sites = rtaService.getSites ? rtaService.getSites() : null;
		var teams = undefined;

		var service = {
			getSiteName: getSiteName,
			getTeamName: getTeamName,
			sites: sites,
			teams: teams
		};

		return service;
		/////////////////

		function getSiteName(siteId) {
			var deferred = $q.defer();
			service.sites.then(function(data) {
				var siteName = null;
				var result = $filter('filter')(data, {
					Id: siteId
				});
				if (result.length > 0) {
					siteName = result[0].Name;
				}
				deferred.resolve(siteName);
			})
			return deferred.promise;
		};

		function getTeamName(teamId) {
			var deferred = $q.defer();
			service.teams = service.teams;
			service.teams.$promise.then(function(data) {
				var teamName = null;
				var result = $filter('filter')(data, {
					Id: teamId
				});
				if (result.length > 0) {
					teamName = result[0].Name;
				}
				deferred.resolve(teamName);
			})
			return deferred.promise;
		};
	};
})();
