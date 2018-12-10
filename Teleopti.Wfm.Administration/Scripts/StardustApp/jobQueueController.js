(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("jobQueueController", jobQueueController, ["tokenHeaderService"]);

	function jobQueueController($http, $interval, tokenHeaderService, $scope) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = "Stardust Queue";
		vm.NodeError = "";
		vm.JobError = "";
		vm.back = back;
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.noMoreJobs = false;

		vm.getNewFreshData = getNewFreshData;
		vm.toggle = toggle;
		vm.exists = exists;
		vm.isIndeterminate = isIndeterminate;
		vm.deleteQueuedJobs = deleteQueuedJobs;
		vm.isChecked = isChecked;
		vm.toggleAll = toggleAll;
		vm.Jobs = [];
		vm.selected = [];
		var allTenantsString = "All Tenants";
		var allTypesString = "All Types";
		var refreshInterval = 5000;
		vm.selectTenant = selectTenant;
		vm.dataSourceFilter = allTenantsString;
		vm.selectJobType = selectJobType;
		
		vm.search = search;

		getJobsByFilter();

		var refreshPromise = $interval(pollNewData, refreshInterval);

		$scope.$on("$destroy",
			function () {
				$interval.cancel(refreshPromise);
			});

		$http.get("./Stardust/QueueTypes", tokenHeaderService.getHeaders())
			.then(function (response) {
				vm.types = response.data;
			});

		$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.then(function (response) {
				vm.Tenants = response.data;
			});

		function getJobsByFilter() {
			var dataSource = null;
			if (vm.dataSourceFilter !== allTenantsString) {
				dataSource = vm.dataSourceFilter;
			}

			var jobType = null;
			if (vm.jobTypeFilter !== allTypesString) {
				jobType = vm.jobTypeFilter;
			}

			var params = { "from": vm.resultsFrom, "to": vm.resultsTo, "dataSource": dataSource, "type": jobType };
			$http.get("./Stardust/QueuedJobs", tokenHeaderService.getHeadersAndParams(params))
				.then(function (response) {
					if (response.data.length < vm.resultsTo) {
						vm.moreJobs = false;
					} else {
						vm.moreJobs = true;
					}
					vm.Jobs = response.data;
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});

		}

		function cancelPollingAndShowExpiredDialog() {
			if (refreshPromise !== null) {
				$interval.cancel(refreshPromise);
				refreshPromise = null;
				window.alert("Your session has expired, please login again");
			}
		}

		function getNewFreshData() {
			vm.resultsFrom += vm.limit;
			vm.resultsTo += vm.limit;
			getJobs(true);
		}

		function toggle(item, list) {
			var idx = list.indexOf(item);
			if (idx > -1) {
				list.splice(idx, 1);
			}
			else {
				list.push(item);
			}
		};

		function exists(item, list) {
			return list.indexOf(item) > -1;
		};

		function isIndeterminate() {
			return (vm.selected.length !== 0 &&
				vm.selected.length !== vm.Jobs.length);
		};

		function isChecked() {
			if (vm.Jobs.length === 0) return false;
			return vm.selected.length === vm.Jobs.length;
		};

		function toggleAll() {
			if (vm.selected.length === vm.Jobs.length) {
				vm.selected = [];
			} else if (vm.selected.length === 0 || vm.selected.length > 0) {
				vm.selected = [];
				vm.Jobs.forEach(function (job) {
					vm.selected.push(job.JobId);
				});
			}
		};

		function deleteQueuedJobs() {
			$http.post("./Stardust/DeleteQueuedJobs",vm.selected, tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.selected = [];
					vm.Jobs = [];
					vm.resultsFrom = 1;
					getJobs();
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
				});
		};
		function pollNewData() {
			var tmpFrom = vm.resultsFrom;
			vm.resultsFrom = 1;
			getJobsByFilter();
			vm.resultsFrom = tmpFrom;
		}

		function back() {
			window.history.back();
		};

		function selectTenant(name) {
			vm.selectedDataSource = name;
		}

		function selectJobType(name) {
			vm.selectedJobType = name;
		}

		function search() {
			vm.dataSourceFilter = vm.selectedDataSource;
			vm.jobTypeFilter = vm.selectedJobType;
			vm.resultsTo = vm.limit;
			pollNewData();
		}
	}
})();
