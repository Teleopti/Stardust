(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("exportScheduleService", ExportScheduleService);

	ExportScheduleService.$inject = ['$q', '$http','serviceDateFormatHelper'];

	function ExportScheduleService($q, $http, serviceDateFormatHelper) {
		var self = this;
		var scenarioUrl = "../api/TeamScheduleData/Scenarios";
		var timezoneUrl = '../api/Global/TimeZone';
		var optionalColumnUrl = '../api/TeamScheduleData/OptionalColumns';
		var startExportUrl = '../api/TeamSchedule/StartExport';

		self.getScenarioData = getScenarioData;
		self.getTimezonesData = getTimezonesData;
		self.getOptionalColumnsData = getOptionalColumnsData;
		self.startExport = startExport;

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

		function startExport(input) {
			
			return $q(function(resolve, reject) {
				$http({
						url: startExportUrl,
						method: 'POST',
						data: normalizeInput(input),
						responseType: 'arraybuffer',
						headers:{
							Accept: 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
						}
					}).then(function(response) {
						resolve(response);
					},
					function(err) {
						reject(err);
					});
			});
		}
		function normalizeInput(input) {
			var normalizedSelectedGroups = {
				SelectedGroupIds: input.selectedGroups.groupIds,
				SelectedGroupPageId: input.selectedGroups.groupPageId
			};
			return {
				StartDate: serviceDateFormatHelper.getDateOnly(input.period.startDate),
				EndDate: serviceDateFormatHelper.getDateOnly(input.period.endDate),
				ScenarioId: input.scenarioId,
				TimezoneId: input.timezoneId,
				SelectedGroups: normalizedSelectedGroups,
				OptionalColumnIds: input.optionalColumnIds 
			};
		}
	}
})();