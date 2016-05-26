(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);

	requestsOverviewController.$inject = ['$scope', 'requestsDataService', "Toggle", "requestCommandParamsHolder", "$translate"];

	function requestsOverviewController($scope, requestsDataService, toggleService, requestCommandParamsHolder, $translate) {
		var vm = this;
		
		vm.requests = [];
		vm.period = {
			startDate: moment().startOf('week')._d,
			endDate: moment().endOf('week')._d
		};

		toggleService.togglesLoaded.then(init);
				
		vm.agentSearchTerm = "";
		vm.filters = [];
		vm.period.endDate = moment().endOf('week')._d;
		vm.reload = reload;
		vm.sortingOrders = [];
		
		vm.forceRequestsReloadWithSelection = forceRequestsReloadWithSelection;
		vm.onTotalRequestsCountChanges = onTotalRequestsCountChanges;
		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.onPageSizeChanges = onPageSizeChanges;
		
		vm.showSelectedRequestsInfo = showSelectedRequestsInfo;
		
		getSelectedRequestsInfoText();

		vm.init = init;

		vm.paging = {
			pageSize: 50,
			pageNumber: 1,
			totalPages: 1,
			totalRequestsCount: 0
		};


		function init() {
			vm.requestsPromise = vm.shiftTradeView ? requestsDataService.getShiftTradeRequestsPromise : requestsDataService.getAllRequestsPromise;
			vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
		}

		function forceRequestsReloadWithSelection() {
			$scope.$broadcast('reload.requests.with.selection');
		}


		function getSelectedRequestsInfoText() {
			$translate("SelectedRequestsInfo").then(function (text) {
				vm.selectedRequestsInfoText = text;
			});
		}

		function showSelectedRequestsInfo() {

			console.log(requestCommandParamsHolder.getSelectedRequestsIds(),"GET REQUEST IDS");

			vm.selectedRequestsCount = requestCommandParamsHolder.getSelectedRequestsIds(vm.shiftTradeView).length;
			if (vm.selectedRequestsCount > 0 && vm.selectedRequestsInfoText) {
				return vm.selectedRequestsInfoText.replace(/\{0\}|\{1\}/gi, function(target) {
					if (target == '{0}') return vm.selectedRequestsCount;
					if (target == '{1}') return vm.paging.totalRequestsCount;
				});
			} else {
				return '';
			}
		}

		function onPageSizeChanges() {
			vm.paging.totalPages = Math.ceil(vm.paging.totalRequestsCount / vm.paging.pageSize);
			vm.paging.pageNumber = 1;
			forceRequestsReloadWithSelection();
		}

		function onTotalRequestsCountChanges(totalRequestsCount) {

			var totalPages = Math.ceil(totalRequestsCount / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;

			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = totalRequestsCount;
		}

		function getRequests(requestsFilter, sortingOrders, paging, done) {
			vm.requestsPromise(requestsFilter, sortingOrders, paging).then(function (requests) {
				vm.requests = requests.data.Requests;
				vm.shiftTradeRequestDateSummary = {
					Minimum: requests.data.MinimumDateTime,
					Maximum: requests.data.MaximumDateTime,
					StartOfWeek: requests.data.FirstDateForVisualisation,
					EndOfWeek: requests.data.LastDateForVisualisation
				}
				if (vm.totalRequestsCount !== requests.data.TotalCount) {
					vm.totalRequestsCount = requests.data.TotalCount;
					if (typeof vm.onTotalRequestsCountChanges == 'function')
						vm.onTotalRequestsCountChanges(vm.totalRequestsCount);
				}
				vm.loaded = true;
				if (done != null) done();
			});
		}

		function reload(requestsFilter, sortingOrders, paging, done) {
			vm.loaded = false;

			if (vm.isPaginationEnabled) {
				getRequests(requestsFilter, sortingOrders, paging, done);
			} else {
				requestsDataService.getAllRequestsPromise_old(requestsFilter, sortingOrders).then(function(requests) {
					vm.requests = requests.data;
					vm.loaded = true;
				});
				if (done != null) done();
			}
		}
	}

	function requestsOverviewDirective() {
		return {
			controller: 'requestsOverviewCtrl',
			controllerAs: 'requestsOverview',
			bindToController: true,
			scope: {
				period: '=',
				agentSearchTerm: '=?',
				filters: '=?',
				filterEnabled: '='
				
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function postlink(scope, elem, attrs, ctrl) {

			ctrl.shiftTradeView = 'shiftTradeView' in attrs;
			
			scope.$watch(function() {
				var target = {
					startDate: scope.requestsOverview.period ? scope.requestsOverview.period.startDate : null,
					endDate: scope.requestsOverview.period ? scope.requestsOverview.period.endDate : null,
					agentSearchTerm: scope.requestsOverview.agentSearchTerm ? scope.requestsOverview.agentSearchTerm : '',
					filters: scope.requestsOverview.filters ? scope.requestsOverview.filters : ''
				};
				return target;
			}, function(newValue) {
				if (newValue.endDate === null || newValue.startDate === null) return;
				if (moment(newValue.endDate).isBefore(newValue.startDate, 'day')) return;
				scope.$broadcast('reload.requests.without.selection');
				listenToReload();
			}, true);

			scope.$watch(function() {
				return scope.requestsOverview.sortingOrders;
			}, reload());

			function listenToReload() {
				scope.$on('reload.requests.with.selection', reload());
				scope.$on('reload.requests.without.selection', reload());
			}

			function reload(done) {
				return function () {
					ctrl.reload({
						period: scope.requestsOverview.period,
						agentSearchTerm: scope.requestsOverview.agentSearchTerm,
						filters: scope.requestsOverview.filters
					}, scope.requestsOverview.sortingOrders, scope.requestsOverview.paging, done);
				};
			}

			ctrl.init();
		}
	}
})();
