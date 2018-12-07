(function () {
	'use strict';

	angular.module('adminApp')
		.directive('timeValidationMax', function () {
			return {
				require: 'ngModel',
				link: function (scope, element, attr, ctrl) {
					function myValidation(value) {
						scope.$watch('ctrl.form.DailyFrequencyEnd',
							function (value) {
								if (angular.isUndefined(ctrl.$error.pattern)) {
									//no pattern error
									if (angular.isDefined(value) && value !== null) {
										var hour = value.split(":")[0];
										var minutes = value.split(":")[1];

										var endDate = new Date(2018, 12, 24, hour, minutes);

										hour = ctrl.$viewValue.split(":")[0];
										minutes = ctrl.$viewValue.split(":")[1];

										var startDate = new Date(2018, 12, 24, hour, minutes);

										ctrl.$setValidity('invalidStartTime', startDate < endDate);
									}
								}
							});
						
						return value;
					}
					ctrl.$parsers.push(myValidation);
				}
			};
		})
		.directive('timeValidationMin', function () {
			return {
				require: 'ngModel',
				link: function (scope, element, attr, ctrl) {
					function myValidation(value) {
						scope.$watch('ctrl.form.DailyFrequencyStart',
							function (value) {
								if (angular.isUndefined(ctrl.$error.pattern)) {
									//no pattern error
									if (angular.isDefined(value) && value !== null) {
										var hour = value.split(":")[0];
										var minutes = value.split(":")[1];

										var startDate = new Date(2018, 12, 24, hour, minutes);

										hour = ctrl.$viewValue.split(":")[0];
										minutes = ctrl.$viewValue.split(":")[1];

										var endDate = new Date(2018, 12, 24, hour, minutes);

										ctrl.$setValidity('invalidEndTime', startDate < endDate);
									}
								}
							});

						return value;
					}
					ctrl.$parsers.push(myValidation);
				}
			};
		})
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
				.success(function (data) {
					ctrl.jobs = data;
				})
				.error(function (data) {
					ctrl.jobs = [];
				});
		}

		function getTenantsForModal() {
			ctrl.tenants = [];
			$http
				.get("./Etl/GetTenants", tokenHeaderService.getHeaders())
				.success(function (data) {
					for (var i = 0; i < data.length; i++) {
						if (data[i].IsBaseConfigured) {
							ctrl.tenants.push(data[i]);
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
						DailyFrequencyStart: moment(new Date()).format("HH:mm"),
						DailyFrequencyEnd: moment(new Date()).format("HH:mm")
					};

					ctrl.selectedTenant = ctrl.tenants[0];
					selectedTenantChanged(ctrl.selectedTenant);
				});
		}

		function toggleFrequencyType(form) {
			if (ctrl.frequencyType) {
				form.DailyFrequencyStart = moment().format("HH:mm");
				form.DailyFrequencyEnd = moment().add(1, 'hours').format("HH:mm");
			}
			else {
				form.DailyFrequencyMinute = null;
				//form.DailyFrequencyStart = null;
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
				.success(function (data) {
					ctrl.tenantLogData = data;
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

			if (arr.indexOf(name) >= 0) {
				return false;
			}
			return true;
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
				.success(function (data) {
					ctrl.jobs = data;
					ctrl.form.JobName = getItemBasedOnName(ctrl.jobs, ctrl.job.JobName, 'JobName');
				});

			$http
				.post(
					"./Etl/TenantAllLogDataSources",
					JSON.stringify(ctrl.form.Tenant.TenantName),
					tokenHeaderService.getHeaders()
				)
				.success(function (data) {
					ctrl.tenantLogData = data;
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
				DailyFrequencyStart: moment(ctrl.job.DailyFrequencyStart).format("HH:mm"),
				DailyFrequencyEnd: moment(ctrl.job.DailyFrequencyEnd).format("HH:mm"),
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
			};

			handleDynamicEditValues();

			if (ctrl.job.DailyFrequencyMinute) {
				ctrl.form.DailyFrequencyMinute = angular.fromJson(ctrl.job.DailyFrequencyMinute);
				ctrl.frequencyType = true;
			} else {
				ctrl.form.DailyFrequencyMinute = null;
			}
		}

	}
}
)();