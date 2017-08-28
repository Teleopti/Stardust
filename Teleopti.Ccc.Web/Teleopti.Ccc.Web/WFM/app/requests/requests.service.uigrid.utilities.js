﻿'use strict';

(function () {
	angular.module('wfm.requests').service('UIGridUtilitiesService', ['$filter', '$translate', 'requestsDefinitions', 'requestCommandParamsHolder', 'RequestsFilter', uiGridUtilitiesService]);

	function uiGridUtilitiesService($filter, $translate, requestsDefinitions, requestCommandParamsHolder, requestFiltersMgr) {
		var svc = this;

		svc.formatDateTime = function (dateTime,
			requestTimezone,
			userTimeZone,
			displayDateOnly,
			isUsingRequestSubmitterTimeZone) {
			var angularTimezone = moment.tz(dateTime, isUsingRequestSubmitterTimeZone ? requestTimezone : userTimeZone)
				.format('Z');
			angularTimezone = angularTimezone.replace(':', '');
			var targetDateTime = moment.tz(dateTime, requestTimezone).toDate();
			if (displayDateOnly && isUsingRequestSubmitterTimeZone) {
				return $filter('date')(targetDateTime, 'shortDate', angularTimezone);
			} else {
				return $filter('date')(targetDateTime, 'short', angularTimezone);
			}
		};

		svc.formatToTimespan = function (lengthInSecond, isFullDay) {
			var durationData = moment.duration(lengthInSecond, "seconds")._data;
			var days = durationData.days;
			var hours = durationData.hours;
			var minutes = durationData.minutes === 0 ? "00" : durationData.minutes;
			var totalHours = hours + days * 24 === 0 ? "00" : hours + days * 24;

			return isFullDay ? (totalHours + 1 + ":" + "00") : (totalHours + ":" + minutes);
		};

		svc.prepareComputedColumns = function (requests, userTimeZone, isUsingRequestSubmitterTimeZone) {
			angular.forEach(requests,
				function (row) {
					row.GetDuration = function () {
						var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
						return svc.formatToTimespan(length, row.IsFullDay);
					};
					row.FormatedPeriodStartTime = function () {
						return svc.formatDateTime(row.PeriodStartTime,
							row.TimeZone,
							userTimeZone,
							row.IsFullDay,
							isUsingRequestSubmitterTimeZone);
					};
					row.FormatedPeriodEndTime = function () {
						return svc.formatDateTime(row.PeriodEndTime,
							row.TimeZone,
							userTimeZone,
							row.IsFullDay,
							isUsingRequestSubmitterTimeZone);
					};

					row.FormatedCreatedTime = function () {
						return svc.formatDateTime(row.CreatedTime,
							row.TimeZone,
							userTimeZone,
							false,
							isUsingRequestSubmitterTimeZone);
					};

					row.FormatedUpdatedTime = function () {
						return svc.formatDateTime(row.UpdatedTime,
							row.TimeZone,
							userTimeZone,
							false,
							isUsingRequestSubmitterTimeZone);
					};

					row.GetType = function () {
						var typeText = row.TypeText;
						if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE || row.Type == requestsDefinitions.REQUEST_TYPES.OVERTIME) {
							typeText += ' (' + row.Payload.Name + ')';
						}
						return typeText;
					};

					row.GetBrokenRules = function() {
						var translatedBrokenRules = new Array();
						var brokenRules = row.BrokenRules;
						if (brokenRules) {
							angular.forEach(brokenRules,
								function(value) {
									translatedBrokenRules.push($translate.instant(value));
								});
							return translatedBrokenRules.join(', ');
						}
						return brokenRules;
					};

				});
		};

		svc.setShiftTradeSelectedRequestIds = function (visibleSelectedRequestsIds, visibleRequestsIds, messages) {
			if (visibleSelectedRequestsIds.length === 1) {
				requestCommandParamsHolder.setSelectedIdAndMessage(visibleSelectedRequestsIds, messages);
			}

			requestCommandParamsHolder.setSelectedRequestIds(setAllSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, true), true);
		};

		svc.setAbsenceAndTextSelectedRequestIds = function(visibleSelectedRequestsIds, visibleRequestsIds, messages) {
			if (visibleSelectedRequestsIds.length === 1) {
				requestCommandParamsHolder.setSelectedIdAndMessage(visibleSelectedRequestsIds, messages);
			}

			requestCommandParamsHolder.setSelectedRequestIds(setAllSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, false), false);
		};

		svc.getDefaultStatus = function (filters, tabName) {
			var selectedRequestStatuses = [];
			if (filters && filters.length > 0) {
				if(!filters.filter(function(f){return Object.keys(f)[0] == 'Status';})[0]) return;

				var defaultStatusFilter = filters.filter(function(f){return Object.keys(f)[0] == 'Status';})[0].Status;
				requestFiltersMgr.setFilter('Status', defaultStatusFilter, tabName);
				angular.forEach(defaultStatusFilter.split(' '),
					function(value) {
						if (value.trim() !== '') {
							selectedRequestStatuses.push({ Id: value.trim() });
						}
					});
			}
			return selectedRequestStatuses;
		};

		function setAllSelectedRequestIds(visibleSelectedRequestsIds, visibleRequestsIds, isShiftTradeView) {
			var visibleNotSelectedRequestsIds = visibleRequestsIds.filter(function (id) {
				return visibleSelectedRequestsIds.indexOf(id) < 0;
			});

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds(isShiftTradeView);
			var newAllSelectedRequestsIds = [];

			angular.forEach(allSelectedRequestsIds, function (id) {
				if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsIds.push(id);
			});

			angular.forEach(visibleSelectedRequestsIds, function (id) {
				if (newAllSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsIds.push(id);
			});

			return newAllSelectedRequestsIds;
		}

		return svc;
	}
}());
