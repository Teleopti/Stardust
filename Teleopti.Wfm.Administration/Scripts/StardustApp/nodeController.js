(function() {
	"use strict";

	angular
		.module("adminApp")
		.controller("nodeController", nodeController, ["tokenHeaderService"]);

	function nodeController($http, $interval, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = "Stardust Nodes";
		vm.NodeError = "";
		vm.JobError = "";
		vm.back = back;
		vm.WorkerNodes = [];
		var refreshInterval = 3000;
		getWorkerNodes();

		function getWorkerNodes() {	
		$http.get("./Stardust/WorkerNodes", tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.WorkerNodes = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				vm.NodeError = ajaxOptions;
				console.log(xhr.Message + ": " + xhr.ExceptionMessage);
				if (xhr !== "") {
					vm.NodeError = vm.NodeError + " " + xhr.Message + ": " + xhr.ExceptionMessage;
				}

			});
		};
		function pollWorkerNodes() {
			vm.WorkerNodes = [];
			getWorkerNodes();
		}
		$interval(pollWorkerNodes, refreshInterval);

		function back() {
			window.history.back();
		};
	}
})();
