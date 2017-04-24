(function() {
	'use strict';
	angular.module('wfm.reports').service('ReportsService', reportsService);
	reportsService.$inject = ['$http', '$q'];

	function reportsService($http, $q) {
		var urlForGetPermittedReports = '../api/Reports/Navigations';
		var urlForGetCategorizedReports = '../api/Reports/NavigationsCategorized';
		this.getPermittedReports = getPermittedReports;
		this.getCategorizedReports = getCategorizedReports;

		//old
		function getPermittedReports() {

			var deferred = $q.defer();
			$http.get(urlForGetPermittedReports).success(function(data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

		//new
		function getCategorizedReports() {
			var deferred = $q.defer();
			$http.get(urlForGetCategorizedReports).success(function(data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

	}
})();
