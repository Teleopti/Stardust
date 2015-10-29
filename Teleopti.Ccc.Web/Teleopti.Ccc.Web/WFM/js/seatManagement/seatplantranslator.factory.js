'use strict';

(function () {

	angular.module('wfm.seatPlan').factory('seatPlanTranslatorFactory', seatPlanTranslatorFactory);

	seatPlanTranslatorFactory.$inject = ['$translate'];

	function seatPlanTranslatorFactory(translate) {

		var translatedStrings = {};

		var setupTranslatedString = function (key) {
			translate(key).then(function (result) {
				translatedStrings[key] = result;
			});
		};

		setupTranslatedString("NoLocationsAvailable");
		setupTranslatedString("SeatCountTitle");
		setupTranslatedString("AgentCountTitle");
		setupTranslatedString("TeamsOrLocationsAreUnselected");
		//setupTranslatedString("SeatPlanSubmittedOK");
		setupTranslatedString("DayOff");
		setupTranslatedString("FullDayAbsence");
		setupTranslatedString("SeatPlanResultDetailMessage");
		

		return {
			TranslatedStrings: translatedStrings
		}
	};

}());