﻿(function() {
	"use strict";

	angular
		.module("adminApp")
		.controller("summaryController", summaryController, ["tokenHeaderService"]);

	function summaryController($http, tokenHeaderService, $interval, $scope) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = "Stardust Summary";
		vm.NodeError = "";
		vm.JobError = "";
		vm.triggerResourceCalculation = triggerResourceCalculation;
		vm.refreshPayrollFormats = refreshPayrollFormats;
		vm.intradayToolGoWithTheFlow = intradayToolGoWithTheFlow;
		vm.healthCheck = healthCheck;
		vm.selectTenant = selectTenant;
		vm.anyQueuedJobs = vm.anyFailedJobs = vm.anyHistory = vm.anyNodes = false;
		vm.showFailureAlert = vm.showNodesAlert = vm.showHistoryAlert = vm.showHealthAlert = false;
		vm.result = "";
		vm.showRefreshPayrollFormats = false;
		vm.queueCount = 0;
		vm.showIntradayTool = false;
		refresh();

		var refreshPromise = $interval(refresh, 3000);

		$scope.$on("$destroy",
			function() {
				$interval.cancel(refreshPromise);
			});

		$http.get("./Toggle/IsEnabled",
				{
					params: { toggle: "Wfm_Payroll_SupportMultiDllPayrolls_75959" }
				},
				tokenHeaderService.getHeaders())
			.then(function(data) {
				vm.showRefreshPayrollFormats = data.data;
			});

	$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.Tenants = data;
			});

		$http.get("./Stardust/ShowIntradayTool", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.showIntradayTool = data;
			});

		function cancelPollingAndShowExpiredDialog() {
			if (refreshPromise !== null) {
				$interval.cancel(refreshPromise);
				refreshPromise = null;
				window.alert("Your session has expired, please login again");
			}
		}

		function refresh() {
			$http.get("./Stardust/Jobs", tokenHeaderService.getHeadersAndParams({ "from": 1, "to": 5}))
				.success(function(data) {
					vm.RunningJobs = data;
					if (data.length > 0) {
						vm.anyHistory = true;
						vm.showHistoryAlert = false;
					} else {
						vm.anyHistory = false;
						vm.showHistoryAlert = true;
					}
				})
				.error(function(xhr, ajaxOptions) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});

			$http.get("./Stardust/FailedJobs", tokenHeaderService.getHeadersAndParams({ "from": 1, "to": 5}))
				.success(function(data) {
					vm.FailedJobs = data;
					if (data.length > 0) {
						vm.anyFailedJobs = true;
						vm.showFailureAlert = true;
					} else {
						vm.anyFailedJobs = false;
						vm.showFailureAlert = false;
					}
				})
				.error(function(xhr, ajaxOptions) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});

			$http.get("./Stardust/QueuedJobs", tokenHeaderService.getHeadersAndParams({ "from": 1, "to": 5 }))
				.success(function(data) {
					vm.QueuedJobs = data;
					if (data.length > 0) {
						vm.anyQueuedJobs = true;
					} else {
						vm.anyQueuedJobs = false;
					}
				})
				.error(function(xhr, ajaxOptions) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});

			$http.get("./Stardust/QueueCount", tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.queueCount = data;
				});

			$http.get("./Stardust/AliveWorkerNodes", tokenHeaderService.getHeaders())
				.success(function(data) {
					vm.WorkerNodes = data;
					if (data.length > 0) {
						vm.anyNodes = true;
						vm.showNodesAlert = false;
					} else {
						vm.anyNodes = false;
						vm.showNodesAlert = true;
					}
				})
				.error(function(xhr, ajaxOptions) {
					vm.NodeError = ajaxOptions;
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					if (xhr !== "") {
						vm.NodeError = vm.NodeError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});
		}

		function selectTenant(name) {
			vm.selectedTenantName = name;
		}

		function triggerResourceCalculation() {
			if (!vm.selectedTenantName) return;
			$http.post("./Stardust/TriggerResourceCalculation",
				{
					"Tenant": vm.selectedTenantName,
					"Days": 14
				},
				tokenHeaderService.getHeaders()
			).success(function() {
				refresh();
			});
		}

		function healthCheck() {
			vm.result = "Running...";
			vm.showHealthAlert = false;
			$http.get("./Stardust/HealthCheck",
					tokenHeaderService.getHeaders())
				.success(function(data) {
					vm.result = data;
					if (data !== "Everything looks OK!")
						vm.showHealthAlert = true;
					refresh();
				})
				.error(function() {
					vm.result = "Something is wrong but we can't figure out what!";
					vm.showHealthAlert = true;
				});
		}

		function refreshPayrollFormats() {
			if (!vm.selectedTenantName) return;
			$http.post("./Stardust/RefreshPayrollFormats",
				{
					"Tenant": vm.selectedTenantName
				},
				tokenHeaderService.getHeaders()
			).success(function () {
				refresh();
			});
		}

		function intradayToolGoWithTheFlow() {
			$http.get("./Stardust/IntradayToolGoWithTheFlow",
				tokenHeaderService.getHeaders()
			).success(function () {
				refresh();
			});
		}
	}


})();