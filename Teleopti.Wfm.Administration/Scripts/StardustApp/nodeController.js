(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('nodeController', nodeController, ['tokenHeaderService']);

	function nodeController($http, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = 'Stardust Nodes';
		vm.NodeError = '';
		vm.JobError = '';

		$http.get("./Stardust/WorkerNodes", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.WorkerNodes = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				vm.NodeError = ajaxOptions;
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				if (xhr !== "") {
					vm.NodeError = vm.NodeError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
				}

			});
	}
})();
