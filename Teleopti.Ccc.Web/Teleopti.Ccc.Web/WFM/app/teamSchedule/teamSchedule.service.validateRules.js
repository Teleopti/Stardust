(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("ValidateRulesService", ValidateRulesService);

	ValidateRulesService.$inject = ["$http", "$q", 'serviceDateFormatHelper'];

	function ValidateRulesService($http, $q, serviceDateFormatHelper) {

		var self = this;
		var getValidationRulesUrl = '../api/TeamScheduleData/FetchAllValidationRules';
		var getValidateRulesResultUrl = '../api/TeamScheduleData/FetchRuleValidationResult';
		var warningDict = {};
		var currentEnabledTypes = {};

		self.getAvailableValidationRules = getAvailableValidationRules;
		self.getValidateRulesResultForCurrentPage = getValidateRulesResultForCurrentPage;
		self.updateCheckedValidationTypes = updateCheckedValidationTypes;
		self.updateValidateRulesResultForPeople = updateValidateRulesResultForPeople;
		self.checkValidationForPerson = checkValidationForPerson;
		self.checkIsLoadedValidationForPerson = checkIsLoadedValidationForPerson;

		function getAvailableValidationRules() {
			var deferred = $q.defer();

			$http.get(getValidationRulesUrl).then(function (data) {
				deferred.resolve(data);
			}, function (error) {
				deferred.reject(error);
			});

			return deferred.promise;
		}

		function updateCheckedValidationTypes(type, checked) {
			currentEnabledTypes[type] = checked;
		}

		function getValidateRulesResultForCurrentPage(momentDate, personIds) {
			warningDict = {};
			personIds.forEach(function (id) {
				warningDict[id] = {
					isLoaded: false,
					warnings: []
				}
			});
			getValidateRulesResult(momentDate, personIds);
		}

		function updateValidateRulesResultForPeople(momentDate, personIds) {
			var personIdOnCurrentPage = [];
			personIds.forEach(function (id) {
				if (warningDict[id]) {
					warningDict[id] = {
						isLoaded: false,
						warnings: []
					}
					personIdOnCurrentPage.push(id);
				}
			});

			getValidateRulesResult(momentDate, personIdOnCurrentPage);
		}

		function getValidateRulesResult(momentDate, personIds) {
			var postData = {
				Date: serviceDateFormatHelper.getDateOnly(momentDate),
				PersonIds: personIds
			};

			$http.post(getValidateRulesResultUrl, postData).then(function (response) {
				for (var personId in warningDict) {
					warningDict[personId] && (warningDict[personId].isLoaded = true);
				}

				response.data.forEach(function (warning) {
					warningDict[warning.PersonId].warnings = warning.Warnings;
				});
			});
		}

		function checkValidationForPerson(personId, filteredRuleType) {
			if (!warningDict[personId]) return [];

			var result = warningDict[personId].warnings.filter(function (w) {
				if (filteredRuleType)
					return filteredRuleType == w.RuleType;
				return currentEnabledTypes[w.RuleType];
			}).map(function (w) {
				return w.Content;
			});

			return result;
		}

		function checkIsLoadedValidationForPerson(personId) {
			return warningDict[personId] && warningDict[personId].isLoaded;
		}
	}
})();