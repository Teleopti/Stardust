(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('removeAbsence', removeAbsenceDirective);

	function removeAbsenceDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: removeAbsenceCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'removeAbsence'],
			link: postlink
		};

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

	removeAbsenceCtrl.$inject = ['$scope', 'PersonAbsence', 'PersonSelection', 'teamScheduleNotificationService', '$wfmConfirmModal', 'ScenarioTestUtil'];

	function removeAbsenceCtrl($scope, PersonAbsenceSvc, PersonSelection, notification, $wfmModal, ScenarioTestUtil) {
		var vm = this;
		vm.label = 'RemoveAbsence';

		vm.selectedPersonProjections = PersonSelection.getSelectedPersonInfoList();
		vm.removeAbsence = function () {
			var personIds = vm.selectedPersonProjections.map(function (x) { return x.PersonId; });
			var selectedPersonAbsences = [];

			vm.selectedPersonProjections.forEach(function(selection) {
				if (!selection.SelectedAbsences || selection.SelectedAbsences.length === 0) return;
				var absenceDates = selection.SelectedAbsences.map(function(selectedAbsence) {
					return {
						PersonAbsenceId: selectedAbsence.absenceId,
						Date: selectedAbsence.date
					};
				});

				selectedPersonAbsences.push({
					PersonId: selection.PersonId,
					Name: selection.Name,
					AbsenceDates: absenceDates
				});
			});

			var requestData = {
				SelectedPersonAbsences: selectedPersonAbsences,			
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			return PersonAbsenceSvc.removeAbsence(requestData).then(function (response) {
				$scope.$emit('teamSchedule.hide.loading');
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
					};
				}), response.data);
			});
		};

		vm.popDialog = function () {
			var title = vm.label;
			var message = notification.buildConfirmationMessage(
				'AreYouSureToRemoveSelectedAbsence',
				PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.PersonCount,
				PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount,
				true
			);
			$wfmModal.confirm(message, title).then(function (result) {				
				vm.resetActiveCmd();
				if (result) {
					$scope.$emit('teamSchedule.show.loading');
					vm.removeAbsence();
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
})();
