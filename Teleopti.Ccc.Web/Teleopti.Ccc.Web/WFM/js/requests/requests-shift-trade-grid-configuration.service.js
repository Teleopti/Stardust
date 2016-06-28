'use strict';

(function () {

	angular.module('wfm.requests')
		.factory('ShiftTradeGridConfiguration', ['$filter', function ($filter) {

			var columns = [];

			var service = {
				columnDefinitions: columnDefinitions,
				categories: getCategories
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
						minWidth: 111,
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
						minWidth: 111,
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
						minWidth: 111,
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
						minWidth: 111,
					}
				];

			}

			function columnDefinitions(shiftTradeRequestDateSummary) {

				if (columns.length == 0) {
					setupStandardColumns();
				}

				if (shiftTradeRequestDateSummary) {
					return columns.concat(getShiftTradeVisualisationDayColumns(shiftTradeRequestDateSummary));
				}

				return columns;
			}

			function getStartOfWeek(day, startOfWeekIsoDayNumber) {

				var difference = ((day.isoWeekday()) - startOfWeekIsoDayNumber);

				if (difference < 0) {
					difference = 7 + difference;
				}

				return day.subtract(difference, 'days');
			};

			function getColumnForDay(day, startOfWeekIsoDay) {
				var isWeekend = (day.isoWeekday() === 6 || day.isoWeekday() === 7);
				var startOfWeek = getStartOfWeek(day.clone(), startOfWeekIsoDay);

				return {
					displayName: $filter('date')(day.toDate(), 'dd'),
					field: $filter('date')(day.toDate(), 'shortDate'),
					headerCellFilter: 'translate',
					enablePinning: false,
					enableColumnMenu: false,
					enableHiding: false,
					enableColumnResizing: false,
					category: $filter('date')(startOfWeek.toDate(), 'shortDate'),
					cellTemplate: 'shift-trade-day-template.html',
					headerCellClass: isWeekend ? 'shift-trade-header-weekend' : '',
					width: 37,
					enableSorting: false,
					enableFiltering: false,
					isShiftTradeDayColumn: true,
					isWeekend: isWeekend,
				}
			}

			function getShiftTradeVisualisationDayColumns(shiftTradeRequestDateSummary) {
				var day = moment(shiftTradeRequestDateSummary.Minimum);
				var maxDay = moment(shiftTradeRequestDateSummary.Maximum);
				var startOfWeekIsoDay = shiftTradeRequestDateSummary.FirstDayOfWeek; //day.isoWeekday();

				var columnArray = [];

				columnArray.push(getColumnForDay(day.clone(), startOfWeekIsoDay));

				while (day.add(1, 'days').diff(maxDay) <= 0) {
					columnArray.push(getColumnForDay(day.clone(), startOfWeekIsoDay));
				}


				return columnArray;
			}

			function getCategories(shiftTradeRequestDateSummary) {

				var categories = [];
				if (shiftTradeRequestDateSummary) {

					var maximum = shiftTradeRequestDateSummary.Maximum;
					var minimum = shiftTradeRequestDateSummary.Minimum;

					var day = moment(minimum);
					var startOfWeekDay = null;

					while (startOfWeekDay == null) {

						if (day.isoWeekday() === shiftTradeRequestDateSummary.FirstDayOfWeek) {
							startOfWeekDay = day;
							break;
						}
						day.add(-1, 'days');
					}

					if (startOfWeekDay) {

						while (!startOfWeekDay.isAfter(maximum)) {

							categories.push({
								name: $filter('date')(startOfWeekDay.toDate(), 'shortDate'),
								visible: true,
								suppressCategoryHeader : startOfWeekDay.isBefore(minimum)
							});

							startOfWeekDay.add(1, 'weeks');
						}
					}

				}

				return categories;

			}

			return service;

		}]);

}());
