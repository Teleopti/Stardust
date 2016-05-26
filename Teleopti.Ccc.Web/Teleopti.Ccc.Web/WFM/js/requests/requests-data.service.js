(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsDataService', ['$http', '$translate', 'requestsDefinitions', requestsDataService]);

	function requestsDataService($http, $translate, requestsDefinitions) {
		var loadTextAndAbsenceRequestsUrl_old = '../api/Requests/loadTextAndAbsenceRequests';
		var listRequestsUrl = '../api/Requests/requests';
		var listShiftTradeRequestsUrl = '../api/Requests/shiftTradeRequests';
		var approveRequestsUrl = '../api/Requests/approveRequests';
		var denyRequestsUrl = '../api/Requests/denyRequests';
		var cancelRequestsUrl = '../api/Requests/cancelRequests';
		var requestableAbsenceUrl = '../api/Absence/GetRequestableAbsences';
		var processWaitlistRequests = '../api/Requests/runWaitlist';

		this.getAllRequestsPromise_old = function(filter, sortingOrders) {
			return $http.post(loadTextAndAbsenceRequestsUrl_old, requestsDefinitions.normalizeRequestsFilter_old(filter, sortingOrders));
		};

		this.getAllRequestsPromise = function(filter, sortingOrders, paging) {
			return $http.get(listRequestsUrl,
				{ params: requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging) }
			);
		};

		this.getShiftTradeRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.get(listShiftTradeRequestsUrl,
				{ params: requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging) }
			);
		};

		this.approveRequestsPromise = function(selectedRequestIds) {
			return $http.post(approveRequestsUrl, selectedRequestIds);
		};

		this.cancelRequestsPromise = function(selectedRequestIds) {
			return $http.post(cancelRequestsUrl, selectedRequestIds);
		};

		this.processWaitlistRequestsPromise = function (waitlistPeriod,commandId) {
            var waitlistPeriodGet= {
                startTime: waitlistPeriod.startDate,
                endTime: waitlistPeriod.endDate,
                commandId:commandId
            }
            return $http.get(processWaitlistRequests, { params: waitlistPeriodGet });
		};

		this.denyRequestsPromise = function(selectedRequestIds) {
			return $http.post(denyRequestsUrl, selectedRequestIds);
		}

		this.getRequestableAbsences = function() {
			return $http.get(requestableAbsenceUrl);
		}

		this.getAllRequestStatuses = function() {
			// TODO: Should get this list in a better way
			// Refer to definition of Teleopti.Ccc.Domain.AgentInfo.Requests.PersonRequest.personRequestState.CreateFromId()
			return [
				{ Id: 0, Name: $translate.instant("Pending") },
				{ Id: 1, Name: $translate.instant("Denied") },
				{ Id: 2, Name: $translate.instant("Approved") },
				{ Id: 3, Name: $translate.instant("New") },
				{ Id: 4, Name: $translate.instant("AutoDenied") },
				{ Id: 5, Name: $translate.instant("Waitlisted") },
				{ Id: 6, Name: $translate.instant("Cancelled") }
			];
		}
	}
})();