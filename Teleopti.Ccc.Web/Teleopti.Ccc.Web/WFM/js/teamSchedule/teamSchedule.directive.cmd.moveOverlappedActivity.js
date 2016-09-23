(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveOverlappedActivity', moveOverlappedActivityDirective);

	moveOverlappedActivityCtrl.$inject = ['PersonSelection', 'ActivityService', '$wfmModal', 'teamScheduleNotificationService', 'ScenarioTestUtil'];

	function moveOverlappedActivityCtrl(PersonSelection, ActivityService, notification, ScenarioTestUtil) {
		var vm = this;
		vm.label = 'MoveInvalidOverlappedActivity';
	}

	function moveOverlappedActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: moveOverlappedActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'moveOverlappedActivity'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.resetActiveCmd = containerCtrl.resetActiveCmd;

			selfCtrl.init();
		}
	}
})();