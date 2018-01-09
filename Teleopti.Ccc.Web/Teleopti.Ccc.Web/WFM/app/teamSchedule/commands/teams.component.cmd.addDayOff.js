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

	AddDayOffCtrl.$inject = ['$scope','PersonSelection', 'DayOffService', 'teamScheduleNotificationService'];

	function AddDayOffCtrl($scope, personSelectionSvc, dayOffService, teamScheduleNotificationService) {
		var ctrl = this;
		dayOffService.getAvailableTemplates().then(function (templates) {
			ctrl.availableTemplates = templates;
		})

		ctrl.$onInit = function () {
			var curDate = moment(ctrl.containerCtrl.getDate()).toDate();
			ctrl.dateRange = {
				startDate: curDate,
				endDate: curDate
			};
		}
		ctrl.isFormValid = function () {
			var dateValid = ctrl.dateRange.startDate
				&& ctrl.dateRange.endDate
				&& moment(ctrl.dateRange.endDate).isSameOrAfter(moment(ctrl.dateRange.startDate));

			return $scope.newDayOffForm.$valid && dateValid;
		}
	}
})();