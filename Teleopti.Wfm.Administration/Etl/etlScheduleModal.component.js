(function () {
	'use strict';

	angular.module('adminApp')
		.component('etlScheduleModal',
			{
				templateUrl: './Etl/scheduleModal.html',
				controller: ['$http', 'tokenHeaderService', etlScheduleModal],
				controllerAs: 'ctrl',
				bindings: {
					job: '<',
					output: '=',
					callback: '='
				}
			});

	function etlScheduleModal($http, tokenHeaderService) {
		var ctrl = this;

		ctrl.selectedTenantChanged = selectedTenantChanged;
		ctrl.toggleFrequencyType = toggleFrequencyType;
		ctrl.selectedjobChanged = selectedjobChanged;
		ctrl.isRelativePeriodNeeded = isRelativePeriodNeeded;
		ctrl.frequencyType = false;
		ctrl.tenantLogData = [];

		(function init() {
			getTenantsForModal();
		})();

		function getJobs(tenant) {
			$http
				.post(
					"./Etl/Jobs",
					JSON.stringify(tenant),
					tokenHeaderService.getHeaders()
				)
				.then(function (response) {
					ctrl.jobs = response.data;
				})
				.catch(function (response) {
					ctrl.jobs = [];
				});
		}

		function getTenantsForModal() {
			ctrl.tenants = [];
			$http
				.get("./Etl/GetTenants", tokenHeaderService.getHeaders())
				.then(function (response) {
					for (var i = 0; i < response.data.length; i++) {
						if (response.data[i].IsBaseConfigured) {
							ctrl.tenants.push(response.data[i]);
						}
					}
					ctrl.tenants.unshift({
						TenantName: '<All>'
					});
					if (ctrl.job) {
						editHandler();
						return;
					}
					ctrl.form = {
						DailyFrequencyStart: moment().format("HH:mm")
					};

					ctrl.selectedTenant = ctrl.tenants[0];
					selectedTenantChanged(ctrl.selectedTenant);
				});
		}

		function toggleFrequencyType(form) {
			if (ctrl.frequencyType) {
				form.DailyFrequencyStart = null;
				ctrl.form.DailyFrequencyStart = moment().format("HH:mm");
				ctrl.form.DailyFrequencyEnd = moment().add(1, 'hours').format("HH:mm");
			}
			else {
				form.DailyFrequencyMinute = null;
				form.DailyFrequencyStart = null;
				form.DailyFrequencyEnd = null;
			}
		}

		function getLogDataForATenant(tenant) {
			ctrl.tenantLogData = null;
			$http
				.post(
					"./Etl/TenantAllLogDataSources",
					JSON.stringify(tenant),
					tokenHeaderService.getHeaders()
				)
				.then(function (response) {
					ctrl.tenantLogData = response.data;
				});
		}

		function selectedTenantChanged(tenant) {
			ctrl.selectedTenant = tenant;
			getJobs(ctrl.selectedTenant.TenantName);
			getLogDataForATenant(ctrl.selectedTenant.TenantName);
		}

		function selectedjobChanged(form) {
			form.LogDataSourceId = null;
			form.QueueStats = {};
			form.Schedule = {};
			form.InitialPeriod = {};
			form.Forecast = {};
			form.AgentStats = {};
		}

		function isRelativePeriodNeeded(name, arr) {
			if (!arr) {
				return true;
			}
			return !arr.includes(name);
		}

		function getItemBasedOnName(arr, name, prop) {
			for (var i = 0; i < arr.length; i++) {
				if (arr[i][prop] === name) {
					return arr[i];
				}
			}
		}

		function handleDynamicEditValues() {
			ctrl.form.ScheduleName = ctrl.job.ScheduleName;
			ctrl.form.Tenant = getItemBasedOnName(ctrl.tenants, ctrl.job.Tenant, 'TenantName');
			$http
				.post(
					"./Etl/Jobs",
					JSON.stringify(ctrl.form.Tenant.TenantName),
					tokenHeaderService.getHeaders()
				)
				.then(function (response) {
					ctrl.jobs = response.data;
					ctrl.form.JobName = getItemBasedOnName(ctrl.jobs, ctrl.job.JobName, 'JobName');
				});

			$http
				.post(
					"./Etl/TenantAllLogDataSources",
					JSON.stringify(ctrl.form.Tenant.TenantName),
					tokenHeaderService.getHeaders()
				)
				.then(function (response) {
					ctrl.tenantLogData = response.data;
					ctrl.form.LogDataSourceId = getItemBasedOnName(ctrl.tenantLogData, ctrl.job.LogDataSourceId, 'Id');
					handleRelativePeriods();
				});
		}

		function handleRelativePeriods() {
			ctrl.form.Schedule = getItemBasedOnName(ctrl.job.RelativePeriods, "Schedule", 'JobCategoryName');
			ctrl.form.QueueStats = getItemBasedOnName(ctrl.job.RelativePeriods, "QueueStatistics", 'JobCategoryName');
			ctrl.form.Forecast = getItemBasedOnName(ctrl.job.RelativePeriods, "Forecast", 'JobCategoryName');
			ctrl.form.AgentStats = getItemBasedOnName(ctrl.job.RelativePeriods, "AgentStatistics", 'JobCategoryName');
			ctrl.form.InitialPeriod = getItemBasedOnName(ctrl.job.RelativePeriods, "Initial", 'JobCategoryName');
		}

		function editHandler() {
			ctrl.form = {
				DailyFrequencyEnd: moment(ctrl.job.DailyFrequencyEnd).format("HH:mm"),
				DailyFrequencyStart: moment(ctrl.job.DailyFrequencyStart).format("HH:mm"),
				Description: ctrl.job.Description,
				JobName: ctrl.job.jobName,
				LogDataSourceId: null,
				Enabled: ctrl.job.Enabled,
				Id: ctrl.job.ScheduleId,
				QueueStats: {},
				Schedule: {},
				InitialPeriod: {},
				Forecast: {},
				AgentStats: {}
			}
			handleDynamicEditValues();
			if (ctrl.job.DailyFrequencyMinute) {
				ctrl.form.DailyFrequencyMinute = angular.fromJson(ctrl.job.DailyFrequencyMinute);
				ctrl.frequencyType = true;
			} else {
				ctrl.form.DailyFrequencyMinute = null;
			}
		}

	}
})();
