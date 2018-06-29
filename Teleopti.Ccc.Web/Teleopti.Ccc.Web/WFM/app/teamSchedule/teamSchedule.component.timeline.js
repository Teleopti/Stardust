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

	TimeLineCtrl.$inject = ['$timeout', '$document', '$window'];
	function TimeLineCtrl($timeout, $document, $window) {
		var ctrl = this;
		ctrl.height = 0;
		ctrl.$onChanges = function (changesObj) {
			if (changesObj.scheduleCount && !!changesObj.scheduleCount.currentValue) {
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
				var tHeader = $document[0].querySelector('schedule-table thead');
				ctrl.height = (ctrl.scheduleCount * 32.5) + tHeader.offsetHeight - 5;
			});
		}

	}

})(angular);
