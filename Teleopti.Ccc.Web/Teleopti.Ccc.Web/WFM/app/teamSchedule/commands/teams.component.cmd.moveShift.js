(function () {
	'use strict';
	angular.module('wfm.teamSchedule').component('moveShift',
		{
			require: {
				containerCtrl: '^teamscheduleCommandContainer'
			},
			templateUrl: 'app/teamSchedule/commands/teams.component.cmd.moveShift.html',
			controller: MoveShiftCtrl
		});

	MoveShiftCtrl.$inject = ['$scope', '$element', '$translate', 'ActivityValidator', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService', 'serviceDateFormatHelper'];

	function MoveShiftCtrl($scope, $element, $translate, validator, personSelectionSvc, activitySvc, teamScheduleNotificationService, serviceDateFormatHelper) {
		var ctrl = this;
		ctrl.label = 'MoveShift';
		ctrl.processingCommand = false;
		ctrl.invalidAgents = [];
		ctrl.agentsInDifferentTimeZone = [];

		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
			ctrl.moveToTime = ctrl.containerCtrl.getDate() + " 08:00";
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			addTabindexAndFocus();
			ctrl.updateInvalidAgents(true);
		};

		ctrl.updateInvalidAgents = function (isTimeValid) {
			var invalidAgents = [];
			findAgentsInDifferentTimezone();
			if (ctrl.agentsInDifferentTimeZone.length > 0) {
				invalidAgents = invalidAgents.concat(ctrl.agentsInDifferentTimeZone);
			}
			if (isTimeValid) {
				validator.validateMoveToTimeForShift(ctrl.containerCtrl.scheduleManagementSvc, getMoveToStartTimeMoment());
				invalidAgents = invalidAgents.concat(validator.getInvalidPeople());
			}
			ctrl.invalidAgents = filterAgentArray(invalidAgents);
		}

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};
		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};
		ctrl.anyAgentsInDifferentTimeZone = function () {
			return ctrl.agentsInDifferentTimeZone.length > 0;
		};

		ctrl.moveShift = function () {
			var invalidAgentIds = ctrl.invalidAgents.map(function (a) {
				return a.PersonId;
			});
			var agentsInDifferentTimeZone = ctrl.agentsInDifferentTimeZone.map(function (a) {
				return a.PersonId;
			});
			invalidAgentIds = invalidAgentIds.concat(agentsInDifferentTimeZone);

			var validAgents = ctrl.selectedAgents.filter(function (agent) {
				return invalidAgentIds.indexOf(agent.PersonId) < 0;
			});

			var validAgentIds = validAgents.map(function (agent) {
				return agent.PersonId;
			});

			if (validAgentIds.length > 0) {
				var requestData = {
					Date: ctrl.containerCtrl.getDate(),
					NewShiftStart: ctrl.containerCtrl.getServiceTimeInCurrentUserTimezone(getMoveToStartTimeMoment()),
					PersonIds: validAgentIds,
					TrackedCommandInfo: { TrackId: ctrl.trackId }
				};

				ctrl.processingCommand = true;

				activitySvc.moveShift(requestData)
					.then(function (response) {
						if (ctrl.containerCtrl.getActionCb(ctrl.label)) {
							ctrl.containerCtrl.getActionCb(ctrl.label)(ctrl.trackId, validAgentIds);
						}
						teamScheduleNotificationService.reportActionResult({
							success: $translate.instant('SuccessfulMessageForMovingShift'),
							warning: $translate.instant('PartialSuccessMessageForMovingShift')
						},
							validAgents.map(function (x) {
								return {
									PersonId: x.PersonId,
									Name: x.Name
								}
							}),
							response.data);

						ctrl.processingCommand = false;
					});
			} else {
				if (ctrl.containerCtrl.getActionCb(ctrl.label)) {
					ctrl.containerCtrl.getActionCb(ctrl.label)(ctrl.trackId, []);
				}
			}
		};

		function addTabindexAndFocus() {
			var tabindex = angular.isDefined($element.attr.tabindex) ? $element.attr.tabindex : '0';
			var addTabIndexTo = function () {
				angular.forEach(arguments, function (arg) {
					angular.forEach(arg, function (elem) {
						elem.setAttribute('tabIndex', tabindex);
					});
				});
			};

			addTabIndexTo(
				$element[0].querySelectorAll('div[uib-timepicker]'),
				$element[0].querySelectorAll('button#applyMoveShift')
			);

			$scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = $element[0].querySelector('.focus-default input');
				if (focusTarget) angular.element(focusTarget).focus();
			});

			var inputs = $element[0].querySelectorAll('input[type=text]');
			angular.forEach(inputs, function (input) {
				angular.element(input).on('focus', function (event) {
					event.target.select();
				});
			});
		}

		function getMoveToStartTimeMoment() {
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			return moment.tz(ctrl.moveToTime, currentTimezone)
		}

		function filterAgentArray(arr) {
			var map = {};
			arr.forEach(function (agent) {
				map[agent.PersonId] = {
					PersonId: agent.PersonId,
					Name: agent.Name
				};
			});
			var personIds = Object.keys(map);
			return personIds.map(function (key) { return map[key]; });
		}

		function findAgentsInDifferentTimezone() {
			ctrl.agentsInDifferentTimeZone.splice(0);
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			ctrl.selectedAgents.forEach(function (agent) {
				var personSchedule = ctrl.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId);
				if (currentTimezone != personSchedule.Timezone.IanaId) {
					ctrl.agentsInDifferentTimeZone.push(agent);
				}
			});
		}

	}
})();