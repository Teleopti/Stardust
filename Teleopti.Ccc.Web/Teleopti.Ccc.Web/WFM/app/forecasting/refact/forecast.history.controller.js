(function() {
	'use strict';

	angular.module('wfm.forecasting').controller('ForecastHistoryController', ForecastHistoryCtrl);

	ForecastHistoryCtrl.$inject = [
		'forecastingService',
		'NoticeService',
		'$translate',
		'$state',
		'$scope',
		'skillIconService'
	];

	function ForecastHistoryCtrl(forecastingService, noticeSvc, $translate, $state, $scope, skillIconService) {
		var vm = this;

		vm.getSkillIcon = skillIconService.get;

		(function init() {
			loadWorkload();
			if (!vm.workloadFound) {
				return;
			}

			vm.isLoadingHistoryData = true;
			forecastingService.history(
				angular.toJson(vm.selectedWorkload.Workload.Id),
				function(data, status, headers, config) {
					vm.historyData = data.QueueStatisticsDays;
					vm.hasHistoryData = vm.historyData.length > 0;
				},
				function(data, status, headers, config) {
					// TODO: Show error message on failed
				},
				function() {
					vm.isLoadingHistoryData = false;
				}
			);
		})();

		function loadWorkload() {
			vm.workloadFound = true;
			if (sessionStorage.currentForecastWorkload) {
				vm.selectedWorkload = angular.fromJson(sessionStorage.currentForecastWorkload);
			} else {
				vm.workloadFound = false;
			}
		}
	}
})();
