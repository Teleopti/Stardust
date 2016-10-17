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


		$http.get("./Stardust/QueuedJobs", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.QueuedJobs = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
				}
			});
	}
})();
