(function() {
	"use strict";

	angular
        .module("adminApp")
        .controller("hangfireMonitoringController_RTA_HangfireStatistics_76139_76373", hangfireMonitoringController, ["tokenHeaderService"]);

	function hangfireMonitoringController($http, $interval, tokenHeaderService) {
		var vm = this;
		vm.getTypesOfFailedEvents = getTypesOfFailedEvents;
		vm.getPerformanceStatistics = getPerformanceStatistics;
		vm.isFetching = false;

		getTypesOfFailedEvents();
		getPerformanceStatistics();

		function getTypesOfFailedEvents() {
			vm.isFetching = true;
			return $http.get("./Hangfire/TypesOfFailedEvents", tokenHeaderService.getHeaders())
				.then(function (data) {
					vm.eventCountFailed = data.data.sort(byCount);
					vm.isFetching = false;
				});
		}
		
		function byCount(e1, e2) {
			return e2.Count - e1.Count;
		}

		function getPerformanceStatistics() {
			vm.isFetching = true;
			return $http.get("./Hangfire/PerformanceStatistics", tokenHeaderService.getHeaders())
				.then(function (data) {
					vm.jobs = data.data;
					vm.isFetching = false;
				});
		}


	}
})();