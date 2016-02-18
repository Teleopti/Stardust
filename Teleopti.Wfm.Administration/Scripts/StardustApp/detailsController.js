(function () {
	'use strict';

	angular
		 .module('app')
		 .controller('detailsController', detailsController);

	function detailsController($http, $routeParams) {
		/* jshint validthis:true */
		var vm = this;
		vm.JobId = $routeParams.jobId;
		$http.get("../ManagerController/jobdetail/" + vm.JobId ).success(function (data) {
			vm.JobDetails = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

	}
})();
