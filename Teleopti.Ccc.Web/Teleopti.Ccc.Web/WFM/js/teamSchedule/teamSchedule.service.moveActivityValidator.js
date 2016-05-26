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
					if (schedules[i].PersonId === agentId) {
						
						var newScheduleStart = getNewScheduleStartMoment(schedules[i].Date, schedules[i], newStartMoment);
						var newScheduleEnd = getLatestScheduleEndMoment(schedules[i].Date, schedules[i], newStartMoment);
						var scheduleLength = newScheduleEnd.diff(newScheduleStart, 'minutes');

						if (newScheduleStart.format('YYYY-MM-DD') !== schedules[i].Date.format('YYYY-MM-DD') || scheduleLength > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
							invalidPeople.push(schedules[i].Name);
						}

						break;
					}

				}
			});
			return invalidPeople.length === 0;
		};

		function calculateMaximumProjectionLengthInMinute(projections) {
			if (!projections || projections.length === 0) return null;

			var projectionLengths =	projections.map(function(p) {
				return moment(p.End).diff(moment(p.End), 'minutes');
			});
		
			return Math.max.apply(null, projectionLengths);
		}

		function getUnselectedProjections(scheduleDateMoment, personSchedule) {
			var shiftsOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date.format('YYYY-MM-DD') === scheduleDateMoment.format('YYYY-MM-DD');
			});
			if (shiftsOnCurrentDate.length === 0) return null;
			if (!shiftsOnCurrentDate[0].Projections || shiftsOnCurrentDate[0].Projections.length === 0) return null;
			return shiftsOnCurrentDate[0].Projections.filter(function(p) { return !p.Selected; });
		}

		function getSelectedProjections(scheduleDateMoment, personSchedule) {
			var shiftsOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date.format('YYYY-MM-DD') === scheduleDateMoment.format('YYYY-MM-DD');
			});
			if (shiftsOnCurrentDate.length === 0) return null;
			if (!shiftsOnCurrentDate[0].Projections || shiftsOnCurrentDate[0].Projections.length === 0) return null;
			return shiftsOnCurrentDate[0].Projections.filter(function(p) { return p.Selected; });
		}

		function findEarliestMoment(moments) {
			if (!moments || moments.length === 0) return null;
			var earliestM = null;
			moments.forEach(function(m) {
				if (earliestM === null || m < earliestM) earliestM = m;
			});
			return earliestM;
		}

		function findLatestMoment(moments) {
			if (!moments || moments.length === 0) return null;
			var latestM = null;
			moments.forEach(function(m) {
				if (latestM === null || m > latestM) latestM = m;
			});
			return latestM;
		}
		
		function getNewScheduleStartMoment(scheduleDateMoment, personSchedule, newStartMoment) {
			var unselected = getUnselectedProjections(scheduleDateMoment, personSchedule);
			if (!unselected || unselected.length === 0) return newStartMoment;

			var starts = unselected.map(function(p) { return moment(p.Start); });
			starts.push(newStartMoment);
			return findEarliestMoment(starts);
		}

		function getLatestScheduleEndMoment(scheduleDateMoment, personSchedule, newStartMoment) {
			var selected = getSelectedProjections(scheduleDateMoment, personSchedule);
			var newEndMoment = newStartMoment.clone().add(calculateMaximumProjectionLengthInMinute(selected), 'minutes');

			var unselected = getUnselectedProjections(scheduleDateMoment, personSchedule);
			if (!unselected || unselected.length === 0) return newEndMoment;
			
			var ends =  unselected.map(function(p) { return moment(p.End); });
			ends.push(newEndMoment);
			return findLatestMoment(ends);
		}

		this.getInvalidPeople = function() {
			return invalidPeople;
		}

	}


})()