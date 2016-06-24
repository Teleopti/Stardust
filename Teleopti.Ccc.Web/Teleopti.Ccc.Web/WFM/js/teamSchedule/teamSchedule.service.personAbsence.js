(function() {
	"use strict";

	angular.module("wfm.teamSchedule").service("PersonAbsence", [
		"$http", "$q", function($http, $q) {
			var service = this;

			var loadAbsencesUrl = "../api/Absence/GetAvailableAbsences";
			var addFullDayAbsenceUrl = "../api/TeamSchedule/AddFullDayAbsence";
			var addIntradayAbsenceUrl = "../api/TeamSchedule/AddIntradayAbsence";
			var removeAbsenceUrl = "../api/TeamSchedule/RemoveAbsence";

			service.loadAbsences = function() {
				var deferred = $q.defer();
				$http.get(loadAbsencesUrl).success(function(data) {
					deferred.resolve(data);
				});
				return deferred.promise;
			}

			service.addFullDayAbsence = function(data) {
				return $http.post(addFullDayAbsenceUrl, data);
			}

			service.addIntradayAbsence = function(data) {
				return $http.post(addIntradayAbsenceUrl, data);
			}

			service.removeAbsence = function(data) {
				return $http.post(removeAbsenceUrl, data);
			}

			service.removePersonAbsence = function(scheduleDate, selectedPersonAbsences, removeEntireCrossDayAbsence, trackId) {
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
})();
