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

		svc.PromiseForSwapShifts = function(personFrom, personTo, scheduleDate, action) {
			var trackId = guidgenerator.newGuid();
			swapShifts.post({
				PersonIdFrom: personFrom,
				PersonIdTo: personTo,
				ScheduleDate: scheduleDate.format("YYYY-MM-DD"),
				TrackedCommandInfo: { TrackId: trackId }
			}).$promise.then(function (result) {
				action({
					TrackId: trackId,
					Errors: result
				});
			});
		}
	}
]);