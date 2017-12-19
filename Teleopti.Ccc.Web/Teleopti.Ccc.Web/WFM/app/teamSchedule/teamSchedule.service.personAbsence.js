(function () {
	"use strict";

	angular.module("wfm.teamSchedule").service("PersonAbsence", [
		"$http", "$q", PersonAbsenceService]);

	function PersonAbsenceService($http, $q) {
		var service = this;

		var loadAbsencesUrl = "../api/Absence/GetAvailableAbsences";
		var addFullDayAbsenceUrl = "../api/TeamSchedule/AddFullDayAbsence";
		var addIntradayAbsenceUrl = "../api/TeamSchedule/AddIntradayAbsence";
		var removeAbsenceUrl = "../api/TeamSchedule/RemoveAbsence";

		var absences = [];
		service.loadAbsences = function () {
			return $q(function (resolve, reject) {
				if (!!absences.length) {
					resolve(absences);
					return;
				}
				$http.get(loadAbsencesUrl).success(function (data) {
					absences = data;
					resolve(data);
				});
			});
		};

		service.addAbsence = function (requestData, isFullDay) {
			var url = isFullDay ? addFullDayAbsenceUrl : addIntradayAbsenceUrl;
			return $http.post(url, requestData);
		};

		service.removeAbsence = function (data) {
			return $http.post(removeAbsenceUrl, data);
		};
	}
})();