(function() {
	"use strict";

	angular
		.module("adminApp")
		.controller("jobController", jobController, ["tokenHeaderService"]);

	function jobController($http, $interval, tokenHeaderService, $scope) {
		/* jshint validthis:true */

		var vm = this;
		vm.title = "Stardust Jobs";
		vm.NodeError = "";
		vm.JobError = "";
		vm.back = back;
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.moreJobs = true;
		vm.getMoreData = getMoreData;
		vm.selectTenant = selectTenant;
		vm.selectJobType = selectJobType;
		vm.search = search;
		vm.Jobs = [];
		var allTenantsString = "All Tenants";
		var allTypesString = "All Types";
		vm.selectedJobType = allTypesString;
		vm.selectedDataSource = allTenantsString;
		vm.dataSourceFilter = allTenantsString;
		vm.jobTypeFilter = allTypesString;
		var refreshInterval = 5000;

		var refreshPromise = $interval(getJobsByFilter, refreshInterval);

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
				vm.fromDateFilter = vm.minFrom;
				vm.toDateFilter = vm.maxTo;
				getJobsByFilter();
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

			var params = { "from": vm.resultsFrom, "to": vm.resultsTo, "dataSource": dataSource, "type": jobType, "fromdate": vm.fromDateFilter, "todate": vm.toDateFilter };
			$http.get("./Stardust/Jobs", tokenHeaderService.getHeadersAndParams(params))
				.then(function(data) {
					if (data.length < vm.resultsTo) {
						vm.moreJobs = false;
					} else {
						vm.moreJobs = true;
					}
					vm.Jobs = data;
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
					}
					cancelPollingAndShowExpiredDialog();
				});
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

		function getMoreData() {
			vm.resultsTo += vm.limit;
			getJobsByFilter();
		}

		function search() {
			vm.dataSourceFilter = vm.selectedDataSource;
			vm.jobTypeFilter = vm.selectedJobType;
			vm.resultsTo = vm.limit;
			vm.fromDateFilter = vm.selectedFromDate;
			vm.toDateFilter = vm.selectedToDate;
			getJobsByFilter();
		}
	} 
})();
