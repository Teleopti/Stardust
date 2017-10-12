(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("exportScheduleService", ExportScheduleService);

	ExportScheduleService.$inject = ['$q', '$http'];

	function ExportScheduleService($q, $http) {
		var self = this;
		var scenarioUrl = "../api/TeamScheduleData/Scenarios";
		var timezoneUrl = '../api/Global/TimeZone';
		var optionalColumnUrl = '../api/TeamScheduleData/OptionalColumns';

		self.getScenarioData = getScenarioData;
		self.getTimezonesData = getTimezonesData;
		self.getOptionalColumnsData = getOptionalColumnsData;

		function getOptionalColumnsData() {
			return $q(function (resolve, reject) {
				$http.get(optionalColumnUrl).then(function (response) {
					resolve(response.data);
				}, function (err) {
					reject(err);
				});
			});
		}

		function getTimezonesData() {
			return $q(function (resolve, reject) {
				$http.get(timezoneUrl).then(function (response) {
					resolve(response.data);
				}, function (err) {
					reject(err);
				});
			});
		}

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