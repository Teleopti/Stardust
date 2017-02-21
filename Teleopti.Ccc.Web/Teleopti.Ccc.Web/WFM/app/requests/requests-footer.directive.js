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
				pageSizeOptions:'=?'
			},
			restrict: 'E',
			templateUrl: 'app/requests/html/requests-footer.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {
			var vm = scope.requestsOverview;

			

			function listenToReload() {
				scope.$on('reload.requests.with.selection', function (event, data) {
					if ((!angular.isArray(vm.selectedTeamIds) || vm.selectedTeamIds.length == 0) && angular.isUndefined(data)) {
						return;
					}

					ctrl.reload(data);
				});

				scope.$on('reload.requests.without.selection', function (event) {
					ctrl.reload();
				});

				ctrl.loadRequestWatchersInitialized = true;
			}
		}
	}
})();
