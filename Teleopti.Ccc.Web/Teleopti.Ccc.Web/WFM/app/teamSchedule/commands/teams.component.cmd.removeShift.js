(function () {
	'use strict';
	angular.module('wfm.teamSchedule').component('removeShift',
		{
			require: {
				containerCtrl: '^teamscheduleCommandContainer'
			},
			controller: RemoveShiftCtrl
		});
	RemoveShiftCtrl.$inject = ['$scope', '$wfmConfirmModal', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];
	function RemoveShiftCtrl($scope, $wfmModal, personSelectionSvc, activityService, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.runningCommand = false;
		ctrl.label = 'RemoveShift';

		ctrl.$onInit = function () {
			ctrl.selectedDate = moment(ctrl.containerCtrl.getDate()).toDate();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			ctrl.getActionCb = ctrl.containerCtrl.getActionCb;
			ctrl.resetActiveCmd = ctrl.containerCtrl.resetActiveCmd;
			ctrl.popDialog();
		}

		ctrl.popDialog = function () {
			var personInfos = getCheckedPersonInfoListWithShift();
			var title = ctrl.label;
			var message = teamScheduleNotificationService.buildConfirmationMessage(
				'AreYouSureToRemoveSelectedShift',
				personInfos.length,
				personInfos.length,
				true
			);
			$wfmModal.confirm(message, title).then(function (result) {
				ctrl.resetActiveCmd();
				if (result) {
					$scope.$emit('teamSchedule.show.loading');
					ctrl.removeShift();
				}
			});
		}
		ctrl.removeShift = function () {
			var personInfos = getCheckedPersonInfoListWithShift();
			var personIds = personInfos.map(function (p) { return p.PersonId; });
			var input = {
				Date: moment(ctrl.selectedDate).format('YYYY-MM-DD'),
				PersonIds: personIds,
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			}
			return activityService.removeShift(input).then(function (response) {
				$scope.$emit('teamSchedule.hide.loading');
				if (ctrl.getActionCb(ctrl.label)) {
					ctrl.getActionCb(ctrl.label)(ctrl.trackId, personIds);
				}
				teamScheduleNotificationService.reportActionResult({
					"success": 'FinishedRemoveShift',
					"warning": 'PartialSuccessMessageForRemovingShift'
				}, personInfos.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					};
				}), response.data);
			});
		}

		function getCheckedPersonInfoListWithShift() {
			var selectedPersonInfoList = personSelectionSvc.getCheckedPersonInfoList();

			return selectedPersonInfoList.filter(function (p) {
				return p.Checked && p.SelectedActivities && p.SelectedActivities.filter(function (a) { return !a.isOvertime; }).length > 0;
			});
		}



	}

})();