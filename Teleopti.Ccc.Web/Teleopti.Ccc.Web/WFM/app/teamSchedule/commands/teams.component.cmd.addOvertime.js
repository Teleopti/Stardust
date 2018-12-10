(function () {
	'use strict';
	angular.module('wfm.teamSchedule').component('addOvertime',
		{
			templateUrl: 'app/teamSchedule/commands/teams.component.cmd.addOvertime.html',
			require: {
				containerCtrl: '^teamscheduleCommandContainer'
			},
			controller: AddOvertimeCtrl
		});

	AddOvertimeCtrl.$inject = ['$translate', 'PersonSelection', 'ActivityService', 'ActivityValidator', 'belongsToDateDecider', 'teamScheduleNotificationService', 'serviceDateFormatHelper'];

	function AddOvertimeCtrl($translate, personSelectionSvc, activityService, activityValidator, belongsToDateDecider, teamScheduleNotificationService, serviceDateFormatHelper) {
		var ctrl = this;
		var defaultWorkStartInHour = 8,
			defaultWorkEndInHour = 17;

		ctrl.label = 'AddOvertimeActivity';
		ctrl.processingCommand = false;
		ctrl.invalidAgents = [];

		ctrl.validateTimeRangeAndInput = function (isFormValid) {
			if (!isFormValid) return;
			ctrl.timeRangeIsValid = validateTimeRange(ctrl.fromTime, ctrl.toTime);
			ctrl.validateInput();
		}

		ctrl.$onInit = function () {
			ctrl.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			ctrl.fromTime = serviceDateFormatHelper.getDateTime(getDefaultStartTimeMoment());
			ctrl.toTime = serviceDateFormatHelper.getDateTime(getEndTimeMoment());
			ctrl.timeRangeIsValid = true;

			activityService.fetchAvailableActivities().then(function (activities) {
				ctrl.availableActivities = activities;
			});

			activityService.fetchAvailableDefinitionSets()
				.then(function (data) {
					ctrl.definitionSets = data;
				});
		};

		ctrl.anyValidAgent = function () {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};

		ctrl.anyInvalidAgent = function () {
			return ctrl.invalidAgents.length > 0;
		};

		ctrl.validateInput = function () {
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			var timeRange = {
				startTime: moment.tz(ctrl.fromTime, timezone),
				endTime: moment.tz(ctrl.toTime, timezone)
			};

			
			ctrl.invalidAgents = activityValidator.validateInputForOvertime(ctrl.containerCtrl.scheduleManagementSvc, timeRange, ctrl.selectedDefinitionSetId, timezone);
		};

		ctrl.addOvertime = function () {
			var invalidAgentIds = ctrl.invalidAgents.map(function (a) {
				return a.PersonId;
			});

			var validAgents = ctrl.selectedAgents.filter(function (agent) {
				return invalidAgentIds.indexOf(agent.PersonId) < 0;
			});

			if (validAgents.length > 0) {
				var requestData = prepareRequestData(validAgents);

				ctrl.processingCommand = true;

				activityService.addOvertimeActivity(requestData)
					.then(function (response) {
						if (ctrl.containerCtrl.getActionCb(ctrl.label)) {
							ctrl.containerCtrl.getActionCb(ctrl.label)(ctrl.trackId, validAgents.map(function (a) { return a.PersonId; }));
						}
						teamScheduleNotificationService.reportActionResult({
							success: $translate.instant('SuccessfulMessageForAddingOvertime'),
							warning: $translate.instant('PartialSuccessMessageForAddingOvertime')
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

		function getDefaultStartTimeMoment() {
			var basedOnAgentInfo = getAgentInfoOnTheTop();
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			if (basedOnAgentInfo.IsDayOff || basedOnAgentInfo.IsEmptyDay || basedOnAgentInfo.IsFullDayAbsence) {
				return moment.tz(basedOnAgentInfo.ScheduleDate, currentTimezone).hour(defaultWorkStartInHour);
			}
			return basedOnAgentInfo.ScheduleEndTimeMoment.clone();
		}

		function getEndTimeMoment() {
			var basedOnAgentInfo = getAgentInfoOnTheTop();
			var currentTimezone = ctrl.containerCtrl.getCurrentTimezone();
			if (basedOnAgentInfo.IsDayOff || basedOnAgentInfo.IsEmptyDay || basedOnAgentInfo.IsFullDayAbsence) {
				return moment.tz(basedOnAgentInfo.ScheduleDate, currentTimezone).hour(defaultWorkEndInHour);
			}
			return basedOnAgentInfo.ScheduleEndTimeMoment.clone().add(1, 'hours');
		}

		function getAgentInfoOnTheTop() {
			return angular.copy(ctrl.selectedAgents).sort(function (a1, a2) {
				return a1.OrderIndex - a2.OrderIndex;
			})[0];
		}

		function prepareRequestData(validAgents) {
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			var timeRange = {
				startTime: moment.tz(ctrl.fromTime, timezone),
				endTime: moment.tz(ctrl.toTime, timezone)
			};

			var personDates = validAgents.map(function (agent) {
				var personSchedule = ctrl.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId);
				var normalizedScheduleVm = belongsToDateDecider.normalizePersonScheduleVm(personSchedule, timezone);
				var belongsToDate = belongsToDateDecider.decideBelongsToDateForOvertimeActivity(timeRange, normalizedScheduleVm);
				return {
					PersonId: agent.PersonId,
					Date: belongsToDate
				}
			});

			var requestData = {
				PersonDates: personDates,
				ActivityId: ctrl.selectedActivityId,
				MultiplicatorDefinitionSetId: ctrl.selectedDefinitionSetId,
				StartDateTime: getServiceTimeInCurrentUserTimezone(timeRange.startTime),
				EndDateTime: getServiceTimeInCurrentUserTimezone(timeRange.endTime),
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			};

			return requestData;
		}

		function getServiceTimeInCurrentUserTimezone(dateTime) {
			return ctrl.containerCtrl.getServiceTimeInCurrentUserTimezone(dateTime);
		}

		function validateTimeRange(fromTime, toTime) {
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			var from = moment.tz(fromTime, timezone);
			var to = moment.tz(toTime, timezone);
			return to.isAfter(from);
		}
	}
})();