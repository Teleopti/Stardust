(function () {
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
		var approveWithValidatorsUrl = '../api/Requests/approveWithValidators';
		var replyRequestsUrl = '../api/Requests/replyRequests';
		var getSitesUrl = '../api/sites';
		var maintainOpenHoursUrl = '../api/Sites/MaintainOpenHours';
		var getLastCaluclatedDateUrl = '../GetLastCaluclatedDateTime';
		var resourceCalculateUrl = '../TriggerResourceCalculate';

		this.getAllRequestsPromise_old = function (filter, sortingOrders) {
			return $http.post(loadTextAndAbsenceRequestsUrl_old, requestsDefinitions.normalizeRequestsFilter_old(filter, sortingOrders));
		};

		this.getAllRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.get(listRequestsUrl,
				{ params: requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging) }
			);
		};

		this.getSitesPromise = function() {
			return $http.get(getSitesUrl);
		};

		this.maintainOpenHoursPromise = function (sites) {
			return $http.post(maintainOpenHoursUrl, sites);
		};

		this.getShiftTradeRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.get(listShiftTradeRequestsUrl,
				{ params: requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging) }
			);
		};

		this.replyRequestsPromise = function (selectedRequestIdsAndMessage) {
			return $http.post(replyRequestsUrl, selectedRequestIdsAndMessage);
		}

		this.approveRequestsPromise = function (selectedRequestIdsAndMessage) {
			return $http.post(approveRequestsUrl, selectedRequestIdsAndMessage);
		};

		this.cancelRequestsPromise = function (selectedRequestIdsAndMessage) {

			return $http.post(cancelRequestsUrl, selectedRequestIdsAndMessage);
		};

		this.processWaitlistRequestsPromise = function (waitlistPeriod, commandId) {
			var waitlistPeriodGet = {
				startTime: waitlistPeriod.startDate,
				endTime: waitlistPeriod.endDate,
				commandId: commandId
			}
			return $http.get(processWaitlistRequests, { params: waitlistPeriodGet });
		};

		this.approveWithValidatorsPromise = function (parameters) {
			return $http.post(approveWithValidatorsUrl, parameters);
		};

		this.denyRequestsPromise = function (selectedRequestIdsAndMessage) {
			return $http.post(denyRequestsUrl, selectedRequestIdsAndMessage);
		}

		this.getRequestableAbsences = function () {
			return $http.get(requestableAbsenceUrl);
		}

		this.getLastCaluclatedDateTime = function() {
			return $http.get(getLastCaluclatedDateUrl);
		}
		this.triggerResourceCalculate = function() {
			return $http.get(resourceCalculateUrl);
		}

		this.getAllRequestStatuses = function (isShiftTradeView) {
			// TODO: Should get this list in a better way
			// Refer to definition of Teleopti.Ccc.Domain.AgentInfo.Requests.PersonRequest.personRequestState.CreateFromId()
			var statuses = [
				{ Id: 0, Name: $translate.instant("Pending") },
				{ Id: 1, Name: $translate.instant("Denied") },
				{ Id: 2, Name: $translate.instant("Approved") }
			];
			if (!isShiftTradeView) {
				statuses.push(
					{ Id: 5, Name: $translate.instant("Waitlisted") },
					{ Id: 6, Name: $translate.instant("Cancelled") }
				);
			}

			return statuses;
		}

		this.getAllBusinessRulesForApproving = function () {
			return [
				{
					Id: requestsDefinitions.REQUEST_VALIDATORS.BudgetAllotmentValidator,
					Checked: false,
					Name: "BudgetAllotmentValidator",
					Description: $translate.instant("ValidateRequestsBasedOnBudgetAllotment")
				}, {
					Id: requestsDefinitions.REQUEST_VALIDATORS.IntradayValidator,
					Checked: false,
					Name: "IntradayValidator",
					Description: $translate.instant("ValidateRequestsBasedOnIntraday")
				}
			];
		}
	}
})();