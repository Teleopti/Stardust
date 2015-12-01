(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsData', ['$http', requestsDataService]);
		

	function requestsDataService($http) {
		var loadTextAndAbsenceRequestsUrl = '../api/Requests/loadTextAndAbsenceRequests';

		this.getAllRequestsPromise = function(filter) {			
			return $http.post(loadTextAndAbsenceRequestsUrl, normalizePeriod(filter.period));
		};

		function normalizePeriod(period) {
			return {
				StartDate: { Date: period.startDate },
				EndDate: { Date: period.endDate }
			};
		};

	}
})();