(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("hangfireMonitoringController_RTA_HangfireStatistics_76139_76373", hangfireMonitoringController, ["tokenHeaderService"]);

	function hangfireMonitoringController($http, $interval, tokenHeaderService) {
		var vm = this;

		$http.get("./Hangfire/Statistics2", tokenHeaderService.getHeaders())
			.then(function (response) {
				console.log(response.data);
				vm.jobFailures = response.data.JobFailures;
				vm.jobPerformance = response.data.JobPerformance;
			});
	}
})();