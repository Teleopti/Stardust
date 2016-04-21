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

		service.removePersonAbsence = removePersonAbsence;

		function removePersonAbsence(scheduleDate, selectedPersonAbsences, removeEntireCrossDayAbsence, trackId) {
			var cmd = {
				ScheduleDate: scheduleDate.format("YYYY-MM-DD"),
				SelectedPersonAbsences: selectedPersonAbsences,
				RemoveEntireCrossDayAbsence: removeEntireCrossDayAbsence,
				TrackedCommandInfo: { TrackId: trackId }
			};
			return service.removeAbsence.post(cmd).$promise;
		}
	}
]);
