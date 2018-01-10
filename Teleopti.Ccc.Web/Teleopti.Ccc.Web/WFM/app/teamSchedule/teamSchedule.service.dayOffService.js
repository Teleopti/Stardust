(function () {
	"use strict";

	angular.module("wfm.teamSchedule").service("DayOffService", [
		"$http", "$q", DayOffService]);

	function DayOffService($http, $q) {
		var urlMap = {
			loadTemplate: "../api/TeamScheduleData/GetAvailableTemplates",
			addDayOff: "../api/TeamScheduleCommand/AddDayOff"
		};

		this.getAvailableTemplates = getAvailableTemplates;
		this.addDayOff = addDayOff;

		var dayOffs = [];
		function getAvailableTemplates() {
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

	}
})();