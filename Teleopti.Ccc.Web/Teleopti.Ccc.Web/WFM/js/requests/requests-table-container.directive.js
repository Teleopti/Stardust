(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer',   requestsTableContainerDirective);


	requestsTableContainerController.$inject = ['$scope', 'requestsDefinitions'];

	function requestsTableContainerController($scope, requestsDefinitions) {

		var vm = this;

		vm.gridOptions = getGridOptions([]);
		vm.getGridOptions = getGridOptions;

		function getGridOptions(requests) {
			return {
				enableGridMenu: true,
				useExternalSorting: true,
				data: requests,
				columnDefs: [
					{ displayName: 'CreatedOn', field: 'CreatedTime', headerCellFilter: 'translate', cellClass: 'request-created-time', headerCellClass: 'request-created-time-header', cellFilter: 'date : "short"' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name', headerCellClass: 'request-agent-name-header' },
					{ displayName: 'Type', field: 'TypeText', headerCellFilter: 'translate', cellClass: 'request-type', enableSorting: false, headerCellClass: 'request-type-header' },
					{ displayName: 'Subject', field: 'Subject', headerCellFilter: 'translate', cellClass: 'request-subject', enableSorting: false, headerCellClass: 'request-subject-header' },
					{ displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', enableSorting: false, headerCellClass: 'request-message-header' },
					{ displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', enableSorting: false, headerCellClass: 'request-status-header' },
					{ displayName: 'UpdatedOn', field: 'UpdatedTime', headerCellFilter: 'translate', cellClass: 'request-updated-time', cellFilter: 'date : "shortDate"', visible: false, headerCellClass: 'request-updated-time-header' }
				],
				onRegisterApi: function (gridApi) {
					gridApi.core.on.sortChanged($scope, function (grid, sortColumns) {
						vm.sortingOrders = sortColumns.map(requestsDefinitions.translateSingleSortingOrder).filter(function (x) { return x !== null; });
					});
				}
			};
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
			}, function(v) {
				scope.requestsTableContainer.gridOptions.data = v;				
			});

					
		}
	}	

})();