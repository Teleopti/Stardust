(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDataService', ['$http', 'requestsDefinitions', requestsDataService]);
		

	function requestsDataService($http, requestsDefinitions) {
		var loadTextAndAbsenceRequestsUrl_old = '../api/Requests/loadTextAndAbsenceRequests';
		var listRequestsUrl = '../api/Requests/requests';
		var approveRequestsUrl = '../api/Requests/requests/approve';

		this.approveRequests = function (selectedRequestIds, successCb, errorCb) {
			
			return $http.post(approveRequestsUrl, selectedRequestIds).success(function () {
				
			}).error(function(e) {
				
			});
		};

		this.getAllRequestsPromise_old = function(filter, sortingOrders) {			
			return $http.post(loadTextAndAbsenceRequestsUrl_old, requestsDefinitions.normalizeRequestsFilter_old(filter, sortingOrders));
		};

		this.getAllRequestsPromise = function(filter, sortingOrders, paging) {			
			return $http.get(listRequestsUrl,
				{ params: requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging) }
			);
		};

	}
})();