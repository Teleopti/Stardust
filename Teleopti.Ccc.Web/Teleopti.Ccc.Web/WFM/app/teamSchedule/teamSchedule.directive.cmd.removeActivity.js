﻿(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('removeActivity', removeActivityDirective);

	removeActivityCtrl.$inject = ['ActivityService', 'PersonSelection', 'teamScheduleNotificationService', '$wfmModal', 'ScheduleManagement', 'ScenarioTestUtil'];

	function removeActivityCtrl(ActivityService, PersonSelection, notification, $wfmModal, scheduleManagementSvc, ScenarioTestUtil) {
		var vm = this;
		vm.label = 'RemoveActivity';

		vm.selectedPersonProjections = PersonSelection.getSelectedPersonInfoList();
		vm.removeActivity = function () {
			var personIds = vm.selectedPersonProjections.map(function (x) { return x.PersonId; });
			var personProjectionsWithSelectedActivities = vm.selectedPersonProjections.filter(function (x) {
				return (Array.isArray(x.SelectedActivities) && x.SelectedActivities.length > 0);
			});

			var shiftDates = {};

			vm.selectedPersonProjections.forEach(function(data) {
				if (!Array.isArray(data.SelectedActivities) || data.SelectedActivities.length === 0) return;
				var personScheduleVm = scheduleManagementSvc.findPersonScheduleVmForPersonId(data.PersonId);
				if (!personScheduleVm.Shifts) return;
				
				personScheduleVm.Shifts.forEach(function(shift) {
					if (!shift.Projections) return;
					shift.Projections.forEach(function(projection) {
						if (!projection.ShiftLayerIds) return;
						projection.ShiftLayerIds.forEach(function(shiftId) {
							shiftDates[shiftId] = shift.Date;
						});
					});
				});
			});

			var requestData = {				
				PersonActivities: personProjectionsWithSelectedActivities.map(function(x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name,
						ShiftLayers: x.SelectedActivities.map(function(shiftLayerId) {
							return {
								ShiftLayerId: shiftLayerId,
								Date: shiftDates[shiftLayerId]
							}
						})
					};
				}),
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			ActivityService.removeActivity(requestData).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}
				var personActivities = [];
				personProjectionsWithSelectedActivities.forEach(function (x) {
					x.SelectedActivities.forEach(function (a) {
						personActivities.push({
							PersonId: x.PersonId,
							Name: x.Name,
							Activity: a
						});
					});
				});
				notification.reportActionResult({
					"success": 'SuccessfulMessageForRemovingActivity',
					"warning": 'PartialSuccessMessageForRemovingActivity'
				}, personActivities, response.data);
			});
		};

		vm.popDialog = function () {
			var title = vm.label;
			var message = notification.buildConfirmationMessage(
				'AreYouSureToRemoveSelectedActivity',
				PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.PersonCount,
				PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount,
				true
			);
			$wfmModal.confirm(message, title).then(function (result) {
				vm.resetActiveCmd();

				if (result) {
					vm.removeActivity();
				}
			});
		};

		vm.init = function () {
			if (ScenarioTestUtil.isScenarioTest())
				vm.removeActivity();
			else
				vm.popDialog();
		};
	}


	function removeActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: removeActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'removeActivity'],
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
