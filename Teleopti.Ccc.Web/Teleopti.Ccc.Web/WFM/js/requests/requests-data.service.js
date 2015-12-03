(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDataService', ['$http', 'requestsDefinitions', requestsDataService]);
		

	function requestsDataService($http, requestsDefinitions) {
		var loadTextAndAbsenceRequestsUrl = '../api/Requests/loadTextAndAbsenceRequests';

		this.getAllRequestsPromise = function(filter) {			
			return $http.post(loadTextAndAbsenceRequestsUrl, requestsDefinitions.normalizeRequestsFilter(filter));
		};

	}
})();