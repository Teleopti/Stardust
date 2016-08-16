(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('removeAbsence', removeAbsenceDirective);

	removeAbsenceCtrl.$inject = ['PersonAbsence', 'PersonSelection', 'teamScheduleNotificationService', '$wfmModal', 'ScenarioTestUtil'];

	function removeAbsenceCtrl(PersonAbsenceSvc, PersonSelection, notification, $wfmModal, ScenarioTestUtil) {
		var vm = this;
		vm.label = 'RemoveAbsence';

		vm.selectedPersonProjections = PersonSelection.getSelectedPersonInfoList();
		vm.removeAbsence = function () {
			var personIds = vm.selectedPersonProjections.map(function (x) { return x.PersonId; });
			var selectedPersonAbsences = vm.selectedPersonProjections.filter(function (x) {
				return x.PersonAbsenceCount > 0;
			});
			var requestData = {
				ScheduleDate: moment(vm.selectedDate()).format("YYYY-MM-DD"),
				SelectedPersonAbsences: selectedPersonAbsences.map(function (x) {
					return { PersonId: x.PersonId, Name: x.Name, PersonAbsenceIds: x.SelectedAbsences };
				}),
				RemoveEntireCrossDayAbsence: false,
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			PersonAbsenceSvc.removeAbsence(requestData).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}

				notification.reportActionResult({
					"success": 'FinishedRemoveAbsence',
					"warning": 'PartialSuccessMessageForRemovingAbsence'
				}, selectedPersonAbsences.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					}
				}), response.data);
			});
		};

		vm.popDialog = function () {
			return $wfmModal.show({
				onRemoving: function () {
					vm.resetActiveCmd();
				},
				locals: {
					commandTitle: vm.label,
					fix: null,
					getTargets: PersonSelection.getSelectedPersonIdList,
					command: vm.removeAbsence,
					require: null,
					getCommandMessage: function () {
						return notification.buildConfirmationMessage(
							'AreYouSureToRemoveSelectedAbsence',
							PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.PersonCount,
							PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount,
							true);
					}
				}
			});
		};

		vm.init = function () {
			if (ScenarioTestUtil.isScenarioTest())
				vm.removeAbsence();
			else
				vm.popDialog();
		};
	}

	function removeAbsenceDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: removeAbsenceCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'removeAbsence'],
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
