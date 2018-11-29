(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("jobFailedController", jobFailedController, ["tokenHeaderService"]);

	function jobFailedController($http, $interval, tokenHeaderService, $scope) {
		/* jshint validthis:true */

		var vm = this;
		vm.title = "Stardust Failed Jobs";
		vm.NodeError = "";
		vm.JobError = "";
		vm.back = back;
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.moreJobs = true;
		vm.selectTenant = selectTenant;
		vm.selectJobType = selectJobType;
		vm.search = search;
		vm.Jobs = [];
		var allTenantsString = "All Tenants";
		var allTypesString = "All Types";
		vm.dataSourceFilter = allTenantsString;
		vm.jobTypeFilter = allTypesString;
		var refreshInterval = 5000;

		getJobsByFilter();

		var refreshPromise = $interval(pollNewData, refreshInterval);

		$http.get("./Stardust/Types", tokenHeaderService.getHeaders())
			.then(function (data) {
				vm.types = data;
			});

		$scope.$on("$destroy",
			function () {
				$interval.cancel(refreshPromise);
			});

		$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.then(function (data) {
				vm.Tenants = data;
			});

		$http.get("./Stardust/OldestJob", tokenHeaderService.getHeaders())
			.then(function (data) {
				var oldestJob = data;
				vm.minFrom = new Date(oldestJob.Created);
				vm.maxTo = new Date(new Date());
				vm.selectedFromDate = vm.minFrom;
				vm.selectedToDate = vm.maxTo;
			});

		function cancelPollingAndShowExpiredDialog() {
			if (refreshPromise !== null) {
				$interval.cancel(refreshPromise);
				refreshPromise = null;
				window.alert("Your session has expired, please login again");
			}
		}

		function getJobsByFilter() {
			var dataSource = null;
			if (vm.dataSourceFilter !== allTenantsString) {
				dataSource = vm.dataSourceFilter;
			}

			var jobType = null;
			if (vm.jobTypeFilter !== allTypesString) {
				jobType = vm.jobTypeFilter;
			}

			var params = { "from": vm.resultsFrom, "to": vm.resultsTo, "dataSource": dataSource, "type": jobType, "fromdate": vm.fromDateFilter, "todate": vm.toDateFilter};
			$http.get("./Stardust/FailedJobs", tokenHeaderService.getHeadersAndParams(params))
				.then(function (data) {
					if (data.length < vm.resultsTo) {
						vm.moreJobs = false;
					} else {
						vm.moreJobs = true;
					}
					vm.Jobs = data;
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});

		}

		function pollNewData() {
			var tmpFrom = vm.resultsFrom;
			vm.resultsFrom = 1;
			getJobsByFilter();
			vm.resultsFrom = tmpFrom;
		}

		function back() {
			window.history.back();
		}

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
			vm.fromDateFilter = vm.selectedFromDate;
			vm.toDateFilter = vm.selectedToDate;
			pollNewData();
		}
	}
})();
