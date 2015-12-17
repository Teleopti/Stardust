(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDataService', ['$http', 'requestsDefinitions', requestsDataService]);
		

	function requestsDataService($http, requestsDefinitions) {
		var loadTextAndAbsenceRequestsUrl = '../api/Requests/loadTextAndAbsenceRequests';
		var listRequestsUrl = '../api/Requests/listRequests';

		this.getAllRequestsPromise_old = function(filter, sortingOrders) {			
			return $http.post(loadTextAndAbsenceRequestsUrl, requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders));
		};

		this.getAllRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.post(listRequestsUrl, requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging));
		};


	}
})();