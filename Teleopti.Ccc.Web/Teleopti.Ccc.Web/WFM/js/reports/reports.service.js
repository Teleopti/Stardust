(function() {
	'use strict';
	angular.module('wfm.reports').service('reportsService', reportsService);
	reportsService.$inject = ['$http', '$q'];

	function reportsService($http, $q) {
		var urlForGetPermittedReports = '../api/Reports/Navigations';
		this.getPermittedReports = getPermittedReports;

		function getPermittedReports() {

			var deferred = $q.defer();
			$http.get(urlForGetPermittedReports).success(function(data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

	}
})();