(function (angular) {
	'use strict';

	angular.module('wfm.teamSchedule').component('timeLine', {
		controller: TimeLineCtrl,
		templateUrl: 'app/teamSchedule/html/timeline.html',
		bindings: {
			times: '<',
			scheduleCount: '<',
			isScheduleInEditing: '<'
		},
	});

	TimeLineCtrl.$inject = ['$timeout', '$window'];
	function TimeLineCtrl($timeout,  $window) {
		var ctrl = this;
		ctrl.height = 0;
		ctrl.$onChanges = function (changesObj) {
			if ((changesObj.scheduleCount && !!changesObj.scheduleCount.currentValue)
				|| changesObj.isScheduleInEditing) {
				updateRulerHeight();
			}
		};

		ctrl.$onInit = function () {
			updateRulerHeight();
			angular.element($window).bind('resize', function () {
				updateRulerHeight();
			});
		};


		function updateRulerHeight() {
			$timeout(function () {
				var editingHeight = ctrl.isScheduleInEditing ? 210 : 0;
				ctrl.height = (ctrl.scheduleCount * 32.5) + 40 - 5 + editingHeight;
			});
		}

	}

})(angular);
