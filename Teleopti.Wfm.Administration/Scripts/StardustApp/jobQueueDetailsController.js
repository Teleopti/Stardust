(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobQueueDetailsController', jobQueueDetailsController);

	function jobQueueDetailsController($http, $routeParams) {
		/* jshint validthis:true */
		var vm = this;
		vm.back = back;
		vm.JobId = $routeParams.jobId;
		$http.get("./Stardust/QueuedJobs/" + vm.JobId)
			.then(function (response) {
				vm.Job = response.data;
			});
		
		function back() {
			window.history.back();
		}
	}
})();
