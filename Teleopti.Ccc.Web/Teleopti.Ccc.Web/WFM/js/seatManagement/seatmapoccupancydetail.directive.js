'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatMapOccupancyDetailCtrl', locationPickerDirectiveController);
	locationPickerDirectiveController.$inject = ['seatMapCanvasUtilsService','seatMapService','growl'];

	function locationPickerDirectiveController(utils, seatMapService, growl) {
		var vm = this;

		vm.getDisplayTime = function(booking) {
			return utils.getSeatBookingTimeDisplay(booking, vm.scheduleDate);
		};

		vm.deleteSeatBooking = function (booking) {
			seatMapService.occupancy.remove({ Id: booking.BookingId }).$promise.then(function () {
				vm.refreshSeatMap();
				onSuccessDeleteSeatBooking('The booking of seat '
					+ booking.SeatName + ' for ' + booking.FirstName + ' ' + booking.LastName + ' was deleted');
			});
		};

		vm.getSeatBookingDetailClass = function (booking) {
			var belongsToDateMoment = moment(booking.BelongsToDate.Date);
			var scheduleDateMoment = moment(vm.scheduleDate);
		
			if (!belongsToDateMoment.isSame(scheduleDateMoment, 'day')) {
				return 'seatmap-seatbooking-previousday';
			}

			return '';
		};

		function onSuccessDeleteSeatBooking(message) {
			growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
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
				scheduleDate: '=',
				refreshSeatMap : '&'
	},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatmapoccupancylist.html"
		};
	};

})();