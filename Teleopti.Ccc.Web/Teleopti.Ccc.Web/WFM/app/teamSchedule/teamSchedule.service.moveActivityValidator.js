﻿(function() {
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
						if (newScheduleEnd === null) continue;

						var scheduleLength = newScheduleEnd.diff(newScheduleStart, 'minutes');

						if (newScheduleStart.format('YYYY-MM-DD') !== schedules[i].Date || scheduleLength > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
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

			return moment(projections[projections.length - 1].Start).diff(moment(projections[0].Start), 'minutes') + projections[projections.length - 1].Minutes;
		}

		function getUnselectedProjections(scheduleDate, personSchedule) {
			var shiftsOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date === scheduleDate;
			});
			if (shiftsOnCurrentDate.length === 0) return null;
			if (!shiftsOnCurrentDate[0].Projections || shiftsOnCurrentDate[0].Projections.length === 0) return null;
			return shiftsOnCurrentDate[0].Projections.filter(function(p) { return !p.Selected; });
		}

		function getSelectedProjections(scheduleDate, personSchedule) {
			var shiftsOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date === scheduleDate;
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
		
		function getNewScheduleStartMoment(scheduleDate, personSchedule, newStartMoment) {
			var unselected = getUnselectedProjections(scheduleDate, personSchedule);
			if (!unselected || unselected.length === 0) return newStartMoment;

			var starts = unselected.map(function(p) { return moment(p.Start); });
			starts.push(newStartMoment);
			return findEarliestMoment(starts);
		}

		function getLatestScheduleEndMoment(scheduleDate, personSchedule, newStartMoment) {
			var selected = getSelectedProjections(scheduleDate, personSchedule);

			var pl = calculateMaximumProjectionLengthInMinute(selected);
			if (pl === null) return null;

			var newEndMoment = newStartMoment.clone().add(pl, 'minutes');

			var unselected = getUnselectedProjections(scheduleDate, personSchedule);
			if (!unselected || unselected.length === 0) return newEndMoment;
			
			var ends =  unselected.map(function(p) {

				return moment(p.Start).add(p.Minutes, 'minutes');
			});
			ends.push(newEndMoment);
			return findLatestMoment(ends);
		}

		this.getInvalidPeople = function() {
			return invalidPeople;
		}

	}


})()