(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('jobDetailsController', jobDetailsController);

	function jobDetailsController($http, $routeParams) {
		/* jshint validthis:true */
		var vm = this;
		vm.JobId = $routeParams.jobId;
		$http.get("./Stardust/JobDetails/" + vm.JobId).success(function (data) {
			vm.JobDetails = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

	}
})();
