(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("etlScheduleController", etlScheduleController, [
			"$http",
			"$timeout"
		]);

	function etlScheduleController($http, $timeout) {
		var vm = this;

		vm.schedules = null;
		vm.scheduleNameEnabled = true;
		vm.scheduleToEdit;
		vm.frequencyType = false;
		vm.showAddScheduleJobModal = false;
		vm.newScheduleData = null;
		vm.language = navigator.language || navigator.userLanguage;

		vm.addScheduleJob = addScheduleJob;
		vm.toggleScheduleJob = toggleScheduleJob;
		vm.deleteScheduleJob = deleteScheduleJob;
		vm.editScheduleJob = editScheduleJob;


		(function init() {
			getScheduledJobs();
		})();

		function getScheduledJobs() {
			vm.schedules = null;
			$http
				.get("./Etl/ScheduledJobs")
				.then(function (response) {
					vm.schedules = response.data;
				});
		}

		function toggleScheduleJob(scheduleId) {
			$http
				.post("./Etl/ToggleScheduleJob",
					JSON.stringify(scheduleId)
				)
				.then(function (response) {
					getScheduledJobs();
				});
		}

		function deleteScheduleJob(scheduleId) {
			$http
				.post("./Etl/DeleteScheduleJob",
					JSON.stringify(scheduleId)
				)
				.then(function (response) {
					getScheduledJobs();
				});
		}

		function buildRelativePeriods(name, start, end, destination) {
			if (angular.isDefined(start) && angular.isDefined(end)) {
				destination.push(
					{
						JobCategoryName: name,
						Start: start,
						End: end
					}
				);
			} else {
				return;
			}
		}

		function addScheduleJob(form) {
			vm.showAddScheduleJobModal = false;
			form.RelativePeriods = [];

			if (angular.isDefined(form.InitialPeriod)) {
				buildRelativePeriods('Initial', form.InitialPeriod.Start, form.InitialPeriod.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.QueueStats)) {
				buildRelativePeriods('QueueStatistics', form.QueueStats.Start, form.QueueStats.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.AgentStats)) {
				buildRelativePeriods('AgentStatistics', form.AgentStats.Start, form.AgentStats.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.Schedule)) {
				buildRelativePeriods('Schedule', form.Schedule.Start, form.Schedule.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.Forecast)) {
				buildRelativePeriods('Forecast', form.Forecast.Start, form.Forecast.End, form.RelativePeriods);
			}

			var logdataId;
			if (form.LogDataSourceId) {
				logdataId = form.LogDataSourceId.Id;
			} else {
				logdataId = null;
			}

			var postObj = {
				ScheduleName: form.ScheduleName,
				Description: form.Description,
				JobName: form.JobName.JobName,
				Enabled: form.Enabled,
				Tenant: form.Tenant.TenantName,
				DailyFrequencyStart: form.DailyFrequencyStart,
				DailyFrequencyEnd: form.DailyFrequencyEnd,
				DailyFrequencyMinute: form.DailyFrequencyMinute,
				RelativePeriods: form.RelativePeriods,
				LogDataSourceId: logdataId,
				ScheduleId: -1
			}

			$http
				.post(
					"./Etl/ScheduleJob",
					JSON.stringify(postObj)
				)
				.then(function (response) {
					getScheduledJobs();
				});
		}

		function editScheduleJob(form) {
			vm.showEditScheduleJobModal = false;
			form.RelativePeriods = [];

			if (angular.isDefined(form.InitialPeriod)) {
				buildRelativePeriods('Initial', form.InitialPeriod.Start, form.InitialPeriod.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.QueueStats)) {
				buildRelativePeriods('QueueStatistics', form.QueueStats.Start, form.QueueStats.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.AgentStats)) {
				buildRelativePeriods('AgentStatistics', form.AgentStats.Start, form.AgentStats.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.Schedule)) {
				buildRelativePeriods('Schedule', form.Schedule.Start, form.Schedule.End, form.RelativePeriods);
			}

			if (angular.isDefined(form.Forecast)) {
				buildRelativePeriods('Forecast', form.Forecast.Start, form.Forecast.End, form.RelativePeriods);
			}

			var logdataId;
			if (form.LogDataSourceId) {
				logdataId = form.LogDataSourceId.Id;
			} else {
				logdataId = null
			}

			var postObj = {
				ScheduleName: form.ScheduleName,
				Description: form.Description,
				JobName: form.JobName.JobName,
				Enabled: form.Enabled,
				Tenant: form.Tenant.TenantName,
				DailyFrequencyStart: form.DailyFrequencyStart,
				DailyFrequencyEnd: form.DailyFrequencyEnd,
				DailyFrequencyMinute: form.DailyFrequencyMinute,
				RelativePeriods: form.RelativePeriods,
				LogDataSourceId: logdataId,
				ScheduleId: form.Id
			}

			$http
				.post(
					"./Etl/EditScheduleJob",
					JSON.stringify(postObj)
				)
				.then(function (response) {
					getScheduledJobs();
				});
		}
	}
})();
