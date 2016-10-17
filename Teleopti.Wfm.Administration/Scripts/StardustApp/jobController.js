(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('jobController', jobController, ['tokenHeaderService']);

	function jobController($http, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = 'Stardust Jobs';
		vm.NodeError = '';
		vm.JobError = '';

		$http.get("./Stardust/Jobs", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.Jobs = data;
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
