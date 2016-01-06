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

		function getGridOptions(requests) {
			return {
				enableGridMenu: true,
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
					{ displayName: 'UpdatedOn', field: 'FormatedUpdatedTime()', headerCellFilter: 'translate', cellClass: 'request-updated-time', visible: false, headerCellClass: 'request-updated-time-header' }

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
						} else {
							vm.gridApi.grid.selection.selectAll = false;
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

		function prepareComputedColumns(requests) {
			
			vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			
			function formateDateTime(dateTime, timezone, displayDateOnly) {
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
				row.FormatedPeriodStartTime = function() { return formateDateTime(row.PeriodStartTime, row.TimeZone, row.IsFullDay); };
				row.FormatedPeriodEndTime = function() { return formateDateTime(row.PeriodEndTime, row.TimeZone, row.IsFullDay); };

				row.FormatedCreatedTime = function() { return formateDateTime(row.CraetedTime, row.TimeZone, false); };
				row.FormatedUpdatedTime = function() { return formateDateTime(row.UpdatedTime, row.TimeZone, false); };

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
				requestsTableContainerCtrl.clearSelection();
				scope.requestsTableContainer.gridOptions.data = requestsTableContainerCtrl.prepareComputedColumns(v);
			},true);

			scope.$on('reload.requests.immediately', function () {
				requestsTableContainerCtrl.clearSelection();
			});
		}
	}	

})();