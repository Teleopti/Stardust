(function () {
	"use strict";

	angular.module("wfm.teamSchedule").service("DayOffService", [
		"$http", "$q", DayOffService]);

	function DayOffService($http, $q) {
		var service = this;

		var loadTemplateUrl = "../api/TeamSchedule/GetAvailableTemplates";

		var dayOffs = [];
		service.getAvailableTemplates = function () {
			return $q(function (resolve, reject) {
				if (!!dayOffs.length) {
					resolve(dayOffs);
					return;
				}
				$http.get(loadTemplateUrl).success(function (data) {
					dayOffs = data;
					resolve(data);
				});
			});
		};
	}
})();