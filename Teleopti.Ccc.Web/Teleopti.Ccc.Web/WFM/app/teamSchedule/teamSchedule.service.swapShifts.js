(function() {
	"use strict";
	angular.module("wfm.teamSchedule").service("SwapShifts", ["$http", SwapShiftsSevice]);

	function SwapShiftsSevice($http) {
		var svc = this;
		var url = "../api/TeamSchedule/SwapShifts";
		svc.swapShifts = swapShifts;

		function swapShifts(swapShiftsForm) {
			return $http.post(url, swapShiftsForm);
		}
	}
})();