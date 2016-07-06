(function(){
	'use strict';
	angular.module("wfm.teamSchedule").service("ValidateRulesService", ValidateRulesService);

	ValidateRulesService.$inject = ["$http", "$q", "$timeout"];

	function ValidateRulesService($http, $q, $timeout){

		var self = this;
		var getValidateRulesResultUrl = "../api/TeamScheduleData/GetValidateRulesResult";
		var warningDict = {};
		
		// this.getValidateRulesResult = getValidateRulesResult;
		self.getValidateRulesResult = getValidateRulesResultFake;
		self.checkValidationForPerson = checkValidationForPerson;
		self.ValidationIsDone = false;

		function getValidateRulesResult() {
			var deferred = $q.defer();
			$http.get(getValidateRulesResultUrl).success(function (data) {
				data.forEach(function(warning){
					warningDict[warning.PersonId] = warning.WarningMessage;	
				});

				validating = false;
				deferred.resolve(fakeData);
			});

			return deferred.promise;
		}

		function getValidateRulesResultFake() {
			var fakeData;
			var deferred = $q.defer();

			warningDict = {};
			self.ValidationIsDone = false;

			$timeout(function() {
				fakeData = [{
					PersonId:'4fd900ad-2b33-469c-87ac-9b5e015b2564',
					WarningMessage: ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. ", "Quae cum dixisset paulumque institisset, Quid est? "],
				},
				{
					PersonId:'10957ad5-5489-48e0-959a-9b5e015b2b5c',
					WarningMessage: ["Lorem ipsum dolor sit amet, consectetur adipiscing elit. ", "Quae cum dixisset paulumque institisset, Quid est? "],
				}];

				fakeData.forEach(function(warning){
					warningDict[warning.PersonId] = warning.WarningMessage;	
				});

				self.ValidationIsDone = true;

				deferred.resolve(fakeData);
			}, 4000);

			return deferred.promise;
		}

		function checkValidationForPerson(personId) {
			return warningDict[personId] || [];
		}
	}
})();