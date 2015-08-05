"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);

	seatPlanReportCtrl.$inject = ['seatPlanService', '$window'];

	function seatPlanReportCtrl(seatPlanService, $window) {

		var vm = this;

		vm.getDisplayTimeString = function (startTime, endTime) {
			return moment(startTime).format('HH:mm') + ' - ' + moment(endTime).format('HH:mm');
		};

		vm.getDisplayDateString = function (data) {
			return moment(data).format('L');
		};

		var seatBookingReportParams = {
			startDate: moment('2015-08-17').format('YYYY-MM-DD'),
			endDate: moment('2015-09-13').format('YYYY-MM-DD'),
			//teams: []
			//locations: []
			skip: 0,
			take : 20
		};

		seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
			vm.seatBookings = data.SeatBookingsByDate;

		});
	};

})();


(function () {
	angular.module('wfm.seatPlan').directive('seatPlanReport', seatPlanReport);

	function seatPlanReport() {
		return {
			controller: 'seatPlanReportCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: { toggle: '&toggle', selectedPeriod: '=selectedPeriod'},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreport.html"
		};
	};
})();


