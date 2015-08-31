'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatMapOccupancyDetailCtrl', locationPickerDirectiveController);
	locationPickerDirectiveController.$inject = ['seatMapCanvasUtilsService'];

	function locationPickerDirectiveController(utils) {
		var vm = this;

		vm.getDisplayTime = function (booking) {
			return utils.getSeatBookingTimeDisplay(booking);
		}
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
				seatName:'=',
				occupancyDetail:'='
			},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatmapoccupancylist.html"
		};
	};

})();