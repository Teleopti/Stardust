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
			.success(function(data) {
				vm.JobDetails = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});
		$http.get("./Stardust/Job/" + vm.JobId, tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.Job = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});

		function preferedLogLevel() {
			return 50;
		}

		function back() {
			window.history.back();
		};
	}
})();
