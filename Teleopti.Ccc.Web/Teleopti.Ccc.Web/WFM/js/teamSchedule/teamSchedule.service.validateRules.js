(function(){
	'use strict';
	angular.module("wfm.teamSchedule").service("ValidateRulesService", ValidateRulesService);

	ValidateRulesService.$inject = ["$http", "$q", "$timeout"];

	function ValidateRulesService($http, $q, $timeout){

		var self = this;
		var getValidateRulesResultUrl = '../api/TeamScheduleData/FetchRuleValidationResult';
		var warningDict = {};

		self.getValidateRulesResult = getValidateRulesResult;
		self.checkValidationForPerson = checkValidationForPerson;
		self.ValidationIsDone = false;

		function getValidateRulesResult(momentDate, personIds) {
			var postData = {
				Date: momentDate.format('YYYY-MM-DD'),
				PersonIds: personIds
			};

			warningDict = {};
			self.ValidationIsDone = false;

			$http.post(getValidateRulesResultUrl, postData).success(function (data) {
				data.forEach(function(warning){
					warningDict[warning.PersonId] = warning.Warnings;
				});

				self.ValidationIsDone = true;
			});
		}

		function checkValidationForPerson(personId) {
			return warningDict[personId] || [];
		}
	}
})();