(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);


	requestsOverviewController.$inject = ['requestsData'];

	function requestsOverviewController(requestsData) {
		var vm = this;

		vm.requests = [];
		vm.init = init;
		
		function init() {
			requestsData.getAllRequestsPromise().then(function (requests) {
				vm.requests = requests;			
			});
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			restrict: 'E',
			//templateUrl: 'js/requests/html/requests-overview.tpl.html'
			template: getTemplate()
		};

	}


	function getTemplate() {
		return '<requests-table-container class="requests-table-container" requests-data="vm.requests"></requests-table-container>';
	}


})();