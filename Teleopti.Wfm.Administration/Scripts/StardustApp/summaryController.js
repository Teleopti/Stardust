(function() {
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
		vm.result = "";
		$http.get("./Stardust/Jobs/1/5", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.RunningJobs = data;
			})
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
		}

		function healthCheck() {
			vm.result = "running...";
			$http.get("./Stardust/HealthCheck",
					tokenHeaderService.getHeaders())
				.success(function(data) {
					vm.result = data;
				})
				.error(function() {
					vm.result = "Something is wrong but we can't figure out what!";
				});
		}
	}


})();