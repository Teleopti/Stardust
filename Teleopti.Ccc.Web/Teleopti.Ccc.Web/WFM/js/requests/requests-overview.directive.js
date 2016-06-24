﻿(function() {

	'use strict';

	angular.module('wfm.requests')
		.controller('requestsOverviewCtrl', requestsOverviewController)
		.directive('requestsOverview', requestsOverviewDirective);

	requestsOverviewController.$inject = ['$scope', 'requestsDataService', "Toggle", "requestCommandParamsHolder", "$translate"];

	function requestsOverviewController($scope, requestsDataService, toggleService, requestCommandParamsHolder, $translate) {
		var vm = this;

		vm.loadRequestWatchersInitialised = false;
		
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
		vm.init = init;
		vm.onIsActiveChange = onIsActiveChange;
		vm.showSelectedRequestsInfo = showSelectedRequestsInfo;	
		
		getSelectedRequestsInfoText();
		
		vm.paging = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 1,
			totalRequestsCount: 0
		};

		
		function init() {
			vm.requestsPromise = vm.shiftTradeView ? requestsDataService.getShiftTradeRequestsPromise : requestsDataService.getAllRequestsPromise;
			vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
			vm.loaded = false;
		}

		function onIsActiveChange(isActive) {
			if (isActive) {
				if (vm.isDirty || !vm.loaded) {
					vm.reload();
				}
			}
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
				
				if (vm.requests && vm.requests.length > 0) {
					vm.shiftTradeRequestDateSummary = {
						Minimum: requests.data.MinimumDateTime,
						Maximum: requests.data.MaximumDateTime,
						FirstDayOfWeek: requests.data.FirstDayOfWeek
					}
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

		function reload(callback) {

			if (!vm.isActive) {

				vm.isDirty = true; // will cause a reload when is made active.
				return;
			}
			
			var requestsFilter = {
				period: vm.period,
				agentSearchTerm: vm.agentSearchTerm,
				filters: vm.filters
			}
		
			vm.loaded = false;
			vm.isDirty = false;

			if (vm.isPaginationEnabled) {
				getRequests(requestsFilter, vm.sortingOrders, vm.paging, callback);
			} else {
				requestsDataService.getAllRequestsPromise_old(requestsFilter, vm.sortingOrders).then(function(requests) {
					vm.requests = requests.data;
					vm.loaded = true;
				});
				if (callback != null) callback();
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
				filterEnabled: '=',
				isActive: '='
				
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-overview.tpl.html',
			link: postlink
		};

		function validateDateParameters(startDate, endDate) {
			if (endDate === null || startDate === null) return false;
			return !(moment(endDate).isBefore(startDate, 'day'));
		}

		function postlink(scope, elem, attrs, ctrl) {

			ctrl.shiftTradeView = 'shiftTradeView' in attrs;

			var vm = scope.requestsOverview;

			scope.$watch(function() {
				var target = {
					startDate: vm.period ? vm.period.startDate : null,
					endDate: vm.period ? vm.period.endDate : null,
					agentSearchTerm: vm.agentSearchTerm ? vm.agentSearchTerm : '',
					filters: vm.filters ? vm.filters : ''
				};
				return target;
			}, function (newValue) {

				if (!validateDateParameters(newValue.startDate, newValue.endDate)) {
					return;
				}
				
				scope.$broadcast('reload.requests.without.selection');
				
				
				if (!ctrl.loadRequestWatchersInitialised) {
					listenToReload();
				}

				
			}, true);

			scope.$watch(function() {
				return vm.isActive;
			}, function(newValue) {


				if (!validateDateParameters(vm.period.startDate, vm.period.endDate)) {
					return;
				}

				vm.onIsActiveChange(newValue);
			
			});

			scope.$watch(function() {
				return vm.sortingOrders;
			}, function(newValue) {
				if (ctrl.loaded) {
					reload();
				}
			});

			function listenToReload() {
				ctrl.loadRequestWatchersInitialised = true;
				
				scope.$on('reload.requests.with.selection', function(event) {
					reload();
				});

				scope.$on('reload.requests.without.selection', function(event) {

					reload();

				});
			}

			function reload(callback) {
				ctrl.reload(callback);

			}
		}
	}
})();
