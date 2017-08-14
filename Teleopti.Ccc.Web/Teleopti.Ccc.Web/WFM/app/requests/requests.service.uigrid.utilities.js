﻿'use strict';

(function () {
	angular.module('wfm.requests').service('UIGridUtilitiesService', ['$filter', 'requestsDefinitions', 'requestCommandParamsHolder', uiGridUtilitiesService]);

	function uiGridUtilitiesService($filter, requestsDefinitions, requestCommandParamsHolder) {
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
						if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
							typeText += ' (' + row.Payload.Name + ')';
						}
						return typeText;
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

		svc.getDefaultStatus = function (filters, requestFiltersMgr) {
			var selectedRequestStatuses = [];
			if (filters && filters.length > 0) {
				var defaultStatusFilter = filters[0].Status;
				requestFiltersMgr.SetFilter('status', defaultStatusFilter);
				angular.forEach(defaultStatusFilter.split(' '),
					function(value) {
						if (value.trim() !== '') {
							selectedRequestStatuses.push({ Id: value.trim() });
						}
					});
			}
			return selectedRequestStatuses;
		}

		return svc;
	}
}());
