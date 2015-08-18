"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportTableCtrl', seatPlanReportTableCtrl);


	seatPlanReportTableCtrl.$inject = ['seatPlanTranslateFactory'];

	function seatPlanReportTableCtrl(seatPlanTranslations) {
		var vm = this;

		vm.getDisplayTime = function (booking) {

			if (booking.IsDayOff) {
				return seatPlanTranslations.TranslatedStrings['DayOff'];
			}
			if (booking.IsFullDayAbsence) {
				return seatPlanTranslations.TranslatedStrings['FullDayAbsence'];
			}

			return moment(booking.StartDateTime).format('HH:mm') + ' - ' + moment(booking.EndDateTime).format('HH:mm');
		};

		vm.getDisplayDate = function (date) {
			return moment(date).format('L');
		};
	};

})();
