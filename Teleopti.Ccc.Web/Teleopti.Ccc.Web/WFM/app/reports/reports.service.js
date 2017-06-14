(function() {
	'use strict';
	angular.module('wfm.reports').service('ReportsService', reportsService);
	reportsService.$inject = ['$http', '$q'];

	function reportsService($http, $q) {
		var urlForGetCategorizedReports = '../api/Reports/NavigationsCategorized';
		this.getCategorizedReports = getCategorizedReports;

		function getCategorizedReports() {
			var deferred = $q.defer();
			$http.get(urlForGetCategorizedReports).success(function(data) {
				deferred.resolve(data);
			});
			return deferred.promise;
		}

	}
})();
