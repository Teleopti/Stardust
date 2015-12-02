(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);


	requestsOverviewController.$inject = ['requestsData'];

	function requestsOverviewController(requestsData) {
		var vm = this;

		vm.requests = [];
		
		vm.requestsFilter = {
			period: {
				startDate: new Date(),
				endDate: moment().add(1, 'day').toDate()
			}
		};
		vm.reload = reload;

		function reload(requestsFilter) {
			vm.loaded = false;
			requestsData.getAllRequestsPromise(requestsFilter).then(function (requests) {
				vm.requests = requests.data;
				vm.loaded = true;
			});
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: { },
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			scope.$watch(function() {
				var period = scope.requestsOverview.requestsFilter.period;
				return {
					startDate: period.startDate,
					endDate: period.endDate
				}
			}, function(newValue) {
				if (moment(newValue.endDate).isBefore(newValue.startDate, 'day')) return;				
				scope.requestsOverview.requestsFilter.period = newValue;
				ctrl.reload(scope.requestsOverview.requestsFilter);

			}, true);
		};
	}

})();