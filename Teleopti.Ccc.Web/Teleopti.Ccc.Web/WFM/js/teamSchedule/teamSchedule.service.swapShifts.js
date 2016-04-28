"use strict";

angular.module("wfm.teamSchedule").service("SwapShifts", ["$resource", "guidgenerator",
	function ($resource, guidgenerator) {
		var svc = this;

		var swapShifts = $resource("../api/TeamSchedule/SwapShifts", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		svc.SwapShifts = SwapShifts;

		function SwapShifts(swapShiftsForm) {
			return swapShifts.post(swapShiftsForm).$promise;
		}
	}
]);
