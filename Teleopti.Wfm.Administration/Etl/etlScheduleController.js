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

		vm.schedules = [
			{
				Name: "My main job",
				Jname: "Nightly",
				TenantName: 'WFM Teleopti',
				Frequency: 'hourly',
				Enabled: true,
				Description: "Occurs every day at 15:58. Using the log data"
			},
			{
				Name: "My secondary job",
				Jname: "Nightly",
				TenantName: 'WFM Teleopti',
				Frequency: 'hourly',
				Enabled: true,
				Description: "Occurs some days at 15:58. Who knows?"
			}
		];

		vm.scheduleNameEnabled = true;
		vm.scheduleToEdit;
		vm.frequencyType = false;

		vm.toggleFrequencyType = toggleFrequencyType;

		(function init() {
		
		})();

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



