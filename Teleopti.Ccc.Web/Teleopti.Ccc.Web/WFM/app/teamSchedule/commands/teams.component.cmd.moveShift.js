(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('moveShift',
	{
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		templateUrl: 'app/teamSchedule/commands/teams.component.cmd.moveShift.html',
		controller: MoveShiftCtrl
	});

	MoveShiftCtrl.$inject = ['$scope', '$locale', '$element', 'ActivityValidator', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];

	function MoveShiftCtrl($scope, $locale, $element, validator, personSelectionSvc, activitySvc, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.label = 'MoveShift';
		ctrl.processingCommand = false;
		ctrl.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
		ctrl.meridians = ctrl.showMeridian ? $locale.DATETIME_FORMATS.AMPMS : [];
		ctrl.invalidAgents = [];
		ctrl.agentsInDifferentTimeZone = [];

		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			ctrl.moveToTime = getDefaultMoveToTime();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			findAgentsInDifferentTimezone();

			$scope.$watch(function () {
				return getMoveToStartTimeStr();
			}, function (n, o) {
				if (ctrl.agentsInDifferentTimeZone.length === 0) {
					updateInvalidAgents();
				}
				
			});
			addTabindexAndFocus();
		};

		function addTabindexAndFocus() {
			var tabindex = angular.isDefined($element.attr.tabindex) ? $element.attr.tabindex : '0';
			var addTabIndexTo = function() {
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

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};

		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};

		ctrl.moveShift = function () {
			var invalidAgentIds = ctrl.invalidAgents.map(function(a) {
				return a.PersonId;
			});

			var validAgents = ctrl.selectedAgents.filter(function(agent) {
				return invalidAgentIds.indexOf(agent.PersonId) < 0;
			});

			var validAgentIds = validAgents.map(function (agent) {
				return agent.PersonId;
			});

			if (validAgentIds.length > 0) {
				var requestData = {
					Date: ctrl.containerCtrl.getDate(),
					NewShiftStart:ctrl.containerCtrl.convertTimeToCurrentUserTimezone(moment(ctrl.moveToTime).format('YYYY-MM-DD HH:mm')),
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

		function getDefaultMoveToTime() {
			return moment(ctrl.containerCtrl.getDate() + " 08:00").toDate();
		}

		function getMoveToStartTimeStr() {
			var dateStr = ctrl.containerCtrl.getDate();
			var timeStr = moment(ctrl.moveToTime).format('HH:mm');
			return dateStr + ' ' + timeStr;
		}

		function updateInvalidAgents() {
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			validator.validateMoveToTimeForShift(ctrl.containerCtrl.scheduleManagementSvc, moment(getMoveToStartTimeStr()), currentTimezone);
			ctrl.invalidAgents = validator.getInvalidPeople();
		}

		function findAgentsInDifferentTimezone() {
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			ctrl.selectedAgents.forEach(function (agent) {
				if (currentTimezone != ctrl.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId).Timezone.IanaId) {
					ctrl.agentsInDifferentTimeZone.push(agent.Name);
				}

			});
		}
		
	}
})();