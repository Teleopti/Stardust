'use strict';

(function() {
	angular.module('wfm.seatMap').directive('seatmapOccupancyDetail', seatmapOccupancyDetailDir);

	function seatmapOccupancyDetailDir() {
		return {
			scope: {
				seatName:'=',
				occupancyDetail:'='
			},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatmapoccupancylist.html",
			link: function ($scope, elem, attr, ctrl) {
				$scope.getDisplayTime = function(date) {
					return moment(date).format('h:mm a');
				}
			}
		};
	};

})();