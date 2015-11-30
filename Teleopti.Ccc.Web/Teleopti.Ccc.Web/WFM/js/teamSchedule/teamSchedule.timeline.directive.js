'use strict';

(function () {
	var timeLineCtrl = function ($scope) {
		var vm = this;
		vm.height = 0;
		$scope.$watch(function () {
			return vm.scheduleCount;
		}, function (newValue) {
			if (newValue > 0) {
				var headerHeight = 17; //angular.element($("#time-line-container"))[0].offsetHeight
				var labelHeight = 12; //angular.element($(".label-info"))[0].offsetHeight;
				var rowHeight = 41;

				vm.height = rowHeight * (vm.scheduleCount) + headerHeight + labelHeight;
			}
		});
	}
	var directive = function () {
		return {
			controller: 'TimeLineCtrl',
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: "js/teamSchedule/html/timeline.html",
			scope: {
				times: '=?',
				scheduleCount: '=?'
			},
			linkFunction: linkFunction
		};
	};
	angular.module('wfm.teamSchedule')
		.directive('timeLine', directive)
		.controller('TimeLineCtrl', ['$scope', timeLineCtrl]);

	function linkFunction(scope, element, attributes, vm) {
	};
}());
