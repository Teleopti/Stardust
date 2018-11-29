(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobQueueDetailsController', jobQueueDetailsController, ['tokenHeaderService']);

	function jobQueueDetailsController($http, $routeParams, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.back = back;
		vm.JobId = $routeParams.jobId;
		$http.get("./Stardust/QueuedJobs/" + vm.JobId, tokenHeaderService.getHeaders())
			.then(function(data) {
				vm.Job = data;
			})
			.catch(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});
		
		function back() {
			window.history.back();
		};
	}
})();
