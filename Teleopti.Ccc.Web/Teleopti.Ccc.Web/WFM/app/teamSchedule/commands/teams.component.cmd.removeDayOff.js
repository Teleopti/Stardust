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
			ctrl.getActionCb = ctrl.containerCtrl.getActionCb;
			ctrl.resetActiveCmd = ctrl.containerCtrl.resetActiveCmd;
			ctrl.popDialog();
		}

		ctrl.removeDayOff = function () {
			var personInfos = getCheckedPersonInfoListWithDayOff();
			var personIds = personInfos.map(function(p) { return p.PersonId; });

			var input = {
				Date: moment(ctrl.selectedDate).format('YYYY-MM-DD'),
				PersonIds: personIds,
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			}
			return dayOffService.removeDayOff(input).then(function(response) {
				$scope.$emit('teamSchedule.hide.loading');
				if (ctrl.getActionCb(ctrl.label)) {
					ctrl.getActionCb(ctrl.label)(ctrl.trackId, personIds);
				}

				teamScheduleNotificationService.reportActionResult({
					"success": 'FinishedRemoveDayOff',
					"warning": 'PartialSuccessMessageForRemovingDayOff'
				}, personInfos.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					};
				}), response.data);
			});
		}

		ctrl.popDialog = function () {
			var personInfos = getCheckedPersonInfoListWithDayOff();

			var title = ctrl.label;
			var message = teamScheduleNotificationService.buildConfirmationMessage(
				'AreYouSureToRemoveSelectedDayOff',
				personInfos.length,
				personInfos.length,
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

		function getCheckedPersonInfoListWithDayOff() {
			return personSelectionSvc.getCheckedPersonInfoList().filter(function(p) {
				var dayoffsOnSelectedDay = p.SelectedDayOffs.filter(function(d) {
					return d.Date === moment(ctrl.selectedDate).format('YYYY-MM-DD');
				});
				return dayoffsOnSelectedDay.length > 0;
			});
		}
	
	}
})();
