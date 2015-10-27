(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TimeLineCtrl', ['ShiftHelper', TimeLineController]);
	function TimeLineController(shiftHelper) {
		var vm = this;
		//var init = function() {
		//	vm.Times = vm.data.HourPoints;
		//	console.log("timeLineCtrl ", vm.data);
		//}

		//init();
	}
}());