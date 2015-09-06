'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatMapOccupancyDetailCtrl', locationPickerDirectiveController);
	locationPickerDirectiveController.$inject = ['seatMapCanvasUtilsService'];

	function locationPickerDirectiveController(utils) {
		var vm = this;

		vm.getDisplayTime = function(booking) {
			return utils.getSeatBookingTimeDisplay(booking, vm.scheduleDate);
		};

		vm.getSeatBookingDetailClass = function (booking) {
			var belongsToDateMoment = moment(booking.BelongsToDate.Date);
			var scheduleDateMoment = moment(vm.scheduleDate);
		
			if (!belongsToDateMoment.isSame(scheduleDateMoment, 'day')) {
				return 'seatmap-seatbooking-previousday';
			}

			return '';
		};
	};

})();


(function() {
	angular.module('wfm.seatMap').directive('seatmapOccupancyDetail', seatmapOccupancyDetailDir);

	function seatmapOccupancyDetailDir() {
		return {
			controller: 'SeatMapOccupancyDetailCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				seatName: '=',
				occupancyDetail: '=',
				scheduleDate: '='
	},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatmapoccupancylist.html"
		};
	};

})();