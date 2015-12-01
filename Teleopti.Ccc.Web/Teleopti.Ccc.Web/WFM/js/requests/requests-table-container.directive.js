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
				enableGridMenu: true,
				data: requests,
				columnDefs: [
					{ displayName: 'Subject', field: 'Subject', cellClass: 'request-subject', enableSorting: false },
					{ displayName: 'Message', field: 'Message', cellClass: 'request-message', enableSorting: false },
					{ displayName: 'Type', field: 'TypeText', cellClass: 'request-type', enableSorting: false },
					{ displayName: 'AgentName', field: 'AgentName', cellClass: 'request-name', enableSorting: false },
					{ displayName: 'Status', field: 'StatusText', cellClass: 'request-status', enableSorting: false },
					{ displayName: 'UpdatedTime', field: 'UpdatedTime', cellClass: 'request-updated-time'}
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