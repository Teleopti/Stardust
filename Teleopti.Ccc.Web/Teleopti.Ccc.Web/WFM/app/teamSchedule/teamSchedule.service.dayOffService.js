(function () {
	"use strict";

	angular.module("wfm.teamSchedule").service("DayOffService", [
		"$http", "$q", DayOffService]);

	function DayOffService($http, $q) {
		var urlMap = {
			loadTemplate: "../api/TeamScheduleData/AllDayOffTemplates",
			addDayOff: "../api/TeamScheduleCommand/AddDayOff",
			removeDayOff: "../api/TeamScheduleCommand/RemoveDayOff"
		};

		this.getAllDayOffTemplates = getAllDayOffTemplates;
		this.addDayOff = addDayOff;
		this.removeDayOff = removeDayOff;

		var dayOffs = [];
		function getAllDayOffTemplates() {
			return $q(function (resolve, reject) {
				if (!!dayOffs.length) {
					resolve(dayOffs);
					return;
				}
				$http.get(urlMap.loadTemplate).success(function (data) {
					dayOffs = data;
					resolve(data);
				});
			});
		};

		function addDayOff(input) {
			return $http.post(urlMap.addDayOff, input);
		}

		function removeDayOff(input) {
			return $http.post(urlMap.removeDayOff, input);
		}

	}
})();