(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobDetailsController', jobDetailsController, ['tokenHeaderService']);

	function jobDetailsController($http, $routeParams, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.back = back;
		vm.JobId = $routeParams.jobId;
		vm.quantity = preferedLogLevel();
		

		$http.get("./Stardust/JobDetails/" + vm.JobId, tokenHeaderService.getHeaders())
			.then(function (response) {
				vm.JobDetails = response.data;
			});

		$http.get("./Stardust/Job/" + vm.JobId, tokenHeaderService.getHeaders())
			.then(function (response) {
				vm.Job = response.data;
			});

		function preferedLogLevel() {
			return 50;
		}

		function back() {
			window.history.back();
		}
	}
})();
