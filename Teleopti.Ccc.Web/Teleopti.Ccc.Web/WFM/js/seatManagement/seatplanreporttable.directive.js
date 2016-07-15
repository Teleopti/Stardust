"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportTableCtrl', seatPlanReportTableCtrl);
	seatPlanReportTableCtrl.$inject = ['$translate', 'seatMapCanvasUtilsService'];

	function seatPlanReportTableCtrl($translate, utils) {
		var vm = this;

		vm.getDisplayTime = function (booking) {

			if (booking.IsDayOff) {
				return $translate.instant('DayOff');
			}

			if (booking.IsFullDayAbsence) {
				return $translate.instant('FullDayAbsence');
			}

			return utils.getSeatBookingTimeDisplay(booking);
		};	

		vm.getDisplayDate = function (date) {
			return moment(date).format('L');
		};

		var getValueOrEmptyString = function (object) {
			return object || '';
		}

		vm.getSeatName = function (booking) {
			if (booking.SeatName !== '' && booking.SeatName !== null) {
				return getValueOrEmptyString(booking.LocationPrefix) + booking.SeatName + getValueOrEmptyString(booking.LocationSuffix);
			}

			return '';
		}
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
