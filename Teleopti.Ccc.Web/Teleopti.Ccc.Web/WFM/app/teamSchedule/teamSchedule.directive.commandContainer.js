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

	teamscheduleCommandContainerCtrl.$inject = ['$q', '$filter', '$element', '$scope', 'guidgenerator', 'teamsPermissions', 'CommandCheckService', 'ScheduleManagement', 'PersonSelection', 'TeamSchedule','serviceDateFormatHelper'];

	function teamscheduleCommandContainerCtrl($q, $filter, $element, $scope, guidgenerator, teamsPermissions, CommandCheckService, scheduleManagementSvc, personSelectionSvc, teamScheduleSvc, serviceDateFormatHelper) {
		var vm = this;

		vm.scheduleManagementSvc = scheduleManagementSvc.newService();

		vm.ready = false;

		var wrapperEl = $element[0].querySelector('.teamschedule-command-container');
		wrapperEl.addEventListener('keydown', function (event) {
			var isEscape = (event.keyCode === 27);
			if (isEscape && vm.activeCmd) {
				vm.resetActiveCmd();
				event.stopPropagation();
				$scope.$emit('teamSchedule.sidenav.hide');
			}
		});

		vm.initCmd = function (cmd) {
			if (!vm.date) {
				vm.setReady(true);
			} else {

				if (vm.isSelectAll) {
					teamScheduleSvc.searchSchedules(vm.getLoadAllParams()).then(function (result) {
						vm.scheduleManagementSvc.resetSchedules(result.data.Schedules, vm.getDate(), vm.timezone);
						personSelectionSvc.selectAllPerson(vm.scheduleManagementSvc.groupScheduleVm.Schedules);
						personSelectionSvc.updatePersonInfo(vm.scheduleManagementSvc.groupScheduleVm.Schedules);
						vm.setReady(true);
					});
				} else {
					var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
					if (selectedPersonIds.length === 0) {
						vm.setReady(true);
					} else {
						teamScheduleSvc.getSchedules(vm.date, selectedPersonIds).then(function (data) {
							vm.scheduleManagementSvc.resetSchedules(data.Schedules, vm.getDate(), vm.timezone);
							personSelectionSvc.syncProjectionSelection(vm.scheduleManagementSvc.schedules());
							vm.setReady(true);
						});
					}
				}
			}
			vm.setActiveCmd(cmd);
		};

		vm.setReady = function (value) {
			vm.ready = value;
		};

		vm.getDate = function () {
			return serviceDateFormatHelper.getDateOnly(vm.date);
		};

		vm.getCurrentTimezone = function () { return vm.timezone; };

		vm.convertTimeToCurrentUserTimezone = function (time) {
			return $filter('timezone')(time, null, vm.timezone);
		};

		vm.getServiceTimeInCurrentUserTimezone = function (time) {
			return $filter('serviceTimezone')(time, null, vm.timezone);
		}


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

		vm.activeCommandCheck = function () {
			return CommandCheckService.getCommandCheckStatus();
		};
	}
})(angular, moment);
