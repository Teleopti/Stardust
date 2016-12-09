﻿(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('addOvertime',
	{
		templateUrl: 'app/teamSchedule/commands/teams.component.cmd.addOvertime.html',
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		controller: AddOvertimeCtrl
	});

	AddOvertimeCtrl.$inject = ['PersonSelection', 'ScheduleManagement', 'ActivityService', 'ActivityValidator', 'belongsToDateDecider', 'teamScheduleNotificationService'];

	function AddOvertimeCtrl(personSelectionSvc, ScheduleMgmt, activityService, activityValidator, belongsToDateDecider, teamScheduleNotificationService) {
		var ctrl = this;
		ctrl.label = 'AddOvertimeActivity';
		ctrl.processingCommand = false;
		ctrl.invalidAgents = [];
		ctrl.updateFromTime = function(newTime) {
			ctrl.fromTime = newTime;
			ctrl.timeRangeIsValid = validateTimeRange(ctrl.fromTime, ctrl.toTime);
			ctrl.validateInput();
		};
		ctrl.updateToTime = function(newTime) {
			ctrl.toTime = newTime;
			ctrl.timeRangeIsValid = validateTimeRange(ctrl.fromTime, ctrl.toTime);
			ctrl.validateInput();
		}

		ctrl.$onInit = function() {
			ctrl.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			ctrl.trackId = ctrl.containerCtrl.getTrackId();
			ctrl.fromTime = moment(ctrl.containerCtrl.getDate()).toDate();
			ctrl.toTime = moment(ctrl.containerCtrl.getDate()).add(1, 'hour').toDate();
			ctrl.timeRangeIsValid = true;
		};

		ctrl.loadActivities = function () {
			return activityService.fetchAvailableActivities().then(function (activities) {
				ctrl.availableActivities = activities;
			});
		};

		ctrl.loadDefinitionSets = function() {
			return activityService.fetchAvailableDefinitionSets()
				.then(function(data) {
					ctrl.definitionSets = data;
				});
		};

		ctrl.anyValidAgent = function() {
			return ctrl.invalidAgents.length !== ctrl.selectedAgents.length;
		};

		ctrl.anyInvalidAgent = function() {
			return ctrl.invalidAgents.length > 0;
		};

		ctrl.validateInput = function() {
			var timeRange = {
				startTime: moment(ctrl.fromTime),
				endTime: moment(ctrl.toTime)
			};
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			ctrl.invalidAgents = activityValidator.validateInputForOvertime(timeRange, ctrl.selectedDefinitionSetId, timezone);
		};

		ctrl.addOvertime = function() {
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
							ctrl.containerCtrl.getActionCb(ctrl.label)(ctrl.trackId, validAgents.map(function(a) { return a.PersonId; }));
						}
						teamScheduleNotificationService.reportActionResult({
							success: 'SuccessfulMessageForAddingOvertime',
							warning: 'PartialSuccessMessageForAddingOvertime'
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

		function prepareRequestData(validAgents) {
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			var personDates = validAgents.map(function (agent) {
				var timeRange = {
					startTime: moment(ctrl.fromTime),
					endTime: moment(ctrl.toTime)
				};
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(agent.PersonId);
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
				StartDateTime: ctrl.containerCtrl.convertTimeToCurrentUserTimezone(moment(ctrl.fromTime).format('YYYY-MM-DD HH:mm')),
				EndDateTime: ctrl.containerCtrl.convertTimeToCurrentUserTimezone(moment(ctrl.toTime).format('YYYY-MM-DD HH:mm')),
				TrackedCommandInfo: { TrackId: ctrl.trackId }
			};

			return requestData;
		}

		function validateTimeRange(fromTime, toTime) {
			var from = moment(fromTime);
			var to = moment(toTime);
			return to.isAfter(from);
		}
	}
})();