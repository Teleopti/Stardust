(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('undoSchedule', undoScheduleDirective);

	undoScheduleCtrl.$inject = ['PersonSelection', 'ActivityService', '$mdDialog', 'teamScheduleNotificationService', 'ScenarioTestUtil'];

	function undoScheduleCtrl(PersonSelection, ActivityService, $mdDialog, notification, ScenarioTestUtil) {
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
			return $mdDialog.show({
				controller: 'commandConfirmDialog',
				templateUrl: 'js/teamSchedule/html/commandConfirmDialog.tpl.html',
				parent: angular.element(document.body),
				clickOutsideToClose: true,
				bindToController: true,
				onRemoving: function() {
					vm.resetActiveCmd();
				},
				locals: {
					commandTitle: vm.label,
					fix: null,
					getTargets: PersonSelection.getSelectedPersonIdList,
					command: vm.undoSchedule,
					require: null,
					getCommandMessage: function () {
						return notification.buildConfirmationMessage('AreYouSureToUndoSelectedSchedule', PersonSelection.getTotalSelectedPersonAndProjectionCount.CheckedPersonCount, null, true);
					}
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