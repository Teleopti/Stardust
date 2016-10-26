(function (angular) {
	'use strict';

	angular.module('wfm.teamSchedule').component('timeLine', {
		controller: TimeLineCtrl,
		templateUrl: 'app/teamSchedule/html/timeline.html',
		bindings: {
			times: '<',
			scheduleCount: '<'
		},
	});

	function TimeLineCtrl() {
		var ctrl = this;
		ctrl.height = 0;
		ctrl.$onChanges = function (changesObj) {
			if (changesObj.scheduleCount) {
				if (changesObj.scheduleCount.currentValue > 0) {
					var headerHeight = 25; //angular.element($("#time-line-container"))[0].offsetHeight
					var labelHeight = 12; //angular.element($(".label-info"))[0].offsetHeight;
					var rowHeight = 31;

					ctrl.height = rowHeight * (ctrl.scheduleCount) + headerHeight + labelHeight;
				}
			}
		};
	}

})(angular);
