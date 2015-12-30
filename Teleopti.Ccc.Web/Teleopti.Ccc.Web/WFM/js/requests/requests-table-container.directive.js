﻿(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer',   requestsTableContainerDirective);


	requestsTableContainerController.$inject = ['$scope', '$translate', '$filter', 'requestsDefinitions', 'requestCommandParamsHolder'];

	function requestsTableContainerController($scope, $translate, $filter, requestsDefinitions, requestCommandParamsHolder) {

		var vm = this;

		vm.gridOptions = getGridOptions([]);
		vm.getGridOptions = getGridOptions;
		vm.prepareComputedColumns = prepareComputedColumns;
		vm.clearSelection = clearSelection;

		function getGridOptions(requests) {
			return {
				enableGridMenu: true,
				useExternalSorting: true,
				data: requests,
				gridMenuTitleFilter: $translate,
				columnDefs: [
					{ displayName: 'StartTime', field: 'FormatedPeriodStartTime()', headerCellFilter: 'translate', cellClass: 'request-period-start-time', enableSorting: false, headerCellClass: 'request-period-start-time-header', cellFilter: 'date : "short"' },
					{ displayName: 'EndTime', field: 'FormatedPeriodEndTime()', headerCellFilter: 'translate', cellClass: 'request-period-end-time', enableSorting: false, headerCellClass: 'request-period-end-time-header', cellFilter: 'date : "short"', visible: false },
					{ displayName: 'Duration', field: 'GetDuration()', headerCellFilter: 'translate', cellClass: 'request-period-duration', enableSorting: false, headerCellClass: 'request-period-duration-header' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
					{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', cellClass: 'request-team', headerCellClass: 'request-team-header', enableSorting: false },
				    { displayName: 'Seniority', field: 'Seniority', headerCellFilter: 'translate', cellClass: 'request-seniority', headerCellClass: 'request-seniority-header', enableSorting: false },
					{ displayName: 'Type', field: 'GetType()', headerCellFilter: 'translate', cellClass: 'request-type', enableSorting: false, headerCellClass: 'request-type-header' },
					{ displayName: 'Subject', field: 'Subject', headerCellFilter: 'translate', cellClass: 'request-subject', enableSorting: false, headerCellClass: 'request-subject-header' },
					{ displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', enableSorting: false, headerCellClass: 'request-message-header', visible: false },
					{ displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', enableSorting: false, headerCellClass: 'request-status-header' },
					{ displayName: 'CreatedOn', field: 'CreatedTime', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header', cellFilter: 'date : "short"' },
					{ displayName: 'UpdatedOn', field: 'UpdatedTime', headerCellFilter: 'translate', cellClass: 'request-updated-time', cellFilter: 'date : "short"', visible: false, headerCellClass: 'request-updated-time-header' }
				],
				onRegisterApi: function (gridApi) {
					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function (x) { return x !== null; });
					});
					gridApi.selection.on.rowSelectionChanged($scope, function () {					
						requestCommandParamsHolder.setSelectedRequestIds(gridApi.selection.getSelectedRows().map(function (row) { return row.Id; }));

						if (vm.requests && (vm.requests.length === requestCommandParamsHolder.getSelectedRequestsIds().length)) {
							vm.gridApi.grid.selection.selectAll = true;
						}

					});
				}
			};
		}

		function formatToTimespan(length, isFullDay) {
			var days = moment.duration(length, 'seconds')._data.days;
			var hours = moment.duration(length, 'seconds')._data.hours;
			var minutes = moment.duration(length, 'seconds')._data.minutes == 0 ? '00' : moment.duration(length, 'seconds')._data.minutes;
			var totalHours = hours + days * 24 == 0 ? '00' : hours + days * 24;
			if (isFullDay) return totalHours + 1 + ":" + "00";
			else return totalHours + ":" + minutes;
		}

		function prepareComputedColumns(dataArray) {
			angular.forEach(dataArray, function(row) {
				var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
				
				var angularTimezone = moment.tz(row.TimeZone).format("Z");
				row.GetDuration = function () {
					return formatToTimespan(length, row.IsFullDay);
				}
				row.FormatedPeriodStartTime = function () {
					if (row.IsFullDay) return $filter('date')(row.PeriodStartTime, "shortDate", angularTimezone);
					else return $filter('date')(row.PeriodStartTime, 'short', angularTimezone);
				}
				row.FormatedPeriodEndTime = function () {			
					if (row.IsFullDay) return $filter('date')(row.PeriodEndTime, "shortDate", angularTimezone);
					else return $filter('date')(row.PeriodEndTime, "short", angularTimezone);
				}
				row.GetType = function () {
					var typeText = row.TypeText;					
					if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
						typeText += ' (' + row.Payload.Name + ')';
					}
					return typeText;
				}
			});
			return dataArray;
		}

		function clearSelection() {
			if (vm.gridApi.clearSelectedRows) vm.gridApi.clearSelectedRows();
			requestCommandParamsHolder.resetSelectedRequestIds();
		}
	}

	function requestsTableContainerDirective() {
		return {
			controller: 'requestsTableContainerCtrl',
			controllerAs: 'requestsTableContainer',
			bindToController: true,
			restrict: 'E',
			scope: {
				requests: '=',
				sortingOrders: '='				
			},
			require: ['requestsTableContainer'],
			templateUrl: 'js/requests/html/requests-table-container.tpl.html',		
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrls) {			
			var requestsTableContainerCtrl = ctrls[0];			
			scope.requestsTableContainer.gridOptions = requestsTableContainerCtrl.getGridOptions([]);
			
			scope.$watchCollection(function() {
				return scope.requestsTableContainer.requests;
			}, function (v) {
				requestsTableContainerCtrl.clearSelection();
				scope.requestsTableContainer.gridOptions.data = requestsTableContainerCtrl.prepareComputedColumns(v);
			});
		}
	}	

})();