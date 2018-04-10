(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("etlHistoryController", etlHistoryController, ["$http"]);

	function etlHistoryController($http, tokenHeaderService) {
		var vm = this;

		var date = new Date();

		vm.historyWorkPeriod = {
			StartDate: new Date(date.getTime() - 86400000),
			EndDate: new Date()
		};

	}
})();
