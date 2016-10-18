(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobQueueController', jobQueueController, ['tokenHeaderService']);

	function jobQueueController($http, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = 'Stardust Queue';
		vm.NodeError = '';
		vm.JobError = '';
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.noMoreJobs = false;

		vm.getNewFreshData = getNewFreshData;
		vm.Jobs = [];

		getJobs();

		function getJobs(dataExists) {
			$http.get("./Stardust/QueuedJobs/" + vm.resultsFrom + "/" + vm.resultsTo, tokenHeaderService.getHeaders())
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
				.error(function (xhr, ajaxOptions, thrownError) {
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
	}
})();
