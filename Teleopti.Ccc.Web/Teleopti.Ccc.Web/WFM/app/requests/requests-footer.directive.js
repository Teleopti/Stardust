(function () {
	'use strict';
	angular.module('wfm.requests')
		.controller('requestsFooterCtrl', requestsFooterController)
		.directive('requestsFooter', requestsFooterDirective);

	requestsFooterController.$inject = ['$rootScope', '$scope', "$translate", "$state", "Toggle", "requestCommandParamsHolder", "REQUESTS_TAB_NAMES"];

	function requestsFooterController($rootScope, $scope, $translate, $state, toggleSvc, requestCommandParamsHolder, REQUESTS_TAB_NAMES) {
		var vm = this;
		vm.onPageSizeChanges = onPageSizeChanges;
		vm.onPageNumberChange = onPageNumberChange;
		vm.isUsingRequestSubmitterTimeZone = false;
		vm.showSelectedRequestsInfo = showSelectedRequestsInfo;
		getSelectedRequestsInfoText();
		
		function onPageSizeChanges() {
			vm.paging.totalPages = Math.ceil(vm.paging.totalRequestsCount / vm.paging.pageSize);
			vm.paging.pageNumber = 1;
			forceRequestsReloadWithPagingChange();
		}

		function onPageNumberChange(data) {
			vm.paging.pageNumber = data;
			forceRequestsReloadWithPagingChange();
		}

		function forceRequestsReloadWithPagingChange() {
			$rootScope.$broadcast('reload.requests.with.selection', { paging: vm.paging });
		}

		function getSelectedRequestsInfoText() {
			$translate("SelectedRequestsInfo").then(function (text) {
				vm.selectedRequestsInfoText = text;
			});
		}

		function showSelectedRequestsInfo() {
			if (toggleSvc.Wfm_Requests_OvertimeRequestHandling_45177 && toggleSvc.Wfm_Requests_Refactoring_45470 && $state.current.name.indexOf(REQUESTS_TAB_NAMES.overtime) > -1) {
					vm.selectedRequestsCount = requestCommandParamsHolder.getOvertimeSelectedRequestIds().length;
			} else {
				vm.selectedRequestsCount = requestCommandParamsHolder.getSelectedRequestsIds(vm.isShiftTradeViewActive).length;
			}

			if (vm.selectedRequestsCount > 0 && vm.selectedRequestsInfoText) {
				return vm.selectedRequestsInfoText.replace(/\{0\}|\{1\}/gi, function (target) {
					if (target == '{0}') return vm.selectedRequestsCount;
					if (target == '{1}') return vm.paging.totalRequestsCount;
				});
			} else {
				return '';
			}
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
		}
	}
})();
