(function() {
	"use strict";

	angular
        .module("adminApp")
        .controller("hangfireMonitoringController_RTA_HangfirePerformanceStats_76139", hangfireMonitoringController, ["tokenHeaderService"]);

	function hangfireMonitoringController($http, $interval, tokenHeaderService) {
		var vm = this;
		vm.getTypesOfFailedEvents = getTypesOfFailedEvents;
		vm.requeueFailedEvents = requeueFailedEvents;
		vm.deleteFailedEvents = deleteFailedEvents;
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

		function requeueFailedEvents(type) {
			return $http.post("./Hangfire/RequeueFailed", '"' + type + '"', tokenHeaderService.getHeaders()).then(function() {
				getTypesOfFailedEvents();
			});
		}

		function deleteFailedEvents(type) {
			return $http.post("./Hangfire/DeleteFailed", '"' + type + '"', tokenHeaderService.getHeaders()).then(function () {
				getTypesOfFailedEvents();
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