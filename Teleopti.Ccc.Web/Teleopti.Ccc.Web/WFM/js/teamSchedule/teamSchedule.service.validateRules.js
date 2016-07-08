(function(){
	'use strict';
	angular.module("wfm.teamSchedule").service("ValidateRulesService", ValidateRulesService);

	ValidateRulesService.$inject = ["$http", "$q", "$timeout"];

	function ValidateRulesService($http, $q, $timeout){

		var self = this;
		var getValidateRulesResultUrl = '../api/TeamScheduleData/FetchRuleValidationResult';
		var warningDict = {};

		self.getValidateRulesResultForCurrentPage = getValidateRulesResultForCurrentPage;
		self.updateValidateRulesResultForPeople = updateValidateRulesResultForPeople;
		self.checkValidationForPerson = checkValidationForPerson;

		function getValidateRulesResultForCurrentPage(momentDate, personIds) {
			warningDict = {};
			personIds.forEach(function(id) {
				warningDict[id] = {
					isLoaded: false,
					warnings: ""
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
						warnings: ""
					}
					personIdOnCurrentPage.push(id);
				}
				
			});

			getValidateRulesResult(momentDate, personIdOnCurrentPage);
		}

		function getValidateRulesResult(momentDate, personIds) {
			var postData = {
				Date: momentDate.format('YYYY-MM-DD'),
				PersonIds: personIds
			};

			$http.post(getValidateRulesResultUrl, postData).success(function (data) {
				for (var personId in warningDict) {
					if (warningDict.hasOwnProperty(personId)) {
						warningDict[personId].isLoaded = true;
					}
				}
				data.forEach(function (warning) {
					warningDict[warning.PersonId].warnings = warning.Warnings;
				});
			});
		}

		function checkValidationForPerson(personId) {
			return warningDict[personId] || [];
		}
	}
})();