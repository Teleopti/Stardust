(function() {
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
		vm.healthCheck = healthCheck;
		vm.selectTenant = selectTenant;
		vm.anyQueuedJobs = vm.anyFailedJobs = vm.anyHistory = vm.anyNodes = false;
		vm.showFailureAlert = vm.showNodesAlert = vm.showHistoryAlert = vm.showHealthAlert = false;
		vm.result = "";
		refresh();

		var refreshPromise = $interval(refresh, 3000);

		$scope.$on("$destroy",
			function() {
				$interval.cancel(refreshPromise);
			});

		$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.Tenants = data;
			});

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
	}


})();