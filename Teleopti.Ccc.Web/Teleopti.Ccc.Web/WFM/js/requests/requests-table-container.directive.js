(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsTableContainerCtrl', requestsTableContainerController)
		.directive('requestsTableContainer', requestsTableContainerDirective);

	function requestsTableContainerController() {

		this.gridOptions = getGridOptions([]);
		this.getGridOptions = getGridOptions;

		function getGridOptions(requests) {
			return {
				data: requests,
				columnDefs: [
					{ displayName: 'Id', field: 'Id', cellClass: 'request-id'}
				]
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
				requests: '='
			},
			require: 'requestsTableContainer',
			templateUrl: 'js/requests/html/requests-table-container.tpl.html',		
			link: postlink
		};


		function postlink(scope, elem, attrs, ctrl) {
			scope.requestsTableContainer.gridOptions = ctrl.getGridOptions([]);
			scope.requestsTableContainer.gridOptions.data = scope.requestsTableContainer.requests;
		}
	}	

})();