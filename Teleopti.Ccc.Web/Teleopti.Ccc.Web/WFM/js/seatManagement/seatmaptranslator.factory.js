'use strict';

(function () {

	angular.module('wfm.seatMap').factory('seatMapTranslatorFactory', seatMapTranslatorFactory);

	seatMapTranslatorFactory.$inject = ['$translate'];

	function seatMapTranslatorFactory(translate) {

		var translatedStrings = {};

		var setupTranslatedString = function (key) {
			translate(key).then(function (result) {
				translatedStrings[key] = result;
			});
		};

		setupTranslatedString("SeatBookingDeletedSuccessfully");

		return {
			TranslatedStrings: translatedStrings
		}
	};

}());