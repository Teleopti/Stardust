(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ValidateRulesService", ValidateRulesService);

	ValidateRulesService.$inject = ["$http", "$q"];

	function ValidateRulesService($http, $q) {

		var self = this;
		var getValidationRulesUrl = '../api/TeamScheduleData/FetchAllValidationRules';
		var getValidateRulesResultUrl = '../api/TeamScheduleData/FetchRuleValidationResult';
		var warningDict = {};
		var currentEnabledTypes = {};

		self.getAvailableValidationRules = getAvailableValidationRules;
		self.updateCheckedValidationTypes = updateCheckedValidationTypes;
		self.updateValidateRulesResultForPeople = updateValidateRulesResultForPeople;
		self.checkValidationForPerson = checkValidationForPerson;

		function getAvailableValidationRules() {
			return $q(function (resolve, reject) {
				$http.get(getValidationRulesUrl).then(function (data) {
					resolve(data);
				}, function (error) {
					reject(error);
				});
			});
		}

		function updateCheckedValidationTypes(type, checked) {
			currentEnabledTypes[type] = checked;
		}

		function updateValidateRulesResultForPeople(date, personIds) {
			var postData = {
				Date: date,
				PersonIds: personIds
			};
		
			return $q(function (resolve) {
				$http.post(getValidateRulesResultUrl, postData).then(function (response) {
					personIds.forEach(function (personId) {
						delete warningDict[personId];
					});
					response.data.forEach(function (warning) {
						warningDict[warning.PersonId] = warning.Warnings;
					});
					resolve();
				})
			});
		}

		function checkValidationForPerson(personId) {
			if (!warningDict[personId]) return [];

			return warningDict[personId].filter(function (w) {
				return currentEnabledTypes[w.RuleType];
			}).map(function (w) {
				return w.Content;
			});
		}
	}
})();