﻿(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('moveShift',
	{
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		templateUrl: 'app/teamSchedule/commands/teams.component.cmd.moveShift.html',
		controller: MoveShiftCtrl
	});

	MoveShiftCtrl.$inject = ['$scope', '$locale', '$element', 'ActivityValidator', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService','serviceDateFormatHelper'];

	function MoveShiftCtrl($scope, $locale, $element, validator, personSelectionSvc, activitySvc, teamScheduleNotificationService, serviceDateFormatHelper) {
		var ctrl = this;
		ctrl.label = 'MoveShift';
		ctrl.processingCommand = false;
		ctrl.invalidAgents = [];
		ctrl.agentsInDifferentTimeZone = [];

		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
			ctrl.moveToTime = getDefaultMoveToTime();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			findAgentsInDifferentTimezone();
			addTabindexAndFocus();
			ctrl.updateInvalidAgents();
		};

		ctrl.updateInvalidAgents = function () {
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			validator.validateMoveToTimeForShift(ctrl.containerCtrl.scheduleManagementSvc, moment(getMoveToStartTimeStr()), currentTimezone);
			var invalidAgents = validator.getInvalidPeople();
			if (ctrl.agentsInDifferentTimeZone.length > 0) {
				invalidAgents = invalidAgents.concat(ctrl.agentsInDifferentTimeZone);
			}
			ctrl.invalidAgents = filterAgentArray(invalidAgents);
		}

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};
		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length ;
		};
		ctrl.anyAgentsInDifferentTimeZone = function() {
			return ctrl.agentsInDifferentTimeZone.length > 0;
		};

		ctrl.moveShift = function () {
			var invalidAgentIds = ctrl.invalidAgents.map(function(a) {
				return a.PersonId;
			});
			var agentsInDifferentTimeZone = ctrl.agentsInDifferentTimeZone.map(function(a) {
				return a.PersonId;
			});
			invalidAgentIds = invalidAgentIds.concat(agentsInDifferentTimeZone);

			var validAgents = ctrl.selectedAgents.filter(function(agent) {
				return invalidAgentIds.indexOf(agent.PersonId) < 0;
			});

			var validAgentIds = validAgents.map(function (agent) {
				return agent.PersonId;
			});

			if (validAgentIds.length > 0) {
				var requestData = {
					Date: ctrl.containerCtrl.getDate(),
					NewShiftStart: ctrl.containerCtrl.getServiceTimeInCurrentUserTimezone(serviceDateFormatHelper.getDateTime(ctrl.moveToTime)),
					PersonIds: validAgentIds,
					TrackedCommandInfo: { TrackId: ctrl.trackId }
				};

				ctrl.processingCommand = true;

				activitySvc.moveShift(requestData)
					.then(function(response) {
						if (ctrl.containerCtrl.getActionCb(ctrl.label)) {
							ctrl.containerCtrl.getActionCb(ctrl.label)(ctrl.trackId, validAgentIds);
						}
						teamScheduleNotificationService.reportActionResult({
								success: 'SuccessfulMessageForMovingShift',
								warning: 'PartialSuccessMessageForMovingShift'
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

		function getDefaultMoveToTime() {
			return serviceDateFormatHelper.getDateTime(moment(ctrl.containerCtrl.getDate() + " 08:00"));
		}

		function getMoveToStartTimeStr() {
			return serviceDateFormatHelper.getDateTime(moment(ctrl.containerCtrl.getDate() + " " + moment(ctrl.moveToTime).format('HH:mm')));
		}

		function filterAgentArray(arr) {
			var map = {};
			arr.forEach(function(agent) {
				map[agent.PersonId] = {
					PersonId:agent.PersonId,
					Name:agent.Name
				};
			});
			var personIds = Object.keys(map);
			return personIds.map(function(key) { return map[key]; });
		}

		function findAgentsInDifferentTimezone() {
			ctrl.agentsInDifferentTimeZone.splice(0);
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			ctrl.selectedAgents.forEach(function (agent) {
				if (currentTimezone != ctrl.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId).Timezone.IanaId) {
					ctrl.agentsInDifferentTimeZone.push(agent);
				}
			});
		}
		
	}
})();