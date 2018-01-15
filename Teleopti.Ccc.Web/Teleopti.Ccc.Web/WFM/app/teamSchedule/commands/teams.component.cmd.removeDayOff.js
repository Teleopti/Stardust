(function () {

	'use strict';

	angular.module('wfm.teamSchedule').component('removeDayOff',
		{
			require: {
				containerCtrl: '^teamscheduleCommandContainer'
			},
			controller: RemoveDayOffCtrl
		});
	RemoveDayOffCtrl.$inject = ['$scope', '$wfmConfirmModal', 'PersonSelection', 'DayOffService', 'teamScheduleNotificationService'];

	function RemoveDayOffCtrl($scope, $wfmModal, personSelectionSvc, dayOffService, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.label = 'RemoveDayOff';
		

		ctrl.$onInit = function () {
			ctrl.selectedDate = moment(ctrl.containerCtrl.getDate()).toDate();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			ctrl.resetActiveCmd = ctrl.containerCtrl.resetActiveCmd;
			ctrl.popDialog();
		}

		ctrl.removeDayOff = function () {
			var personIds = personSelectionSvc.getCheckedPersonInfoList()
				.map(function (p) { return p.PersonId; });

			var input = {
				Date: moment(ctrl.selectedDate).format('YYYY-MM-DD'),
				PersonIds: personIds,
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			}
			return dayOffService.removeDayOff(input).then(function(response) {
				$scope.$emit('teamSchedule.hide.loading');
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}

				notification.reportActionResult({
					"success": 'FinishedRemoveDayOff',
					"warning": 'PartialSuccessMessageForRemovingDayOff'
				}, selectedPersonAbsences.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					};
				}), response.data);
			});
		}

		ctrl.popDialog = function () {
			var selectedPeople = personSelectionSvc.getCheckedPersonInfoList();
			var title = ctrl.label;
			var message = teamScheduleNotificationService.buildConfirmationMessage(
				'AreYouSureToRemoveSelectedDayOff',
				selectedPeople.length,
				selectedPeople.length,
				true
			);
			$wfmModal.confirm(message, title).then(function (result) {
				ctrl.resetActiveCmd();
				if (result) {
					$scope.$emit('teamSchedule.show.loading');
					ctrl.removeDayOff();
				}
			});
		};
	}
})();
