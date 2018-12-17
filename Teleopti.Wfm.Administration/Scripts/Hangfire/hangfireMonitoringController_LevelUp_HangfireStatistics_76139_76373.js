(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("hangfireMonitoringController_LevelUp_HangfireStatistics_76139_76373", hangfireMonitoringController);

	function hangfireMonitoringController($http, $interval) {
		var vm = this;
		vm.loading = true;

		$http.get("./Hangfire/Statistics2")
			.then(function (response) {
				vm.jobFailures = response.data.JobFailures;
				vm.jobPerformance = response.data.JobPerformance;
				vm.loading = false;
			});
	}
})();