"use strict";

(function () {

	function seatPlanReportTableCtrl(seatPlanTranslatorFactory) {
		var vm = this;

		vm.getDisplayTime = function (booking) {

			if (booking.IsDayOff) {
				return seatPlanTranslatorFactory.TranslatedStrings['DayOff'];
			}
			if (booking.IsFullDayAbsence) {
				return seatPlanTranslatorFactory.TranslatedStrings['FullDayAbsence'];
			}

			return moment(booking.StartDateTime).format('HH:mm') + ' - ' + moment(booking.EndDateTime).format('HH:mm');
		};

		vm.getDisplayDate = function (date) {
			return moment(date).format('L');
		};
	};

	angular.module('wfm.seatPlan').controller('seatPlanReportTableCtrl', seatPlanReportTableCtrl);
	seatPlanReportTableCtrl.$inject = ['seatPlanTranslatorFactory'];
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
