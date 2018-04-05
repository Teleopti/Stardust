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
		vm.showAddScheduleJobModal = false;
		vm.newScheduleData = null;
		vm.language = navigator.language || navigator.userLanguage;

		vm.addScheduleJob = addScheduleJob;

		(function init() {
			getScheduledJobs();
		})();

		function getScheduledJobs() {
			vm.schedules = null;
			$http
			.get("./Etl/ScheduledJobs", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.schedules = data;
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
				)
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
			if (form.LogDataSourceId.Id) {
				logdataId = form.LogDataSourceId.Id;
			} else{
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
				ScheduleId: -1
			}

			console.log(postObj);

			$http
			.post(
				"./Etl/ScheduleJob",
				JSON.stringify(postObj),
				tokenHeaderService.getHeaders()
			)
			.success(function(data) {
				getScheduledJobs();
				console.log(data);
			});
		}
	}
})();
