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
		vm.search = search;
		vm.Jobs = [];
		var allTenantsString = "All Tenants";
		vm.dataSourceFilter = allTenantsString;
		var refreshInterval = 3000;
		
		getJobsByFilter();

		var refreshPromise = $interval(pollNewData, refreshInterval);

		$scope.$on("$destroy",
			function () {
				$interval.cancel(refreshPromise);
			});

		$http.get("./AllTenants", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.Tenants = data;
			});

		function getJobs(dataExists) {
			$http.get("./Stardust/Jobs/" + vm.resultsFrom + "/" + vm.resultsTo, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (data.length < vm.limit) {
						vm.moreJobs = false;
					}
					if (dataExists) {
						vm.Jobs = vm.Jobs.concat(data);
					} else {
						vm.Jobs = data;
					}
					
				})
				.error(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ": " + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
					}
				});
		}

		function getJobsByFilter(dataExists) {
			if (vm.dataSourceFilter === allTenantsString) {
				getJobs(dataExists);
			} else {
				var params = { "from": vm.resultsFrom, "to": vm.resultsTo, "dataSource": vm.dataSourceFilter };
				$http.get("./Stardust/Jobs", tokenHeaderService.getHeadersAndParams(params))
					.success(function (data) {
						if (data.length < vm.limit) {
							vm.moreJobs = false;
						}
						if (dataExists) {
							vm.Jobs = vm.Jobs.concat(data);
						} else {
							vm.Jobs = data;
						}
					})
					.error(function (xhr, ajaxOptions, thrownError) {
						console.log(xhr.Message + ": " + xhr.ExceptionMessage);
						vm.JobError = ajaxOptions;
						if (xhr !== "") {
							vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
						}
					});
			}
		}

		function getMoreData() {
			vm.resultsFrom += vm.limit;
			vm.resultsTo += vm.limit;
			getJobs(true);
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

		function search() {
			vm.dataSourceFilter = vm.selectedDataSource;
			pollNewData();
		}
	}
})();
