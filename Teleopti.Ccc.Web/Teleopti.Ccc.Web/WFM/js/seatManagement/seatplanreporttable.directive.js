"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportTableCtrl', seatPlanReportTableCtrl);
	seatPlanReportTableCtrl.$inject = ['seatPlanTranslatorFactory', 'seatMapCanvasUtilsService'];

	function seatPlanReportTableCtrl(seatPlanTranslatorFactory, utils) {
		var vm = this;

		vm.getDisplayTime = function (booking) {

			if (booking.IsDayOff) {
				return seatPlanTranslatorFactory.TranslatedStrings['DayOff'];
			}

			if (booking.IsFullDayAbsence) {
				return seatPlanTranslatorFactory.TranslatedStrings['FullDayAbsence'];
			}

			return utils.getSeatBookingTimeDisplay(booking);
		};	

		vm.getDisplayDate = function (date) {
			return moment(date).format('L');
		};
	};
	
})();

(function () {
	function seatPlanReportTable() {
		return {
			controller: 'seatPlanReportTableCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				seatBookings: '='
			},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreporttable.html"
		};
	};

	angular.module('wfm.seatPlan').directive('seatPlanReportTable', seatPlanReportTable);
})();
