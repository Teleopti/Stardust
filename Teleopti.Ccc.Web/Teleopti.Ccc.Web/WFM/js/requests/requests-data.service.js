(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsData', ['$http', requestsDataService]);
		

	function requestsDataService($http) {
		var loadTextAndAbsenceRequestsUrl = '../api/Requests/loadTextAndAbsenceRequests';

		this.getAllRequestsPromise = function(filter) {			

			return $http.post(loadTextAndAbsenceRequestsUrl, normalizePeriod(filter.period));
		};

		function normalizePeriod(period) {
			if (!period) period = {
				periodStart: new Date(),
				periodEnd: moment().add(1, 'day').toDate()
			};
			return {
				StartDate: { Date: period.periodStart },
				EndDate: { Date: period.periodEnd }
			};
		};

	}
})();