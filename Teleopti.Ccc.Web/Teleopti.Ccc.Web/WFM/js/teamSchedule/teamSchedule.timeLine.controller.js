(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TimeLineCtrl', ['ShiftHelper', TimeLineController]);
	function TimeLineController(shiftHelper) {
		var vm = this;
		vm.widthInPixels = function() {
			return vm.parentVm.getTimeLineWidth();
		}
	}
}());