(function (angular, moment) {

	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('teamscheduleCommandContainer', teamscheduleCommandContainer);

	function teamscheduleCommandContainer() {
		return {
			restrict: 'E',
			controller: teamscheduleCommandContainerCtrl,
			scope: {
				date: '=',
				timezone: '=',
				actionCallback: '&?',
				onReady: '&',
				isSelectAll: '=?',
				getLoadAllParams: '&?'
			},
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/teamscheduleCommandContainer.html',
			link: function (scope, elem) {
				scope.$on('teamSchedule.init.command', function (e, d) {
					scope.vm.initCmd(d.activeCmd);
				});

				scope.$on('teamSchedule.reset.command', function (e, d) {
					scope.vm.resetActiveCmd();
				});
			}
		};
	}

	teamscheduleCommandContainerCtrl.$inject = ['$q', '$filter', '$scope', 'guidgenerator', 'teamsToggles',  'teamsPermissions',  'CommandCheckService', 'ScheduleManagement', 'PersonSelection', 'TeamSchedule'];

	function teamscheduleCommandContainerCtrl($q, $filter, $scope, guidgenerator, teamsToggles, teamsPermissions, CommandCheckService, scheduleManagementSvc, personSelectionSvc, teamScheduleSvc) {
		var vm = this;

		vm.scheduleManagementSvc = scheduleManagementSvc.newService();

		vm.ready = false;

		vm.initCmd = function(cmd) {
			if (vm.isSelectAll) {
				teamScheduleSvc.searchSchedules(vm.getLoadAllParams()).then(function(result) {
					vm.scheduleManagementSvc.resetSchedules(result.data.Schedules, moment(vm.date), vm.timezone);
					personSelectionSvc.selectAllPerson(vm.scheduleManagementSvc.groupScheduleVm.Schedules);
					personSelectionSvc.updatePersonInfo(vm.scheduleManagementSvc.groupScheduleVm.Schedules);
					vm.setReady(true);
				});
			} else {
				var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
				if (selectedPersonIds.length === 0) {
					vm.setReady(true);
				} else {
					teamScheduleSvc.getSchedules(vm.date, selectedPersonIds).then(function(data) {
						vm.scheduleManagementSvc.resetSchedules(data.Schedules, moment(vm.date), vm.timezone);
						vm.setReady(true);
					});
				}
			}

			vm.setActiveCmd(cmd);
		};

		vm.setReady = function(value) {
			vm.ready = value;
		};

		vm.getDate = function () {
			return moment(vm.date).format('YYYY-MM-DD');
		};

		vm.getCurrentTimezone = function () { return vm.timezone; };

		vm.convertTimeToCurrentUserTimezone = function(time) {
			return $filter('timezone')(time, null, vm.timezone);
		};

		vm.getTrackId = guidgenerator.newGuid;

		vm.activeCmd = null;

		vm.setActiveCmd = function (label) {
			vm.activeCmd = label;
		};

		vm.resetActiveCmd = function () {
			vm.activeCmd = null;
			vm.setReady(false);
		};

		vm.getActionCb = function (_) {
			var returnFn = function (trackId, personIds) {
				vm.resetActiveCmd();
				if (vm.actionCallback) {
					vm.actionCallback({
						trackId: trackId,
						personIds: personIds
					});
				}
			};
			return returnFn;
		};

		vm.setActionCb = function (_, cb) {
			vm.actionCallback = cb;
		};

		vm.hasPermission = function (permission) {
			return teamsPermissions.all()[permission];
		};

		vm.hasToggle = function (toggle) {
			return teamsToggles.all()[toggle];
		};

		vm.activeCommandCheck = function(){
			return CommandCheckService.getCommandCheckStatus();
		};
	}
})(angular, moment);
