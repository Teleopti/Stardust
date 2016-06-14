(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('backoutCmd', backoutDirective);

	backoutCtrl.$inject = ['PersonSelection', 'ActivityService', '$mdDialog', 'teamScheduleNotificationService'];

	function backoutCtrl(PersonSelection, ActivityService, $mdDialog, notification) {
		var vm = this;
		vm.label = 'Backout';

		vm.selectedPersonInfo = PersonSelection.getCheckedPersonInfoList();

		var personIds = vm.selectedPersonInfo.map(function (x) { return x.personId; });

		var requestData = {
			PersonIds: personIds,
			Date: vm.selectedDate,
			TrackedCommandInfo: { TrackId: vm.trackId }
		}

		vm.backoutSchedule = function () {
			ActivityService.backoutScheduleChange(requestData).then(function (reponse) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}

				notification.reportActionResult({
					'success': 'SuccessfulMessageForBackoutSchedule',
					'warning': 'PartialSuccessMessageForBackoutSchedule',
					'error': 'FailedMessageForBackoutSchedule'
				}, vm.selectedPersonInfo.map(function (agent) {
					return {
						PersonId: agent.personId,
						Name: agent.name
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
					command: vm.backoutSchedule,
					require: null,
					getCommandMessage: function () {
						return notification.buildConfirmationMessage('AreYouSureToBackoutSelectedSchedule', PersonSelection.getTotalSelectedPersonAndProjectionCount.CheckedPersonCount, null, true);
					}
				}
			});
		};

		vm.init = function () {
			vm.popDialog();
		};
	}

	function backoutDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: backoutCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'backoutCmd'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate();
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.resetActiveCmd = containerCtrl.resetActiveCmd;

			selfCtrl.init();
		}
	}
})();