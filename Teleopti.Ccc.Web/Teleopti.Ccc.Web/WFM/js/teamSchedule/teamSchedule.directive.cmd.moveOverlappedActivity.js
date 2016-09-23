(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveOverlappedActivity', moveOverlappedActivityDirective);

	moveOverlappedActivityCtrl.$inject = ['PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];

	function moveOverlappedActivityCtrl(PersonSelection, ActivityService, notification) {
		var vm = this;
		vm.label = 'MoveInvalidOverlappedActivity';
		vm.init = function(){
			moveOverlappedActivity();
		}

		function moveOverlappedActivity(){
			var selectedPersonInfo = PersonSelection.getCheckedPersonInfoList();
			var selectedPersonIds = selectedPersonInfo.map(function (p) {
				return p.PersonId;
			});
			var cmdInput = {
				PersonIds: selectedPersonIds,
				Date: vm.selectedDate(),
				TrackedCommandInfo: { TrackId: vm.trackId }
			}
			ActivityService.moveInvalidOverlappedActivity(cmdInput).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, selectedPersonIds);
				}

				notification.reportActionResult({
					"success": 'MoveInvalidOverlappedActivitySuccess',
					"warning": 'MoveInvalidOverlappedActivityWarning'
				}, selectedPersonInfo.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					}
				}), response.data);
			})
		}
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