(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("exportScheduleService", ExportScheduleService);

	ExportScheduleService.$inject = ['$q', '$http'];

	function ExportScheduleService($q, $http) {
		var self = this;
		var scenarioUrl = "../api/TeamScheduleData/Scenarios";

		self.getScenarioData = getScenarioData;

		function getScenarioData() {
			return $q(function (resolve, reject) {
				$http.get(scenarioUrl).then(function (response) {
					resolve(response.data);
				}, function (err) {
					reject(err);
				});
			});

		}
	}
})();