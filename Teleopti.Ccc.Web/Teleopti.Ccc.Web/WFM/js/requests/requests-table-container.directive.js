(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer',   requestsTableContainerDirective);


	requestsTableContainerController.$inject = ['$scope', '$translate', '$filter', 'requestsDefinitions', 'requestCommandParamsHolder','CurrentUserInfo'];

	function requestsTableContainerController($scope, $translate, $filter, requestsDefinitions, requestCommandParamsHolder,CurrentUserInfo) {

		var vm = this;

		vm.gridOptions = getGridOptions([]);
		vm.getGridOptions = getGridOptions;
		vm.prepareComputedColumns = prepareComputedColumns;
		vm.clearSelection = clearSelection;
		vm.reselectRequests = reselectRequests;

		function getGridOptions(requests) {
			return {
				enableGridMenu: true,
				enableHorizontalScrollbar: 2,
				useExternalSorting: true,
				data: requests,
				gridMenuTitleFilter: $translate,
				columnDefs: [
					{ displayName: 'StartTime', field: 'FormatedPeriodStartTime()', headerCellFilter: 'translate', cellClass: 'request-period-start-time', enableSorting: false, headerCellClass: 'request-period-start-time-header' },
					{ displayName: 'EndTime', field: 'FormatedPeriodEndTime()', headerCellFilter: 'translate', cellClass: 'request-period-end-time', enableSorting: false, headerCellClass: 'request-period-end-time-header' },
					{ displayName: 'TimeZone', field: 'TimeZone', headerCellFilter: 'translate', cellClass: 'request-time-zone', headerCellClass: 'request-time-zone-header',visible: false, enableSorting: false },
					{ displayName: 'Duration', field: 'GetDuration()', headerCellFilter: 'translate', cellClass: 'request-period-duration', enableSorting: false, visible: false, headerCellClass: 'request-period-duration-header' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
					{ displayName: 'Team', field: 'Team', headerCellFilter: 'translate', cellClass: 'request-team', headerCellClass: 'request-team-header', enableSorting: false },
				    { displayName: 'Seniority', field: 'Seniority', headerCellFilter: 'translate', cellClass: 'request-seniority', headerCellClass: 'request-seniority-header', visible: false, enableSorting: false },
					{ displayName: 'Type', field: 'GetType()', headerCellFilter: 'translate', cellClass: 'request-type', enableSorting: false, headerCellClass: 'request-type-header' },
					{ displayName: 'Subject', field: 'Subject', headerCellFilter: 'translate', cellClass: 'request-subject', enableSorting: false, headerCellClass: 'request-subject-header' },
					{ displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', enableSorting: false, headerCellClass: 'request-message-header', visible: false },
					{ displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', enableSorting: false, headerCellClass: 'request-status-header' },
					{ displayName: 'CreatedOn', field: 'FormatedCreatedTime()', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header'},
					{ displayName: 'UpdatedOn', field: 'FormatedUpdatedTime()', headerCellFilter: 'translate', cellClass: 'request-updated-time', visible: false, headerCellClass: 'request-updated-time-header' },
					{ displayName: 'Id', field: 'Id', headerCellFilter: 'translate', cellClass: 'request-id', visible: false, headerCellClass: 'request-id-header' }

				],
				onRegisterApi: function (gridApi) {
					vm.gridApi = gridApi;
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function (x) { return x !== null; });
					});
					gridApi.selection.on.rowSelectionChanged($scope, onSelectionChanged);
					gridApi.selection.on.rowSelectionChangedBatch($scope, onSelectionChanged);
				}
			};
		}

		function onSelectionChanged() {
			var visibleRequestsIds = vm.gridOptions.data.map(function (row) { return row.Id; });		
			var visibleSelectedRequestsIds = vm.gridApi.selection.getSelectedRows().map(function (row) { return row.Id; });
			var visibleNotSelectedRequestsIds = visibleRequestsIds.filter(function(id) {
				return visibleSelectedRequestsIds.indexOf(id) < 0;
			});

			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds();
			var newAllSelectedRequestsId = [];

			angular.forEach(allSelectedRequestsIds, function(id) {
				if (visibleNotSelectedRequestsIds.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});

			angular.forEach(visibleSelectedRequestsIds, function(id) {
				if (newAllSelectedRequestsId.indexOf(id) < 0)
					newAllSelectedRequestsId.push(id);
			});
					
			requestCommandParamsHolder.setSelectedRequestIds(newAllSelectedRequestsId);
			
			if (vm.requests && (vm.requests.length === visibleSelectedRequestsIds.length) && vm.requests.length > 0) {
				vm.gridApi.grid.selection.selectAll = true;
			} else {
				vm.gridApi.grid.selection.selectAll = false;
			}
		}

		function getVisibleSelectedRequestsRows() {
			if (!vm.gridOptions.data) return [];
			var allSelectedRequestsIds = requestCommandParamsHolder.getSelectedRequestsIds();
			return vm.gridOptions.data.filter(function(row) {
				return allSelectedRequestsIds.indexOf(row.Id) > -1;
			});			
		}

		function formatToTimespan(length, isFullDay) {
			var days = moment.duration(length, 'seconds')._data.days;
			var hours = moment.duration(length, 'seconds')._data.hours;
			var minutes = moment.duration(length, 'seconds')._data.minutes == 0 ? '00' : moment.duration(length, 'seconds')._data.minutes;
			var totalHours = hours + days * 24 == 0 ? '00' : hours + days * 24;
			if (isFullDay) return totalHours + 1 + ":" + "00";
			else return totalHours + ":" + minutes;
		}

		function prepareComputedColumns(requests) {
			
			vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			
			function formatedDateTime(dateTime, timezone, displayDateOnly) {
				var angularTimezone = moment.tz(vm.isUsingRequestSubmitterTimeZone == true ? timezone : vm.userTimeZone).format("Z");
				var _dateTime = moment.tz(dateTime, timezone).toDate();
				if (displayDateOnly && vm.isUsingRequestSubmitterTimeZone) return $filter('date')(_dateTime, "shortDate", angularTimezone);
				else return $filter('date')(_dateTime, "short", angularTimezone);
			}

			angular.forEach(requests, function (row) {
						
				row.GetDuration = function () {
					var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
					return formatToTimespan(length, row.IsFullDay);
				}
				row.FormatedPeriodStartTime = function() { return formatedDateTime(row.PeriodStartTime, row.TimeZone, row.IsFullDay); };
				row.FormatedPeriodEndTime = function() { return formatedDateTime(row.PeriodEndTime, row.TimeZone, row.IsFullDay); };

				row.FormatedCreatedTime = function() { return formatedDateTime(row.CreatedTime, row.TimeZone, false); };
				row.FormatedUpdatedTime = function() { return formatedDateTime(row.UpdatedTime, row.TimeZone, false); };

				row.GetType = function () {
					var typeText = row.TypeText;					
					if (row.Type == requestsDefinitions.REQUEST_TYPES.ABSENCE) {
						typeText += ' (' + row.Payload.Name + ')';
					}
					return typeText;
				}
			});
			
			return requests;
		}

		function clearSelection() {			
			if (vm.gridApi.clearSelectedRows) {
				vm.gridApi.clearSelectedRows();								
			}
			vm.gridApi.grid.selection.selectAll = false;
			requestCommandParamsHolder.resetSelectedRequestIds();
			vm.gridApi.selection.clearSelectedRows();

		}

		function reselectRequests() {
			if (!vm.gridOptions.data) return;

			var rows = getVisibleSelectedRequestsRows();
			vm.gridApi.grid.modifyRows(vm.gridOptions.data);
			angular.forEach(rows, function(row) {
				vm.gridApi.selection.selectRow(row);
			});

			if (vm.requests && (vm.requests.length === rows.length) && vm.requests.length > 0) {
				vm.gridApi.grid.selection.selectAll = true;
			} else {
				vm.gridApi.grid.selection.selectAll = false;
			}
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
			scope.requestsTableContainer.isUsingRequestSubmitterTimeZone = true;

			scope.$watch(function () {
				return scope.requestsTableContainer.requests;
			}, function (v) {
				scope.requestsTableContainer.gridOptions.data = requestsTableContainerCtrl.prepareComputedColumns(v);
				requestsTableContainerCtrl.reselectRequests();
			},true);

			scope.$on('reload.requests.without.seletion', function () {
				requestsTableContainerCtrl.clearSelection();
			});

			scope.$on('reload.requests.with.seletion', function () {
				requestsTableContainerCtrl.reselectRequests();
			});
		}
	}	

})();