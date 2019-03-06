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
				isSelectAll: '=?', // has selected all in every page
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

	teamscheduleCommandContainerCtrl.$inject = ['$filter', '$translate', '$element', '$scope', 'guidgenerator', 'teamsPermissions', 'CommandCheckService', 'ScheduleManagement', 'PersonSelection', 'TeamSchedule', 'serviceDateFormatHelper'];

	function teamscheduleCommandContainerCtrl($filter, $translate, $element, $scope, guidgenerator, teamsPermissions, CommandCheckService, scheduleManagementSvc, personSelectionSvc, teamScheduleSvc, serviceDateFormatHelper) {
		var vm = this;
		vm.activityCommands = [
			{
				key: 'AddActivity',
				activityType: 'Normal',
				title: $translate.instant('AddActivity'),
				successfulMessage: $translate.instant('SuccessfulMessageForAddingActivity'),
				warningMessage: $translate.instant('PartialSuccessMessageForAddingActivity')
			},
			{
				key: 'AddPersonalActivity',
				activityType: 'PersonalActivity',
				title: $translate.instant('AddPersonalActivity'),
				successfulMessage: $translate.instant('SuccessfulMessageForAddingActivity'),
				warningMessage: $translate.instant('PartialSuccessMessageForAddingActivity')
			}
		];
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
			vm.setActiveCmd(cmd);

			if (!vm.date) {
				vm.setReady(true);
				return;
			}
			if (!vm.isSelectAll) {
				var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
				if (selectedPersonIds.length === 0) {
					vm.setReady(true);
					return;
				}
				teamScheduleSvc.getSchedules(vm.date, selectedPersonIds).then(function (data) {
					afterLoadSchedules(data.Schedules);
				});
				return;
			}

			teamScheduleSvc.searchSchedules(vm.getLoadAllParams()).then(function (result) {
				afterLoadSchedules(result.data.Schedules);
			});
		
		};

		function afterLoadSchedules(schedules) {
			var date = vm.getDate();
			vm.scheduleManagementSvc.resetSchedules(schedules, date, vm.timezone);
			personSelectionSvc.syncProjectionSelection(vm.scheduleManagementSvc.groupScheduleVm.Schedules, date, vm.isSelectAll);
			vm.setReady(true);
		}

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
