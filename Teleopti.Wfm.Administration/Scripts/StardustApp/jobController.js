(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobController', jobController, ['tokenHeaderService']);

	function jobController($http, $interval, tokenHeaderService) {
		/* jshint validthis:true */

		var vm = this;
		vm.title = 'Stardust Jobs';
		vm.NodeError = '';
		vm.JobError = '';
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.noMoreJobs = false;
		vm.getNewFreshData = getNewFreshData;
		vm.Jobs = [];
		var refreshInterval = 10000;

		getJobs();

		function getJobs(dataExists) {
			$http.get("./Stardust/Jobs/" + vm.resultsFrom + "/" + vm.resultsTo, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (data.length < vm.limit) {
						vm.noMoreJobs = true;
					}
					if (dataExists) {
						vm.Jobs = vm.Jobs.concat(data);
					} else {
						vm.Jobs = data;
					}
					
				})
				.error(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
					vm.JobError = ajaxOptions;
					if (xhr !== "") {
						vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
					}
				});
		};
		function getNewFreshData() {
			vm.resultsFrom += vm.limit;
			vm.resultsTo += vm.limit;
			getJobs(true);
		}

		function pollNewData() {
			var tmpFrom = vm.resultsFrom;
			vm.resultsFrom = 1;
			getJobs();
			vm.resultsFrom = tmpFrom;
		}

		$interval(pollNewData, refreshInterval);
	}
})();
