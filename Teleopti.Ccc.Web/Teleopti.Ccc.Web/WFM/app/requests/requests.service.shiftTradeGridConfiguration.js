'use strict';

(function() {
	angular
		.module('wfm.requests')
		.factory('ShiftTradeGridConfiguration', [
			'$filter',
			'$translate',
			'requestsDefinitions',
			'CurrentUserInfo',
			ShiftTradeGridConfiguration
		]);

	function ShiftTradeGridConfiguration($filter, $translate, requestDefinitions, currentUserInfo) {
		var columns = [];
		var service = {
			columnDefinitions: columnDefinitions,
			getDayViewModels: getDayViewModels,
			getShiftTradeScheduleViewModels: getShiftTradeScheduleViewModels
		};

		function getShiftTradeColumnLeftOffset(startMoment, currentMoment) {
			return currentMoment.diff(startMoment, 'days') * requestDefinitions.SHIFTTRADE_COLUMN_WIDTH + 'px';
		}

		function getDayViewModels(requests, shiftTradeRequestDateSummary) {
			if (angular.isUndefined(requests) || requests == null || requests.length === 0) {
				return [];
			}

			var day = moment(shiftTradeRequestDateSummary.Minimum).add(-1, 'days');
			var maxDay = moment(shiftTradeRequestDateSummary.Maximum);
			var startOfWeekIsoDay = shiftTradeRequestDateSummary.FirstDayOfWeek;

			var dayViewModels = [];
			var dayIncrement = day.clone();
			while (dayIncrement <= maxDay) {
				var dayViewModel = createDayViewModel(
					dayIncrement,
					startOfWeekIsoDay,
					true, //don't need to convert time zone for title area
					currentUserInfo.CurrentUserInfo().DefaultTimeZone
				);
				dayViewModel.leftOffset = getShiftTradeColumnLeftOffset(day, dayIncrement);
				dayViewModels.push(dayViewModel);
				dayIncrement.add(1, 'days');
			}

			// Do not display date if the last day is start of week (To fix bug #39874)
			dayViewModels[dayViewModels.length - 1].isLatestDayOfPeriod = true;

			return dayViewModels;
		}

		function isStartOfWeek(day, startOfWeekIsoDayNumber) {
			var difference = day.isoWeekday() - startOfWeekIsoDayNumber;
			return difference === 0;
		}

		function createDayViewModel(day, startOfWeekIsoDay, isUsingRequestSubmitterTimezone, submitterTimezone) {
			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var originalDate = new Date(moment(day).format("YYYY-M-DD"));
			var targetTimezone = isUsingRequestSubmitterTimezone ? submitterTimezone : currentUserTimezone;
			day = convertTimezone(day, submitterTimezone, isUsingRequestSubmitterTimezone);

			var isWeekend = day.isoWeekday() === 6 || day.isoWeekday() === 7;

			return {
				originalDate: originalDate,
				targetTimezone: targetTimezone,
				date: new Date(moment(day).format("YYYY-M-DD")),
				shortDate: $filter('date')(new Date(moment(day).format("YYYY-M-DD")), 'shortDate'),
				dayNumber: $filter('date')(new Date(moment(day).format("YYYY-M-DD")), 'dd'),
				isWeekend: isWeekend,
				isStartOfWeek: isStartOfWeek(day.clone(), startOfWeekIsoDay),
				isLatestDayOfPeriod: false
			};
		}

		function convertTimezone(dateTimeWithSubmitterTimezone, submitterTimezone, isUsingRequestSubmitterTimezone) {
			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			if (isNeedConvertTimeZone(isUsingRequestSubmitterTimezone, submitterTimezone)) {
				var dateTimeWithTimezone = dateTimeWithSubmitterTimezone.tz(currentUserTimezone);
				return moment(dateTimeWithTimezone.format('YYYY-MM-DD'));
			}
			return dateTimeWithSubmitterTimezone;
		}

		function isNeedConvertTimeZone(isUsingRequestSubmitterTimezone, submitterTimezone) {
			var currentUserTimezone = currentUserInfo.CurrentUserInfo().DefaultTimeZone;
			if (!isUsingRequestSubmitterTimezone && submitterTimezone !== currentUserTimezone) return true;
			return false;
		}

		function isDayOff(scheduleDayDetail) {
			return scheduleDayDetail && scheduleDayDetail.Type === requestDefinitions.SHIFT_OBJECT_TYPE.DayOff;
		}

		function createShiftTradeDayViewModel(
			shiftTradeDay,
			shiftTradeRequestDateSummary,
			isUsingRequestSubmitterTimezone,
			submitterTimezone
		) {
			var startDate = moment(shiftTradeRequestDateSummary.Minimum).add(-1, 'days');
			var startOfWeekIsoDay = shiftTradeRequestDateSummary.FirstDayOfWeek;
			var shiftTradeDate = moment.tz(shiftTradeDay.Date, submitterTimezone);

			var viewModel = createDayViewModel(
				shiftTradeDate,
				startOfWeekIsoDay,
				isUsingRequestSubmitterTimezone,
				submitterTimezone
			);
			viewModel.FromScheduleDayDetail = {};
			viewModel.ToScheduleDayDetail = {};

			angular.copy(shiftTradeDay.FromScheduleDayDetail, viewModel.FromScheduleDayDetail);
			angular.copy(shiftTradeDay.ToScheduleDayDetail, viewModel.ToScheduleDayDetail);

			viewModel.FromScheduleDayDetail.IsDayOff = isDayOff(shiftTradeDay.FromScheduleDayDetail);
			viewModel.ToScheduleDayDetail.IsDayOff = isDayOff(shiftTradeDay.ToScheduleDayDetail);
			viewModel.LeftOffset = getShiftTradeColumnLeftOffset(startDate, moment(viewModel.date));

			return viewModel;
		}

		function getShiftTradeScheduleViewModels(
			requests,
			shiftTradeRequestDateSummary,
			isUsingRequestSubmitterTimezone
		) {
			var shiftTradeDataForDisplay = {};

			angular.forEach(requests, function(request) {
				var viewModelArray = [];

				angular.forEach(request.ShiftTradeDays, function(shiftTradeDay) {
					if (
						shiftTradeDay.FromScheduleDayDetail.ShortName !== null ||
						shiftTradeDay.ToScheduleDayDetail.ShortName !== null
					) {
						var viewModel = createShiftTradeDayViewModel(
							shiftTradeDay,
							shiftTradeRequestDateSummary,
							isUsingRequestSubmitterTimezone,
							request.TimeZone
						);
						viewModelArray.push(viewModel);
					}
				});

				shiftTradeDataForDisplay[request.Id] = viewModelArray;
			});

			return shiftTradeDataForDisplay;
		}

		function setupStandardColumns() {
			columns = [
				{
					displayName: $translate.instant('StartTime'),
					field: 'FormatedPeriodStartTime()',
					cellClass: 'request-period-start-time',
					headerCellClass: 'request-period-start-time-header',
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('EndTime'),
					field: 'FormatedPeriodEndTime()',
					cellClass: 'request-period-end-time',
					headerCellClass: 'request-period-end-time-header',
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('Duration'),
					field: 'GetDuration()',
					cellClass: 'request-period-duration',
					enableSorting: false,
					visible: false,
					headerCellClass: 'request-period-duration-header',
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('AgentName'),
					field: 'AgentName',
					cellTemplate:
						'<table style="width: 100%; height: 100%;"><tr><td> {{row.entity[col.field]}}</td></tr><tr><td> {{row.entity["PersonTo"]}}</td></tr></table>',
					headerCellClass: 'request-agent-name-header',
					enableSorting: false,
					pinnedLeft: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('TimeZone'),
					field: 'TimeZone',
					cellClass: 'request-time-zone',
					headerCellClass: 'request-time-zone-header',
					enableSorting: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					visible: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('Team'),
					field: 'Team',
					cellClass: 'request-team',
					headerCellClass: 'request-team-header',
					cellTemplate:
						'<table style="width: 100%; height: 100%;"><tr><td> {{row.entity[col.field]}}</td></tr><tr><td> {{row.entity["PersonToTeam"]}}</td></tr></table>',
					enableSorting: false,
					pinnedLeft: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('Seniority'),
					field: 'Seniority',
					cellClass: 'request-seniority',
					headerCellClass: 'request-seniority-header',
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('Subject'),
					field: 'Subject',
					cellClass: 'request-subject',
					headerCellClass: 'request-subject-header',
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: true,
					minWidth: 100,
					cellTooltip: true,
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters" > <input ng-enter="enter()" ' +
						'type="text" ng-model="grid.appScope.subjectFilter" ' +
						'ng-model-options="{ debounce: 500 }" ng-change="grid.appScope.subjectFilterChanged()" ' +
						'ng-attr-placeholder="{{colFilter.placeholder || \'\'}}" aria-label="{{colFilter.ariaLabel || aria.defaultFilterLabel}}" /></div>'
				},
				{
					displayName: $translate.instant('Message'),
					field: 'Message',
					cellClass: 'request-message',
					headerCellClass: 'request-message-header',
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: true,
					minWidth: 100,
					cellTooltip: true,
					filter: {
						disableCancelFilterButton: true,
						placeholder: $translate.instant('FilterThreeDots')
					},
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters"> <input ng-enter="enter()" ' +
						'type="text" ng-model="grid.appScope.messageFilter" n' +
						'g-model-options="{ debounce: 500 }" ng-change="grid.appScope.messageFilterChanged()" ' +
						'ng-attr-placeholder="{{colFilter.placeholder || \'\'}}" aria-label="{{colFilter.ariaLabel || aria.defaultFilterLabel}}" /></div>'
				},
				{
					displayName: $translate.instant('DenyReason'),
					field: 'DenyReason',
					cellClass: 'request-deny-reason',
					headerCellClass: 'request-deny-reason-header',
					visible: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100,
					cellTooltip: true
				},
				{
					displayName: $translate.instant('Status'),
					field: 'StatusText',
					cellClass: 'request-status',
					headerCellClass: 'request-status-header',
					visible: true,
					enableSorting: false,
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: true,
					minWidth: 100,
					filterHeaderTemplate:
						'<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters">' +
						'<md-select ng-model-options="{trackBy: \'$value.Id\'}" md-on-close="grid.appScope.statusFilterClose()"' +
						'multiple ng-model="grid.appScope.selectedRequestStatuses" placeholder="{{\'FilterColon\' | translate}} {{\'Status\' | translate}}" aria-label="{{\'Status\' | translate}}">' +
						'<md-option ng-repeat="item in grid.appScope.allRequestStatuses" ng-value="item">' +
						'<span>{{item.Name | translate}}</span>' +
						'</md-option>' +
						'</md-select>' +
						'</div>'
				},
				{
					displayName: $translate.instant('CreatedOn'),
					field: 'FormatedCreatedTime()',
					cellClass: 'request-created-time',
					headerCellClass: 'request-created-time-header',
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				},
				{
					displayName: $translate.instant('UpdatedOn'),
					field: 'FormatedUpdatedTime()',
					cellClass: 'request-updated-time',
					visible: false,
					headerCellClass: 'request-updated-time-header',
					pinnedRight: true,
					enablePinning: false,
					enableFiltering: false,
					minWidth: 100
				}
			];
		}

		function setupShiftTradeVisualisationColumn(shiftTradeRequestDateSummary) {
			var minimum = moment(shiftTradeRequestDateSummary.Minimum);
			var maximum = moment(shiftTradeRequestDateSummary.Maximum);
			var numberOfDays = maximum.diff(minimum, 'days') + 2;

			return {
				displayName: $translate.instant('ShiftTrade'),
				field: 'AgentName',
				enablePinning: false,
				enableColumnMenu: false,
				enableHiding: false,
				cellTemplate: 'app/requests/html/shift-trade-day-template.html',
				width: numberOfDays * requestDefinitions.SHIFTTRADE_COLUMN_WIDTH,
				enableSorting: false,
				enableFiltering: false,
				isShiftTradeDayColumn: true
			};
		}

		function columnDefinitions(shiftTradeRequestDateSummary) {
			if (columns.length === 0) {
				setupStandardColumns();

				var brokenRulesColumn = {
					displayName: $translate.instant('BrokenRules'),
					field: 'GetBrokenRules()',
					cellClass: 'request-broken-rules',
					cellTooltip: true,
					visible: true,
					pinnedRight: true,
					enablePinning: false,
					enableSorting: false,
					enableFiltering: false,
					minWidth: 100
				};
				columns.splice(10, 0, brokenRulesColumn);
			}

			if (shiftTradeRequestDateSummary) {
				var shifTradeVisualisationColumn = setupShiftTradeVisualisationColumn(shiftTradeRequestDateSummary);
				return columns.concat([shifTradeVisualisationColumn]);
			}

			// since upgrading to ui-grid 3.2.6, require copy of columns array (concat above already does copy)
			return angular.copy(columns);
		}

		return service;
	}
})();
