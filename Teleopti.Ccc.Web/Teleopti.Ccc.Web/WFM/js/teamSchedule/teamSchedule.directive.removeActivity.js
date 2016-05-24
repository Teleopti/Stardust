﻿(function() {

	'use strict';

	angular.module('wfm.teamSchedule').directive('removeActivity', removeActivityDirective);

	removeActivityCtrl.$inject = ['ActivityService', 'PersonSelection', 'teamScheduleNotificationService', '$mdDialog'];

	function removeActivityCtrl(ActivityService, PersonSelection, notification, $mdDialog) {
		var vm = this;
		vm.label = 'RemoveActivity';

		vm.selectedPersonProjections = PersonSelection.getSelectedPersonInfoList();
		vm.removeActivity = function () {
			var personIds = vm.selectedPersonProjections.map(function(x) { return x.personId; });
			var requestData = {
				Date: vm.referenceDay(),
				PersonActivities: vm.selectedPersonProjections.map(function(x) {
					return { PersonId: x.personId, Name: x.name, ShiftLayerIds: x.selectedActivities };
				}),
				TrackedCommandInfo: { TrackId: vm.trackId }
			}

			ActivityService.removeActivity(requestData).then(function(response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.TrackId, personIds);
				}

				notification.reportActionResult({
					"success": 'SuccessfulMessageForRemovingActivity',
					"warning": 'PartialSuccessMessageForRemovingActivity'
				}, vm.selectedPersonProjections.map(function (x) {
					return {
						PersonId: x.personId,
						Name: x.name
					}
				}), response.data);
			});
		}


		// ToDo[Yanyi] refactor write protection into commandContainer.
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
					command: vm.removeActivity,
					require: null,
					getCommandMessage: function() {
						return notification.buildConfirmationMessage(
							'AreYouSureToRemoveSelectedActivity',
							PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.PersonCount,
							PersonSelection.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount,
							true)}
				}
			});
		}
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

			scope.vm.referenceDay = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.resetActiveCmd = containerCtrl.resetActiveCmd;

			// trigger command directly
			selfCtrl.popDialog();
		}

	}


})();