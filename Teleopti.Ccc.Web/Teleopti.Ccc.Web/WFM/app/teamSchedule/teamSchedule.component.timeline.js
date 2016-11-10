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
			if (changesObj.scheduleCount) {
				if (changesObj.scheduleCount.currentValue > 0) {					
					updateRulerHeight();
				}
			}
		};

		ctrl.$onInit = function() {
			updateRulerHeight();
			angular.element($window).bind('resize', function () {
				updateRulerHeight();				
			});
		};

		function updateRulerHeight() {
			$timeout(function() {
				var height = 0;
				var rows = angular.element($document.find('schedule-table')).find('tr');
				angular.forEach(rows, function(r) {
					height += r.offsetHeight;
				});

				ctrl.height = height - 5;				
			}, 500);
		}

	}

})(angular);
