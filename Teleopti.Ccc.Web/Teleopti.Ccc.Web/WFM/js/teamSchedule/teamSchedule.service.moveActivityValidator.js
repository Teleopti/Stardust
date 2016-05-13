(function() {
	'use strict';

	angular.module("wfm.teamSchedule").service('MoveActivityValidator', ['PersonSelection', 'ScheduleManagement', validator]);

	function validator(PersonSelectionSvc, ScheduleMgmt) {
		var MAX_SCHEDULE_LENGTH_IN_MINUTES = 36 * 60; // 36 hours
		var invalidPeople = [];
		this.validateMoveToTime = function (newStartMoment) {
			invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();
			var schedules = ScheduleMgmt.groupScheduleVm.Schedules;
			selectedPersonIds.forEach(function(agentId) {
				for (var i = 0; i < schedules.length; i++) {
					if (schedules[i].PersonId == agentId) {

						var newScheduleStart = calculateNewScheduleStartMoment(schedules[i], newStartMoment);
						var newScheduleEnd = calculateNewScheduleEndMoment(schedules[i], newStartMoment);
						var scheduleLength = newScheduleEnd.diff(newScheduleStart, 'minutes');

						if (!newScheduleStart.isSame(schedules[i].ScheduleStartTime(), 'day') || scheduleLength > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
							invalidPeople.push(schedules[i].Name);
						}
						break;
					}

				}
			});
			return invalidPeople.length == 0;
		};

		function calculateNewScheduleStartMoment(personSchedule, newStartMoment) {
			var shiftOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date.isSame(personSchedule.Date, 'day');
			});
			var unselectedLayers = shiftOnCurrentDate.length > 0 ? shiftOnCurrentDate[0].Projections.filter(function (projection) {
				return !projection.Selected;
			}) : [];
			return newStartMoment.isBefore(personSchedule.ScheduleStartTime()) || unselectedLayers.length == 0 ? newStartMoment :
						unselectedLayers.length > 0 && newStartMoment.isAfter(unselectedLayers[0].Start) ? moment(unselectedLayers[0].Start) : moment(personSchedule.ScheduleStartTime());
		};
		function calculateNewScheduleEndMoment(personSchedule, newStartMoment) {
			var shiftOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date.isSame(personSchedule.Date, 'day');
			});
		    

			var selectedLayers = shiftOnCurrentDate.length > 0 ? shiftOnCurrentDate[0].Projections.filter(function (projection) {
				return projection.Selected;
			}):[];
			var unselectedLayers = shiftOnCurrentDate.length > 0 ? shiftOnCurrentDate[0].Projections.filter(function (projection) {
				return !projection.Selected;
			}) : [];
			if (shiftOnCurrentDate.length == 0 || selectedLayers.length == 0)
			    return moment(personSchedule.ScheduleEndTime());
			var newLayerEnd = moment(selectedLayers[selectedLayers.length - 1].Start).add(selectedLayers[selectedLayers.length - 1].Minutes, 'minutes').add(newStartMoment.diff(selectedLayers[0].Start, 'minutes'), 'minutes');
			return newLayerEnd.isAfter(personSchedule.ScheduleEndTime()) || unselectedLayers.length == 0 ? newLayerEnd : unselectedLayers.length > 0 && newLayerEnd.isBefore(moment(unselectedLayers[unselectedLayers.length -1].Start).add(unselectedLayers[unselectedLayers.length -1].Minutes,'minutes')) ? 
				moment(unselectedLayers[unselectedLayers.length -1].Start).add(unselectedLayers[unselectedLayers.length -1].Minutes,'minutes') : moment(personSchedule.ScheduleEndTime());
		};
		this.getInvalidPeople = function() {
			return invalidPeople;
		}

	}


})()