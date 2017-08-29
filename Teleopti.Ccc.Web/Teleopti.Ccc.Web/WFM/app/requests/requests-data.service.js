(function () {
	'use strict';

	angular.module('wfm.requests').service('requestsDataService', ['$q', '$http', '$translate', 'requestsDefinitions', 'Toggle', 'REQUESTS_TAB_NAMES', requestsDataService]);

	function requestsDataService($q, $http, $translate, requestsDefinitions, toggleSvc, REQUESTS_TAB_NAMES) {
		var listAbsenceAndTextRequestsUrl = '../api/Requests/requests';
		var listOvertimeRequestsUrl = '../api/Requests/overtimeRequests';
		var listShiftTradeRequestsUrl = '../api/Requests/shiftTradeRequests';
		var approveRequestsUrl = '../api/Requests/approveRequests';
		var denyRequestsUrl = '../api/Requests/denyRequests';
		var cancelRequestsUrl = '../api/Requests/cancelRequests';
		var requestableAbsenceUrl = '../api/Absence/GetRequestableAbsences';
		var processWaitlistRequests = '../api/Requests/runWaitlist';
		var approveWithValidatorsUrl = '../api/Requests/approveWithValidators';
		var replyRequestsUrl = '../api/Requests/replyRequests';
		var getSitesUrl = '../api/Requests/SitesWithOpenHour';
		var maintainOpenHoursUrl = '../api/Sites/MaintainOpenHours';
		var getLastCaluclatedDateUrl = '../GetLastCaluclatedDateTime';
		var resourceCalculateUrl = '../TriggerResourceCalculate';
		var getBudgetGroupsUrl = "../api/RequestAllowance/budgetGroups";
		var getBudgetAllowanceUrl = "../api/RequestAllowance/allowances";
		var overtimeTypesUrl = "../api/MultiplicatorDefinitionSet/Overtime";
		var hierarchyUrl;

		if (toggleSvc.Wfm_HideUnusedTeamsAndSites_42690) {
			hierarchyUrl = '../api/Requests/GetOrganizationWithPeriod';
		} else {
			hierarchyUrl = '../api/Requests/FetchPermittedTeamHierachy';
		}

		this.getAllRequestsPromise = function (filter, sortingOrders, paging) {
			return $q(function (resolve, reject) {
				var requestFilter = requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging);
				resolve($http.post(listAbsenceAndTextRequestsUrl, requestFilter));
			});
		};

		this.getSitesPromise = function () {s
			return $http.get(getSitesUrl);
		};

		this.maintainOpenHoursPromise = function (sites) {
			return $http.post(maintainOpenHoursUrl, sites);
		};

		this.getShiftTradeRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.post(listShiftTradeRequestsUrl, requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging));
		};

		this.getOvertimeRequestsPromise = function (filter, sortingOrders, paging) {
			return $http.post(listOvertimeRequestsUrl, requestsDefinitions.normalizeRequestsFilter(filter, sortingOrders, paging));
		};

		this.getBudgetGroupsPromise = function () {
			return $http.get(getBudgetGroupsUrl);
		};

		this.getBudgetAllowancePromise = function (date, budgetGroupId) {
			return $http.get(getBudgetAllowanceUrl,
				{
					params: {
						date: date,
						budgetGroupId: budgetGroupId
					}
				});
		};

		this.replyRequestsPromise = function (selectedRequestIdsAndMessage) {
			return $http.post(replyRequestsUrl, selectedRequestIdsAndMessage);
		};

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
			};
			return $http.get(processWaitlistRequests, { params: waitlistPeriodGet });
		};

		this.approveWithValidatorsPromise = function (parameters) {
			return $http.post(approveWithValidatorsUrl, parameters);
		};

		this.denyRequestsPromise = function(selectedRequestIdsAndMessage) {
			return $http.post(denyRequestsUrl, selectedRequestIdsAndMessage);
		};

		this.getRequestTypes = function() {
			return $http.get(requestableAbsenceUrl).then(function(result) {
				result.data.unshift({ Id: '0', Name: 'Text', ShortName: '' });
				return result;
			});
		};

		this.getOvertimeTypes  = function () {
			return $http.get(overtimeTypesUrl).then(function (result) {
				return result;
			});
		};

		this.getLastCaluclatedDateTime = function() {
			return $http.get(getLastCaluclatedDateUrl);
		};

		this.triggerResourceCalculate = function() {
			return $http.get(resourceCalculateUrl);
		};

		this.getAllRequestStatuses = function (tabName) {
			// TODO: Should get this list in a better way
			// Refer to definition of Teleopti.Ccc.Domain.AgentInfo.Requests.PersonRequest.personRequestState.CreateFromId()
			var basicStatues = [
				{ Id: 0, Name: $translate.instant("Pending") },
				{ Id: 1, Name: $translate.instant("Denied") },
				{ Id: 2, Name: $translate.instant("Approved") }
			];

			if(tabName.indexOf(REQUESTS_TAB_NAMES.absenceAndText) > -1){
				var statues = [
					{ Id: 5, Name: $translate.instant("Waitlisted") },
					{ Id: 6, Name: $translate.instant("Cancelled") }
				];

				return basicStatues.concat(statues);
			}

			if (tabName.indexOf(REQUESTS_TAB_NAMES.shiftTrade) > -1) {
				return basicStatues;
			}

			if (tabName.indexOf(REQUESTS_TAB_NAMES.overtime) > -1) {
				return basicStatues;
			}
		};

		this.getAllBusinessRulesForApproving = function () {
			return [
				{
					Id: requestsDefinitions.REQUEST_VALIDATORS.BudgetAllotmentValidator,
					Checked: false,
					Name: "BudgetAllotmentValidator",
					Description: $translate.instant("ValidateRequestsBasedOnBudgetAllotment"),
					Enabled: toggleSvc.Wfm_Requests_Approve_Based_On_Budget_Allotment_39626
				},
				{
					Id: requestsDefinitions.REQUEST_VALIDATORS.IntradayValidator,
					Checked: false,
					Name: "IntradayValidator",
					Description: $translate.instant("ValidateRequestsBasedOnIntraday"),
					Enabled: toggleSvc.Wfm_Requests_Approve_Based_On_Intraday_39868
				},
				{
					Id: requestsDefinitions.REQUEST_VALIDATORS.ExpirationValidator,
					Checked: false,
					Name: "ExpirationValidator",
					Description: $translate.instant("ValidateRequestsBasedOnExpiration"),
					Enabled: toggleSvc.Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274
				}
			];
		};

		this.hierarchy = function (params) {
			if (!params) {
				return;
			}
			return $q(function (resolve, reject) {
				$http.get(hierarchyUrl, { params: params })
					.then(function (response) {
						resolve(response.data);
					}, function (response) {
						reject(response.data);
					});
			});
		};
	}
})();