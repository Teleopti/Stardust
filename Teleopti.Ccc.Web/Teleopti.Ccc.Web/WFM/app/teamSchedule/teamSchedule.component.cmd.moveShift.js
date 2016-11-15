(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('moveShift',
	{
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		templateUrl: 'app/teamSchedule/html/moveShift.tpl.html',
		controller: MoveShiftCtrl
	});

	MoveShiftCtrl.$inject = ['$scope', '$locale', 'MoveActivityValidator', 'PersonSelection', 'ActivityService', 'teamScheduleNotificationService'];

	function MoveShiftCtrl($scope, $locale, validator, personSelectionSvc, activitySvc, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.label = 'MoveShift';
		ctrl.processingCommand = false;
		ctrl.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
		ctrl.meridians = ctrl.showMeridian ? $locale.DATETIME_FORMATS.AMPMS : [];
		ctrl.invalidAgents = [];

		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			ctrl.moveToTime = getDefaultMoveToTime();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			$scope.$watch(function () {
				return getMoveToStartTimeStr();
			}, function(n, o) {
				updateInvalidAgents();
			});
		};

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};

		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};

		ctrl.moveShift = function () {
			var validAgents = ctrl.selectedAgents.filter(function(agent) {
				return ctrl.invalidAgents.indexOf(agent) < 0;
			});

			var validAgentIds = validAgents.map(function (agent) {
				return agent.PersonId;
			});

			if (validAgentIds.length > 0) {
				var requestData = {
					Date: ctrl.containerCtrl.getDate(),
					NewShiftStart:ctrl.containerCtrl.convertTimeToCurrentUserTimezone(moment(ctrl.moveToTime).format('YYYY-MM-DDTHH:mm')),
					PersonIds: validAgentIds
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
			validator.validateMoveToTimeForShift(moment(getMoveToStartTimeStr()), currentTimezone);
			ctrl.invalidAgents = validator.getInvalidPeople();
		}
	}
})();