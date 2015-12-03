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
					{ displayName: 'CreatedOn', field: 'CreatedTime', headerCellFilter: 'translate', cellClass: 'request-created-time', enableSorting: false, cellFilter: 'date : "short"' },
					{ displayName: 'AgentName', field: 'AgentName', headerCellFilter: 'translate', cellClass: 'request-agent-name' },
					{ displayName: 'Type', field: 'TypeText', headerCellFilter: 'translate', cellClass: 'request-type', enableSorting: false },
					{ displayName: 'Subject', field: 'Subject', headerCellFilter: 'translate', cellClass: 'request-subject', enableSorting: false },
					{ displayName: 'Message', field: 'Message', headerCellFilter: 'translate', cellClass: 'request-message', enableSorting: false },					
					{ displayName: 'Status', field: 'StatusText', headerCellFilter: 'translate', cellClass: 'request-status', enableSorting: false },
					{ displayName: 'UpdatedOn', field: 'UpdatedTime', headerCellFilter: 'translate', cellClass: 'request-updated-time', cellFilter: 'date : "shortDate"', visible: false }
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