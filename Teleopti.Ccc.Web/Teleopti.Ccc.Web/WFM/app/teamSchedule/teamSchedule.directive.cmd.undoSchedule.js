(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('undoSchedule', undoScheduleDirective);

	undoScheduleCtrl.$inject = ['PersonSelection', 'ActivityService', '$wfmModal', 'teamScheduleNotificationService', 'ScenarioTestUtil'];

	function undoScheduleCtrl(PersonSelection, ActivityService, $wfmModal, notification, ScenarioTestUtil) {
		var vm = this;
		vm.label = 'Undo';

		vm.selectedPersonInfo = PersonSelection.getCheckedPersonInfoList();

		var personIds = vm.selectedPersonInfo.map(function (x) { return x.PersonId; });

		vm.undoSchedule = function () {

			var requestData = {
				PersonIds: personIds,
				Date: vm.selectedDate(),
				TrackedCommandInfo: { TrackId: vm.trackId }
			}

			ActivityService.undoScheduleChange(requestData).then(function (reponse) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}

				notification.reportActionResult({
					'success': 'SuccessfulMessageForUndoSchedule',
					'warning': 'PartialSuccessMessageForUndoSchedule',
					'error': 'FailedMessageForUndoSchedule'
				}, vm.selectedPersonInfo.map(function (agent) {
					return {
						PersonId: agent.PersonId,
						Name: agent.Name
					}
				}), reponse.data);
			});
		};

		vm.popDialog = function() {
			var title = vm.label;
			var message = notification.buildConfirmationMessage(
				'AreYouSureToUndoSelectedSchedule',
				PersonSelection.getTotalSelectedPersonAndProjectionCount.CheckedPersonCount,
				null,
				true
			);
			$wfmModal.confirm(message, title).then(function (result) {
				vm.resetActiveCmd();

				if (result) {
					vm.undoSchedule();
				}
			});
		};

		vm.init = function () {
			if (!ScenarioTestUtil.isScenarioTest()) {
				vm.popDialog();
			} else {
				vm.undoSchedule();
			}
		};
	}

	function undoScheduleDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: undoScheduleCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'undoSchedule'],
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