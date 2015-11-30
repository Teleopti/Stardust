(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);


	requestsOverviewController.$inject = ['requestsData'];

	function requestsOverviewController(requestsData) {
		var vm = this;

		vm.requests = [];
		vm.requestsFilter = {};
		vm.init = init;

		init();

		function init() {
			requestsData.getAllRequestsPromise(vm.requestsFilter).then(function (requests) {
				vm.requests = requests;			
			});
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html'			
		};		
	}

})();