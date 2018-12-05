(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivity', ['serviceDateFormatHelper', moveActivityDirective]);

	function moveActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: moveActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.moveActivity.html',
			require: ['^teamscheduleCommandContainer', 'moveActivity'],
			link: function (scope, elem) {
				scope.$on('teamSchedule.command.focus.default', function () {
					var focusTarget = elem[0].querySelector('.focus-default input');
					if (focusTarget) angular.element(focusTarget).focus();
				});

				var inputs = elem[0].querySelectorAll('input[type=text]');
				angular.forEach(inputs, function (input) {
					angular.element(input).on('focus', function (event) {
						event.target.select();
					});
				});
			}
		};
	}

	moveActivityCtrl.$inject = ['$attrs', '$scope', '$translate', 'ActivityService', 'PersonSelection', 'ScheduleHelper', 'teamScheduleNotificationService', 'ActivityValidator', 'CommandCheckService', 'serviceDateFormatHelper'];

	function moveActivityCtrl($attrs, $scope, $translate, activityService, personSelectionSvc, scheduleHelper, teamScheduleNotificationService, validator, CommandCheckService, serviceDateFormatHelper) {
		var vm = this;
		vm.label = 'MoveActivity';
		vm.tabindex = angular.isDefined($attrs.tabindex) ? $attrs.tabindex : 0;
		vm.checkingCommand = false;
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		vm.invalidAgents = [];

		var containerCtrl = $scope.$parent.vm;
		vm.containerCtrl = containerCtrl;
		vm.selectedDate = containerCtrl.getDate;
		vm.trackId = containerCtrl.getTrackId();
		vm.convertTime = containerCtrl.getServiceTimeInCurrentUserTimezone;
		vm.getActionCb = containerCtrl.getActionCb;
		vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
		vm.scheduleMgtSvc = containerCtrl.scheduleManagementSvc;

		var startTimeMoment = getDefaultMoveToStartTimeMoment();
		vm.moveToTime = serviceDateFormatHelper.getDateTime(startTimeMoment);
		vm.nextDay = serviceDateFormatHelper.getDateOnly(startTimeMoment) !== vm.selectedDate();

		vm.getMoveToDate = function () {
			return vm.nextDay ?
				serviceDateFormatHelper.getDateOnly(moment.tz(vm.selectedDate(), vm.getCurrentTimezone()).add(1, 'days')) : vm.selectedDate();
		}

		vm.anyValidAgent = function () {
			return vm.invalidAgents.length !== vm.selectedAgents.length;
		}

		vm.anyInvalidAgent = function () {
			return vm.invalidAgents.length > 0;
		};

		vm.updateInvalidAgents = function (isFormValid) {
			if (!isFormValid) return;
			var currentTimezone = vm.getCurrentTimezone();
			validator.validateMoveToTime(vm.scheduleMgtSvc, moment.tz(vm.moveToTime, currentTimezone));
			vm.invalidAgents = validator.getInvalidPeople();
		};

		vm.invalidPeople = function () {
			var people = validator.getInvalidPeopleNameList().join(', ');
			return people;
		};

		vm.moveActivity = function () {
			var requestData = getRequestData();
			vm.checkingCommand = true;
			CommandCheckService.checkMoveActivityOverlapping(requestData).then(function (data) {
				moveActivity(data);
			});
		}

		function init() {
			vm.updateInvalidAgents(true);
		}
		init();

		function moveActivity(requestData) {
			if (requestData.PersonActivities.length > 0) {
				activityService.moveActivity(requestData).then(function (response) {
					var personIds = requestData.PersonActivities.map(function (agent) { return agent.PersonId; });
					if (vm.getActionCb(vm.label)) {
						vm.getActionCb(vm.label)(vm.trackId, personIds);
					}
					teamScheduleNotificationService.reportActionResult({
						success: $translate.instant('SuccessfulMessageForMovingActivity'),
						warning: $translate.instant('PartialSuccessMessageForMovingActivity')
					}, vm.selectedAgents.map(function (x) {
						return {
							PersonId: x.PersonId,
							Name: x.Name
						}
					}), response.data);

					vm.checkingCommand = false;
				});
			} else {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, []);
				}
				vm.checkingCommand = false;
			}
		}

		function getRequestData() {
			var invalidPersonIds = vm.invalidAgents.map(function (p) { return p.PersonId });

			var validAgents = vm.selectedAgents.filter(function (agent) {
				return invalidPersonIds.indexOf(agent.PersonId) < 0;
			}).filter(function (x) {
				return (angular.isArray(x.SelectedActivities));
			});

			var personActivities = [];
			angular.forEach(validAgents, function (agent) {
				var selectedActivities = agent.SelectedActivities;
				var groupedActivitiesByDate = {};
				selectedActivities.forEach(function (a) {
					if (!groupedActivitiesByDate[a.date]) {
						groupedActivitiesByDate[a.date] = [];
					}
					if (groupedActivitiesByDate[a.date].indexOf(a.shiftLayerId) === -1) {
						groupedActivitiesByDate[a.date].push(a.shiftLayerId);
					}
				});
				for (var date in groupedActivitiesByDate) {
					personActivities.push({
						PersonId: agent.PersonId,
						Date: date,
						ShiftLayerIds: groupedActivitiesByDate[date]
					});
				}
			});

			var requestData = {
				PersonActivities: personActivities,
				StartTime: vm.convertTime(vm.moveToTime),
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			return requestData;
		}

		function getDefaultMoveToStartTimeMoment() {
			var currentTimezone = vm.getCurrentTimezone();
			var curDate = vm.selectedDate();
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.scheduleMgtSvc.schedules();
			
			var selectedDateProjectionLatestStartMoment = scheduleHelper.getLatestStartTimeMomentOfSelectedProjections(schedules, personIds);
			var previousDateProjectionLatestEndMoment = scheduleHelper.getLatestPreviousDayOvernightShiftEndMoment(schedules, curDate, personIds);
			var timeMoment = moment.tz(curDate, currentTimezone);

			timeMoment = selectedDateProjectionLatestStartMoment || timeMoment;
			timeMoment = previousDateProjectionLatestEndMoment != null && previousDateProjectionLatestEndMoment.isAfter(timeMoment) ? previousDateProjectionLatestEndMoment : timeMoment;

			return timeMoment.add(1, 'hour');
		}
	}
})();
