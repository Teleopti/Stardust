(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('nodeDetailsController', nodeDetailsController, ['tokenHeaderService']);

	function nodeDetailsController($http, $routeParams, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.back = back;
		vm.limit = 50;
		vm.resultsTo = vm.limit;
		vm.resultsFrom = 1;
		vm.noMoreJobs = false;
		vm.getNewFreshData = getNewFreshData;
		vm.NodeId = $routeParams.nodeId;

		getJobs();

		function getJobs(dataExists) {
			$http.get("./Stardust/WorkerNode/" + vm.NodeId, tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.Node = response.data;
				});

			$http.get("./Stardust/JobsByNode/" + vm.NodeId + "/" + vm.resultsFrom + "/" + vm.resultsTo  , tokenHeaderService.getHeaders())
				.then(function (response) {
					if (response.data.length < vm.limit) {
						vm.noMoreJobs = true;
					}
					if (dataExists) {
						vm.Jobs = vm.Jobs.concat(response.data);
					} else {
						vm.Jobs = response.data;
					}
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		function back() {
			window.history.back();
		};

		function getNewFreshData() {
			vm.resultsFrom += vm.limit;
			vm.resultsTo += vm.limit;
			getJobs(true);
		}
	}


})();
