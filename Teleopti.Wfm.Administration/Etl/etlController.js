(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("etlController", etlController, ["$http", "$timeout", "$window"]);

	function etlController($http, $timeout, $window) {
		var vm = this;

		vm.state = null;
		vm.jobs = [];
		vm.tenants = [];
		vm.selectedTenant = "";
		vm.selectedJob = null;
		vm.dataSources = null;
		vm.masterTenantConfigured = false;
		vm.selectDataSource = null;

		vm.getConfigStatus = getConfigStatus;
		vm.getJobs = getJobs;
		vm.getTenants = getTenants;
		vm.sendTenant = sendTenant;
		vm.selectedTenantChanged = selectedTenantChanged;
		vm.selectJob = selectJob;
		vm.enqueueJob = enqueueJob;

		var today = new Date();
		vm.dataSources = [];

		vm.manualInitial = {
			StartDate: null,
			EndDate: null
		};
		vm.manualQueueStats = {
			StartDate: null,
			EndDate: null
		};
		vm.manualAgentStats = {
			StartDate: null,
			EndDate: null
		};
		vm.manualSchedule = {
			StartDate: null,
			EndDate: null
		};
		vm.manualForecast = {
			StartDate: null,
			EndDate: null
		};

		vm.applyToAll = function (param, input) {
			if (vm.selectedJob.Initial) {
				vm.manualInitial[param] = input;
			}

			if (vm.selectedJob.QueueStatistics) {
				vm.manualQueueStats[param] = input;
			}

			if (vm.selectedJob.AgentStatistics) {
				vm.manualAgentStats[param] = input;
			}

			if (vm.selectedJob.Schedule) {
				vm.manualSchedule[param] = input;
			}

			if (vm.selectedJob.Forecast) {
				vm.manualForecast[param] = input;
			}
		};

		vm.getManualData = function () {
			vm.getTenants();
			vm.getConfigStatus();
			vm.language = navigator.language || navigator.userLanguage;
		};
		vm.getManualData();

		function selectJob(job) {
			vm.selectedJob = job;
			for (var i = 0; i < vm.selectedJob.NeededDatePeriod.length; i++) {
				vm.selectedJob[vm.selectedJob.NeededDatePeriod[i]] = true;
			}
			if (!vm.selectedJob.NeedsParameterDataSource) {
				vm.selectDataSource = null;
			} else {
				vm.selectDataSource = vm.dataSources[0];
			}

			if (!vm.selectedJob.Initial) {
				setDateInput(vm.manualInitial, null);
			} else {
				setDateInput(vm.manualInitial, today);
			}
			if (!vm.selectedJob.QueueStatistics) {
				setDateInput(vm.manualQueueStats, null);
			} else {
				setDateInput(vm.manualQueueStats, today);
			}
			if (!vm.selectedJob.AgentStatistics) {
				setDateInput(vm.manualAgentStats, null);
			} else {
				setDateInput(vm.manualAgentStats, today);
			}
			if (!vm.selectedJob.Schedule) {
				setDateInput(vm.manualSchedule, null);
			} else {
				setDateInput(vm.manualSchedule, today);
			}
			if (!vm.selectedJob.Forecast) {
				setDateInput(vm.manualForecast, null);
			} else {
				setDateInput(vm.manualForecast, today);
			}
		}

		function setDateInput(data, value) {
			data.StartDate = value;
			data.EndDate = value;
		}

		function getJobs(tenant) {
			$http
				.post(
					"./Etl/Jobs",
					JSON.stringify(tenant)
				)
				.then(function (response) {
					vm.jobs = response.data;
				})
				.catch(function (response) {
					vm.jobs = [];
				});
		}

		function getTenants() {
			vm.tenants = [];
			$http
				.get("./Etl/GetTenants")
				.then(function (response) {
					for (var i = 0; i < response.data.length; i++) {
						if (response.data[i].IsBaseConfigured) {
							vm.tenants.push(response.data[i]);
						} else {
							vm.unconfigured = true;
						}
					}
					vm.tenants.unshift({
						TenantName: '<All>'
					});

					if (!$window.sessionStorage.tenant) {
						vm.selectedTenant = vm.tenants[0];
						selectedTenantChanged();
					} else {
						vm.selectedTenant = getItemBasedOnName(vm.tenants, $window.sessionStorage.tenant, "TenantName");
						selectedTenantChanged();
					}
				});
		}

		function getItemBasedOnName(arr, name, prop) {
			for (var i = 0; i < arr.length; i++) {
				if (arr[i][prop] === name) {
					return arr[i];
				}
			}
		}

		function getConfigStatus() {
			$http
				.get("./Etl/IsBaseConfigurationAvailable")
				.then(function (response) {
					vm.masterTenant = {
						IsBaseConfigured: response.data.IsBaseConfigured,
						TenantName: response.data.TenantName
					};
				});
		}

		function selectedTenantChanged() {
			$window.sessionStorage.tenant = vm.selectedTenant.TenantName;
			vm.sendTenant(vm.selectedTenant.TenantName);
			vm.getJobs(vm.selectedTenant.TenantName);
			vm.selectDataSource = null;
		}

		function sendTenant(tenant) {
			$http
				.post(
					"./Etl/TenantValidLogDataSources",
					JSON.stringify(tenant)
				)
				.then(function (response) {
					vm.dataSources = response.data;
				});
		}

		function enqueueJob(job) {

			if (vm.selectDataSource === null && job.NeedsParameterDataSource) {
				vm.selectDataSource = { Id: -2 };
			} else if (vm.selectDataSource === null && !job.NeedsParameterDataSource) {
				vm.selectDataSource = { Id: null };
			}

			var data = {
				JobName: job.JobName,
				JobPeriods: [],
				LogDataSourceId: vm.selectDataSource.Id,
				TenantName: vm.selectedTenant.TenantName
			};

			for (var i = 0; i < job.NeededDatePeriod.length; i++) {
				var dates = {
					Start: null,
					End: null
				};

				if (job.NeededDatePeriod[i] === "Initial") {
					dates.Start = vm.manualInitial.StartDate;
					dates.End = vm.manualInitial.EndDate;
				}
				if (job.NeededDatePeriod[i] === "QueueStatistics") {
					dates.Start = vm.manualQueueStats.StartDate;
					dates.End = vm.manualQueueStats.EndDate;
				}
				if (job.NeededDatePeriod[i] === "AgentStatistics") {
					dates.Start = vm.manualAgentStats.StartDate;
					dates.End = vm.manualAgentStats.EndDate;
				}
				if (job.NeededDatePeriod[i] === "Schedule") {
					dates.Start = vm.manualSchedule.StartDate;
					dates.End = vm.manualSchedule.EndDate;
				}
				if (job.NeededDatePeriod[i] === "Forecast") {
					dates.Start = vm.manualForecast.StartDate;
					dates.End = vm.manualForecast.EndDate;
				}
				data.JobPeriods.push({
					Start: dates.Start,
					End: dates.End,
					JobCategoryName: job.NeededDatePeriod[i]
				});
			}

			$http
				.post("./Etl/EnqueueJob", data)
				.then(function () {
					job.Status = "Job enqueued";
					$timeout(function () {
						job.Status = null;
					}, 5000);
				})
				.catch(function () {
					job.Status = "Failed. Check inputs and network";
					$timeout(function () {
						job.Status = null;
					}, 5000);
				});
		}

	}
})();
