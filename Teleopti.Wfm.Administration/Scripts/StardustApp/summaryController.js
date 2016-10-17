﻿(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('summaryController', summaryController, ['tokenHeaderService']);

	function summaryController($http, tokenHeaderService) {
		/* jshint validthis:true */
		var vm = this;
		vm.title = 'Stardust Summary';
		vm.NodeError = '';
		vm.JobError = '';

		$http.get("./Stardust/RunningJobs", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.RunningJobs = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
				}
			});

		$http.get("./Stardust/QueuedJobsSummary", tokenHeaderService.getHeaders())
			.success(function(data) {
				vm.QueuedJobs = data;
			})
			.error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				vm.JobError = ajaxOptions;
				if (xhr !== "") {
					vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
				}
			});

		$http.get("./Stardust/AliveWorkerNodes", tokenHeaderService.getHeaders())
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
