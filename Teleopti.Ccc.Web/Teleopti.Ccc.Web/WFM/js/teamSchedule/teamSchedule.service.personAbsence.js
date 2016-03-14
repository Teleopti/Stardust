"use strict";

angular.module("wfm.teamSchedule").service("PersonAbsence", [
	"$resource", "$q", "guidgenerator", function ($resource, $q, guidgenerator) {
		var service = this;

		service.loadAbsences = $resource("../api/Absence/GetAvailableAbsences", {}, {
			query: {
				method: "GET",
				params: {},
				isArray: true
			}
		});

		service.applyFullDayAbsence = $resource("../api/TeamSchedule/AddFullDayAbsence", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.applyIntradayAbsence = $resource("../api/TeamSchedule/AddIntradayAbsence", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.removeAbsence = $resource("../api/TeamSchedule/RemoveAbsence", {}, {
			post: {
				method: "POST",
				params: {},
				isArray: true
			}
		});

		service.PromiseForRemovePersonAbsence = function (scheduleDate, personIds, personAbsenceIds, removeEntireCrossDayAbsence, action) {
			var trackId = guidgenerator.newGuid();

			var cmd = {
				ScheduleDate: scheduleDate.format("YYYY-MM-DD"),
				PersonIds: personIds,
				PersonAbsenceIds: personAbsenceIds,
				RemoveEntireCrossDayAbsence: removeEntireCrossDayAbsence,
				TrackedCommandInfo: { TrackId: trackId }
			}
			service.removeAbsence.post(cmd).$promise.then(function (result) {
				action({
					TrackId: trackId,
					Errors: result
				});
			});
		}
	}
]);
