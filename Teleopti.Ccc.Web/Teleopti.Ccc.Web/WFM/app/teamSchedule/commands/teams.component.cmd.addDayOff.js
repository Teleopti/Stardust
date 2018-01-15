(function () {
	'use strict';
	angular.module('wfm.teamSchedule').component('addDayOff',
		{
			templateUrl: 'app/teamSchedule/commands/teams.component.cmd.addDayOff.html',
			require: {
				containerCtrl: '^teamscheduleCommandContainer'
			},
			controller: AddDayOffCtrl
		});

	AddDayOffCtrl.$inject = ['$scope', 'PersonSelection', 'DayOffService', 'teamScheduleNotificationService'];

	function AddDayOffCtrl($scope, personSelectionSvc, dayOffService, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.runningCommand = false;
		ctrl.label = 'AddDayOff';

		ctrl.$onInit = function () {
			var curDate = moment(ctrl.containerCtrl.getDate()).toDate();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			ctrl.dateRange = {
				startDate: curDate,
				endDate: curDate
			};
			dayOffService.getAllDayOffTemplates().then(function (templates) {
				ctrl.availableTemplates = templates;
			});
		}

		ctrl.isDateRangeValid = function () {
			return ctrl.dateRange.startDate
				&& ctrl.dateRange.endDate
				&& moment(ctrl.dateRange.endDate).isSameOrAfter(moment(ctrl.dateRange.startDate));
		}

		ctrl.isFormValid = function () {
			return $scope.newDayOffForm.$valid
				&& ctrl.isDateRangeValid()
				&& !ctrl.runningCommand
				&& !!(personSelectionSvc.getCheckedPersonInfoList() || []).length;
		}

		ctrl.addDayOff = function () {
			var agents = personSelectionSvc.getCheckedPersonInfoList();
			var personIds = agents.map(function (agent) { return agent.PersonId; });
			var input = {
				PersonIds: personIds,
				StartDate: moment(ctrl.dateRange.startDate).format('YYYY-MM-DD'),
				EndDate: moment(ctrl.dateRange.endDate).format('YYYY-MM-DD'),
				TemplateId: ctrl.selectedTemplateId,
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			};
			ctrl.runningCommand = true;
			dayOffService.addDayOff(input).then(function (response) {
				ctrl.runningCommand = false;
				var actionCb = ctrl.containerCtrl.getActionCb(ctrl.label);
				actionCb && actionCb(ctrl.trackId, personIds);

				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForAddingDayOff',
					warning: 'PartialSuccessMessageForAddingDayOff'
				},
					agents.map(function (x) {
						return {
							PersonId: x.PersonId,
							Name: x.Name
						}
					}),
					response.data);
			});
		}

	}
})();