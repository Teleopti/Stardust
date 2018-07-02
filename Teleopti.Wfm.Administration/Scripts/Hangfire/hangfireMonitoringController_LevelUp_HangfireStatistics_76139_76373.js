(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("hangfireMonitoringController_LevelUp_HangfireStatistics_76139_76373", hangfireMonitoringController, ["tokenHeaderService"]);

	function hangfireMonitoringController($http, $interval, tokenHeaderService) {
		var vm = this;
		vm.loading = true;

		$http.get("./Hangfire/Statistics2", tokenHeaderService.getHeaders())
			.then(function (response) {
				vm.jobFailures = response.data.JobFailures;
				vm.jobPerformance = response.data.JobPerformance;
				vm.loading = false;
			});
	}
})();