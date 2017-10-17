﻿(function() {
	"use strict";

	angular
		.module("adminApp")
		.controller("summaryController", summaryController, ["tokenHeaderService"]);

	function summaryController($http, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = "Stardust Summary";
		vm.NodeError = "";
		vm.JobError = "";
		vm.triggerResourceCalculation = triggerResourceCalculation;
		vm.healthCheck = healthCheck;
		vm.selectTenant = selectTenant;
		vm.displayFailedJobs = vm.displayQueuedJobs = vm.displayNodes = vm.displayHistory = "none";
		vm.showFailureAlert = vm.showNodesAlert = vm.showHistoryAlert = vm.showHealthAlert = "none";
		vm.noHistoryMessage = vm.noFailedJobsMessage = vm.noQueuedJobsMessage = vm.noNodesMessage = "";
		vm.result = "";

		$http.get("./Stardust/Jobs/1/5", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.RunningJobs = data;
				if (data.length > 0) {
					vm.displayHistory = "";
				} else {
					vm.noHistoryMessage = "No jobs have been processed during the last 7 days!";
					vm.showHistoryAlert = "";
				}})
			.error(function(xhr, ajaxOptions) {
				console.log(xhr.Message + ": " + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
				}
			});

		$http.get("./Stardust/FailedJobs/1/5", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.FailedJobs = data;
				if (data.length > 0) {
					vm.displayFailedJobs = "";
					vm.showFailureAlert = "";
				} else
					vm.noFailedJobsMessage = "No job failures to show!";
			})
			.error(function(xhr, ajaxOptions) {
				console.log(xhr.Message + ": " + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
				}
			});

		$http.get("./Stardust/QueuedJobs/1/5", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.QueuedJobs = data;
				if (data.length > 0) {
					vm.displayQueuedJobs = "";
				} else
					vm.noQueuedJobsMessage = "Job queue is empty!";
			})
			.error(function(xhr, ajaxOptions) {
				console.log(xhr.Message + ": " + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
				}
			});

		$http.get("./Stardust/AliveWorkerNodes", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.WorkerNodes = data;
				if (data.length > 0) {
					vm.displayNodes = "";
				} else {
					vm.noNodesMessage = "No nodes are running. Run the health check to figure out why.";
					vm.showNodesAlert = "";
				}
			})
			.error(function(xhr, ajaxOptions) {
				vm.NodeError = ajaxOptions;
				console.log(xhr.Message + ": " + xhr.ExceptionMessage);
				if (xhr !== "") {
					vm.NodeError = vm.NodeError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
				}

			});
		$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.Tenants = data;
			});

		function selectTenant(name) {
			vm.selectedTenantName = name;

		};

		function triggerResourceCalculation() {
			if (!vm.selectedTenantName) return;
			$http.post("./Stardust/TriggerResourceCalculation",
				{
					"Tenant": vm.selectedTenantName,
					"Days": 14
				},
				tokenHeaderService.getHeaders()
			);
			window.location.reload();
		}

		function healthCheck() {
			vm.result = "Running...";
			$http.get("./Stardust/HealthCheck",
					tokenHeaderService.getHeaders())
				.success(function(data) {
					vm.result = data;
					if (data !== "Everything looks OK!")
						vm.showHealthAlert = "";
				})
				.error(function() {
					vm.result = "Something is wrong but we can't figure out what!";
					vm.showHealthAlert = "";
				});
		}
	}


})();