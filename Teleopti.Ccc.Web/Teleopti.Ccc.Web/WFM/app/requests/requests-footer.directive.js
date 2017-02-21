(function () {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsFooterCtrl', requestsFooterController)
		.directive('requestsFooter', requestsFooterDirective);

	requestsFooterController.$inject = ['$scope', "$attrs", 'requestsDataService', "Toggle", "requestCommandParamsHolder", "$translate"];

	function requestsFooterController($scope, $attrs, requestsDataService, toggleService, requestCommandParamsHolder, $translate) {
		var vm = this;
		vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
		vm.onPageSizeChanges = onPageSizeChanges;
		vm.forceRequestsReloadWithSelection = forceRequestsReloadWithSelection;
		vm.isUsingRequestSubmitterTimeZone = true;
		
		function onPageSizeChanges() {
			vm.paging.totalPages = Math.ceil(vm.paging.totalRequestsCount / vm.paging.pageSize);
			vm.paging.pageNumber = 1;
		}

		function forceRequestsReloadWithSelection(data) {
			vm.paging.pageNumber = data;
		}
	}


	function requestsFooterDirective() {
		return {
			controller: 'requestsFooterCtrl',
			controllerAs: 'requestsFooter',
			bindToController: true,
			scope: {
				paging: '=?',
				pageSizeOptions: '=?',
				isShiftTradeViewActive: '=?',
				isUsingRequestSubmitterTimeZone: '=?'
			},
			restrict: 'E',
			templateUrl: 'app/requests/html/requests-footer.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			var vm = scope.requestsOverview;

		}
	}
})();
