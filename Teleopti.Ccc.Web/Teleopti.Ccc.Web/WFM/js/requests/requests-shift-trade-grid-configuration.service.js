'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('ShiftTradeGridConfiguration', ['$filter', 'Toggle', 'requestsDefinitions', function ($filter, toggleSvc, requestDefinitions) {

			var columns = [];

			var service = {
				columnDefinitions: columnDefinitions,
				getDayViewModels: getDayViewModels,
				getShiftTradeScheduleViewModels: getShiftTradeScheduleViewModels
			}

			function getShiftTradeColumnLeftOffset(startMoment, currentMoment) {
				return (currentMoment.diff(startMoment, 'days') * requestDefinitions.SHIFTTRADE_COLUMN_WIDTH) + "px";
			}

			function getDayViewModels(requests, shiftTradeRequestDateSummary) {
				var day = moment(shiftTradeRequestDateSummary.Minimum);
				var maxDay = moment(shiftTradeRequestDateSummary.Maximum);
				var startOfWeekIsoDay = shiftTradeRequestDateSummary.FirstDayOfWeek;

				var dayViewModels = [];
				var dayIncrement = day.clone();
				while (dayIncrement <= maxDay) {
					var dayViewModel = createDayViewModel(dayIncrement, startOfWeekIsoDay);
					dayViewModel.leftOffset = getShiftTradeColumnLeftOffset(day, dayIncrement);
					dayViewModels.push(dayViewModel);
					dayIncrement.add(1, 'days');

				}

				return dayViewModels;
			}

			function isStartOfWeek(day, startOfWeekIsoDayNumber) {
				var difference = ((day.isoWeekday()) - startOfWeekIsoDayNumber);
				return difference === 0;
			};

			function createDayViewModel(day, startOfWeekIsoDay) {
				var isWeekend = (day.isoWeekday() === 6 || day.isoWeekday() === 7);

				return {
					date: day.toDate(),
					shortDate: $filter('date')(day.toDate(), 'shortDate'),
					dayNumber: $filter('date')(day.toDate(), 'dd'),
					isWeekend: isWeekend,
					isStartOfWeek: isStartOfWeek(day.clone(), startOfWeekIsoDay)
				};
			};

			function isDayOff(scheduleDayDetail) {
				return (scheduleDayDetail && (scheduleDayDetail.Type === requestDefinitions.SHIFT_OBJECT_TYPE.DayOff));
			}


			function createShiftTradeDayViewModel(shiftTradeDay, shiftTradeRequestDateSummary) {

				var startDate = moment(shiftTradeRequestDateSummary.Minimum);
				var startOfWeekIsoDay = shiftTradeRequestDateSummary.FirstDayOfWeek;
				var shiftTradeDate = moment(shiftTradeDay.Date);

				var viewModel = createDayViewModel(shiftTradeDate, startOfWeekIsoDay);

				viewModel.FromScheduleDayDetail = {};
				viewModel.ToScheduleDayDetail = {}

				angular.copy(shiftTradeDay.FromScheduleDayDetail, viewModel.FromScheduleDayDetail);
				angular.copy(shiftTradeDay.ToScheduleDayDetail, viewModel.ToScheduleDayDetail);

				viewModel.FromScheduleDayDetail.IsDayOff = isDayOff(shiftTradeDay.FromScheduleDayDetail);
				viewModel.ToScheduleDayDetail.IsDayOff = isDayOff(shiftTradeDay.ToScheduleDayDetail);
				viewModel.LeftOffset = getShiftTradeColumnLeftOffset(startDate, shiftTradeDate);

				return viewModel;

			}

			function getShiftTradeScheduleViewModels(requests, shiftTradeRequestDateSummary) {

				var shiftTradeDataForDisplay = {};

				angular.forEach(requests, function (request) {
					var viewModelArray = [];

					angular.forEach(request.ShiftTradeDays, function (shiftTradeDay) {
						if (shiftTradeDay.FromScheduleDayDetail.ShortName != null && shiftTradeDay.ToScheduleDayDetail.ShortName != null) {
							var viewModel = createShiftTradeDayViewModel(shiftTradeDay, shiftTradeRequestDateSummary);
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
						displayName: 'StartTime',
						field: 'FormatedPeriodStartTime()',
						headerCellFilter: 'translate',
						cellClass: 'request-period-start-time',
						headerCellClass: 'request-period-start-time-header',
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'EndTime',
						field: 'FormatedPeriodEndTime()',
						headerCellFilter: 'translate',
						cellClass: 'request-period-end-time',
						headerCellClass: 'request-period-end-time-header',
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'Duration',
						field: 'GetDuration()',
						headerCellFilter: 'translate',
						cellClass: 'request-period-duration',
						enableSorting: false,
						visible: false,
						headerCellClass: 'request-period-duration-header',
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'AgentName',
						field: 'AgentName',
						headerCellFilter: 'translate',
						//cellClass: 'request-agent-name',
						cellTemplate: '<table style="width: 100%; height: 100%;"><tr><td> {{row.entity[col.field]}}</td></tr><tr><td> {{row.entity["PersonTo"]}}</td></tr></table>',
						headerCellClass: 'request-agent-name-header',
						enableSorting: false,
						pinnedLeft: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'TimeZone',
						field: 'TimeZone',
						headerCellFilter: 'translate',
						cellClass: 'request-time-zone',
						headerCellClass: 'request-time-zone-header',
						enableSorting: false,
						pinnedRight: true,
						enablePinning: false,
						visible: false,
						minWidth: 111
					},
					{
						displayName: 'Team',
						field: 'Team',
						headerCellFilter: 'translate',
						cellClass: 'request-team',
						headerCellClass: 'request-team-header',
						cellTemplate: '<table style="width: 100%; height: 100%;"><tr><td> {{row.entity[col.field]}}</td></tr><tr><td> {{row.entity["PersonToTeam"]}}</td></tr></table>',
						enableSorting: false,
						pinnedLeft: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'Seniority',
						field: 'Seniority',
						headerCellFilter: 'translate',
						cellClass: 'request-seniority',
						headerCellClass: 'request-seniority-header',
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'Subject',
						field: 'Subject',
						headerCellFilter: 'translate',
						cellClass: 'request-subject',
						headerCellClass: 'request-subject-header',
						filter: {
							disableCancelFilterButton: true,
							placeholder: 'Filter...'
						},
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111,
						filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" > <input ng-enter=\"enter()\" ' +
							'style=\"background-color:#FFF\" type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ' +
							'ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
					},
					{
						displayName: 'Message',
						field: 'Message',
						headerCellFilter: 'translate',
						cellClass: 'request-message',
						headerCellClass: 'request-message-header',
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111,
						filter: {
							disableCancelFilterButton: true,
							placeholder: 'Filter...'
						},
						filterHeaderTemplate: '<div class=\"ui-grid-filter-container\" ng-repeat=\"colFilter in col.filters\" > <input ng-enter=\"enter()\" ' +
							'type=\"text\" class=\"ui-grid-filter-input ui-grid-filter-input-{{$index}}\" ng-model=\"colFilter.term\" ' +
							'ng-attr-placeholder=\"{{colFilter.placeholder || \'\'}}\" aria-label=\"{{colFilter.ariaLabel || aria.defaultFilterLabel}}\" /></div>'
					},
					{
						displayName: 'DenyReason',
						field: 'DenyReason',
						headerCellFilter: 'translate',
						cellClass: 'request-deny-reason',
						headerCellClass: 'request-deny-reason-header',
						visible: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'Status',
						field: 'StatusText',
						headerCellFilter: 'translate',
						cellClass: 'request-status',
						headerCellClass: 'request-status-header',
						visible: true,
						enableSorting: false,
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111,
						filterHeaderTemplate: '<md-select ng-model-options=\"{trackBy: \'$value.Id\'}\" ng-repeat=\"colFilter in col.filters\" md-on-close=\"grid.appScope.statusFilterClose()\"'
							+ 'multiple ng-model=\"grid.appScope.SelectedRequestStatuses\" placeholder=\"{{\'FilterColon\' | translate}} {{\'Status\' | translate}}\" aria-label=\"{{\'Status\' | translate}}\">'
							+ '<md-option ng-repeat=\"item in grid.appScope.AllRequestStatuses\" ng-value=\"item\">'
							+ '<span ng-bind=\"item.Name\"></span>'
							+ '</md-option>'
							+ '</md-select>'
					},
					{
						displayName: 'CreatedOn',
						field: 'FormatedCreatedTime()',
						headerCellFilter: 'translate',
						cellClass: 'request-created-time',
						headerCellClass: 'request-created-time-header',
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'Id',
						field: 'Id',
						headerCellFilter: 'translate',
						cellClass: 'request-id',
						visible: false,
						headerCellClass: 'request-id-header',
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					},
					{
						displayName: 'UpdatedOn',
						field: 'FormatedUpdatedTime()',
						headerCellFilter: 'translate',
						cellClass: 'request-updated-time',
						visible: false,
						headerCellClass: 'request-updated-time-header',
						pinnedRight: true,
						enablePinning: false,
						minWidth: 111
					}
				];
			}

			function setupShiftTradeVisualisationColumn(shiftTradeRequestDateSummary) {

				var minimum = moment(shiftTradeRequestDateSummary.Minimum);
				var maximum = moment(shiftTradeRequestDateSummary.Maximum);
				var numberOfDays = maximum.diff(minimum, 'days') + 1;

				return {
					field: 'AgentName',
					enablePinning: false,
					enableColumnMenu: false,
					enableHiding: false,
					cellTemplate: 'shift-trade-day-template.html',
					width: numberOfDays * 40,
					enableSorting: false,
					enableFiltering: false,
					isShiftTradeDayColumn: true
				};
			}

			function columnDefinitions(shiftTradeRequestDateSummary) {

				if (columns.length === 0) {
					setupStandardColumns();

					if (toggleSvc.Wfm_Requests_Show_Pending_Reasons_39473) {
						var brokenRulesColumn = {
							displayName: 'BrokenRules',
							field: 'GetBrokenRules()',
							headerCellFilter: 'translate',
							cellClass: 'request-broken-rules',
							visible: true,
							pinnedRight: true,
							enablePinning: false,
							enableSorting: false,
							minWidth: 111
						};
						columns.splice(10, 0, brokenRulesColumn);
					}
				}

				if (shiftTradeRequestDateSummary) {
					var shifTradeVisualisationColumn = setupShiftTradeVisualisationColumn(shiftTradeRequestDateSummary);
					return columns.concat([shifTradeVisualisationColumn]);
				}
				
				return columns;
			}

			return service;

		}]);

}());
