(function() {
	'use strict';
	angular.module('wfm.teamSchedule').component('addOvertime',
	{
		templateUrl: 'app/teamSchedule/html/addOvertime.tpl.html',
		require: {
			containerCtrl: '^teamscheduleCommandContainer'
		},
		controller: AddOvertimeCtrl
	});

	AddOvertimeCtrl.$inject = ['PersonSelection', 'ActivityService', 'ActivityValidator'];

	function AddOvertimeCtrl(personSelectionSvc, activityService, activityValidator) {
		var ctrl = this;
		ctrl.label = 'AddOvertimeActivity';
		ctrl.processingCommand = false;
		ctrl.invalidAgents = [];
		ctrl.updateFromTime = function(newTime) {
			ctrl.fromTime = newTime;
			ctrl.timeRangeIsValid = validateTimeRange(ctrl.fromTime, ctrl.toTime);
			validateInput();
		};
		ctrl.updateToTime = function(newTime) {
			ctrl.toTime = newTime;
			ctrl.timeRangeIsValid = validateTimeRange(ctrl.fromTime, ctrl.toTime);
			validateInput();
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
			return true;
		};

		function validateInput() {
			//ctrl.selectedDefinitionSetId  allowed
			var timeRange = {
				startTime: moment(ctrl.fromTime),
				endTime: moment(ctrl.toTime)
			};
			var timezone = ctrl.containerCtrl.getCurrentTimezone();
			ctrl.invalidAgents = activityValidator.validateInputForOvertime(timeRange, ctrl.selectedDefinitionSetId, timezone);
			console.log(ctrl.invalidAgents);
		}

		function validateTimeRange(fromTime, toTime) {
			var from = moment(fromTime);
			var to = moment(toTime);
			return to.isAfter(from);
		}
	}
})();