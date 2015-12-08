﻿(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer',   requestsTableContainerDirective);


	requestsTableContainerController.$inject = ['$scope', '$translate', 'requestsDefinitions'];

	function requestsTableContainerController($scope, $translate, requestsDefinitions) {

		var vm = this;

		vm.gridOptions = getGridOptions([]);
		vm.getGridOptions = getGridOptions;
		vm.prepareComputedColumns = prepareComputedColumns;

		function getGridOptions(requests) {
			return {
				enableGridMenu: true,
				useExternalSorting: true,
				data: requests,
				gridMenuTitleFilter: $translate,
				columnDefs: [
					{ displayName: 'StartTime', field: 'PeriodStartTime', headerCellFilter: 'translate', cellClass: 'request-period-start-time', enableSorting: false, headerCellClass: 'request-period-start-time-header', cellFilter: 'date : "short"' },
					{ displayName: 'EndTime', field: 'PeriodEndTime', headerCellFilter: 'translate', cellClass: 'request-period-end-time', enableSorting: false, headerCellClass: 'request-period-end-time-header', cellFilter: 'date : "short"', visible: false },
					{ displayName: 'Duration', field: 'GetDuration()', headerCellFilter: 'translate', cellClass: 'request-period-duration', enableSorting: false, headerCellClass: 'request-period-duration-header' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
				    { displayName: 'Seniority', field: 'Seniority', headerCellFilter: 'translate', cellClass: 'request-seniority', headerCellClass: 'request-seniority-header' },
					{ displayName: 'Type', field: 'TypeText', headerCellFilter: 'translate', cellClass: 'request-type', enableSorting: false, headerCellClass: 'request-type-header' },
					{ displayName: 'Subject', field: 'Subject', headerCellFilter: 'translate', cellClass: 'request-subject', enableSorting: false, headerCellClass: 'request-subject-header' },
					{ displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', enableSorting: false, headerCellClass: 'request-message-header', visible: false },
					{ displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', enableSorting: false, headerCellClass: 'request-status-header' },
					{ displayName: 'CreatedOn', field: 'CreatedTime', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header', cellFilter: 'date : "short"' },
					{ displayName: 'UpdatedOn', field: 'UpdatedTime', headerCellFilter: 'translate', cellClass: 'request-updated-time', cellFilter: 'date : "shortDate"', visible: false, headerCellClass: 'request-updated-time-header' }
				],
				onRegisterApi: function (gridApi) {
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function (x) { return x !== null; });
					});
				}
			};
		}

		function prepareComputedColumns(dataArray) {
			angular.forEach(dataArray, function(row) {
				row.GetDuration = function() {
					var length = moment(row.PeriodEndTime).diff(moment(row.PeriodStartTime), 'seconds');
					return moment.duration(length, 'seconds').humanize();
				}
			});
			return dataArray;
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
				scope.requestsTableContainer.gridOptions.data = requestsTableContainerCtrl.prepareComputedColumns(v);
			});

					
		}
	}	

})();