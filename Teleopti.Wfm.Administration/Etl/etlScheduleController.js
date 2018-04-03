(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("etlScheduleController", etlScheduleController, [
			"$http",
			"$timeout"
		]);

	function etlScheduleController($http, tokenHeaderService, $timeout) {
		var vm = this;

		vm.schedules = null;
		vm.scheduleNameEnabled = true;
		vm.scheduleToEdit;
		vm.frequencyType = false;

		vm.toggleFrequencyType = toggleFrequencyType;

		(function init() {
			//getScheduledJobs();
		})();

		//function getScheduledJobs() {
		//	vm.schedules = null;
		//	$http
		//		.get("./Etl/ScheduledJobs", tokenHeaderService.getHeaders())
		//		.success(function (data) {
		//			vm.schedules = data;
		//		});
		//}

		function toggleFrequencyType() {
			if (vm.frequencyType) {
				//startEnd
				console.log('startEnd');
				vm.scheduleToEdit.onceTime = null;
			} else {
				// today
				console.log('today');
				vm.scheduleToEdit.onceTime = '15:00';

				vm.scheduleToEdit.everyTime = null;
				vm.scheduleToEdit.everyStartTime = null;
				vm.scheduleToEdit.everyEndTime = null;
			}
		}
	}
})();



