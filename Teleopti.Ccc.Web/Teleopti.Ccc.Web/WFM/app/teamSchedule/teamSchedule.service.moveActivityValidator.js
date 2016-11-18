﻿(function() {
	'use strict';

	angular.module("wfm.teamSchedule").service('ActivityValidator', ['$filter', 'PersonSelection', 'ScheduleManagement', 'belongsToDateDecider', validator]);

	function validator($filter, PersonSelectionSvc, ScheduleMgmt, belongsToDateDecider) {
		var MAX_SCHEDULE_LENGTH_IN_MINUTES = 36 * 60; // 36 hours
		var invalidPeople = [];
		this.validateMoveToTime = validateMoveToTimeForScheduleInDifferentTimezone;
		this.validateMoveToTimeForShift = validateShiftsToMove;
		this.validateInputForOvertime = validateInputForOvertime;

		function validateInputForOvertime(timeRange, selectedDefinitionSetId, currentTimezone) {
			var invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();

			for (var i = 0; i < selectedPersonIds.length; i++) {
				var personId = selectedPersonIds[i];
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(personId);
				if (selectedDefinitionSetId && (!personSchedule.MultiplicatorDefinitionSetIds ||
					personSchedule.MultiplicatorDefinitionSetIds.indexOf(selectedDefinitionSetId) < 0)) {
					invalidPeople.push({ PersonId: personId, Name: personSchedule.Name });
					continue;
				}
				var normalizedScheduleVm = belongsToDateDecider.normalizePersonScheduleVm(personSchedule, currentTimezone);
				var belongsToDate = belongsToDateDecider.decideBelongsToDateForOvertimeActivity(timeRange, normalizedScheduleVm);
				var shiftsForBelongsToDate = personSchedule.Shifts.filter(function(shift) {
					return shift.Date === belongsToDate;
				});

				var start, end, duration;
				if (shiftsForBelongsToDate.length > 0 && shiftsForBelongsToDate[0].ProjectionTimeRange) {
					start = moment(shiftsForBelongsToDate[0].ProjectionTimeRange.Start);
					end = moment(shiftsForBelongsToDate[0].ProjectionTimeRange.End);
					if (timeRange.startTime.isBefore(start)) {
						start = timeRange.startTime;
					}
					if (timeRange.endTime.isAfter(end)) {
						end = timeRange.endTime;
					}
					duration = end.diff(start, 'minutes');
					if (duration > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
						invalidPeople.push({PersonId: personId, Name: personSchedule.Name});
					}
				}

			}

			return invalidPeople;
		}

		function getShiftDate(personSchedule) {
			var selectedShift = personSchedule.Shifts.filter(function(shift) {
				var selectedProjections = shift.Projections.filter(function(layer) {
					return layer.Selected;
				});
				return selectedProjections.length > 0;
			})[0];
			return selectedShift.Date;
		}

		function validateShiftsToMove(newStartMoment, currentTimezone) {
			invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();
			for (var i = 0; i < selectedPersonIds.length; i++) {
				var personId = selectedPersonIds[i];
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(personId);
				var currentDate = personSchedule.Date;
				var currentDateMoment = moment(currentDate);

				if (personSchedule.IsFullDayAbsence) {
					invalidPeople.push(personSchedule);
					continue;
				}

				var newStartInAgentTimezone = $filter('timezone')(newStartMoment.format('YYYY-MM-DD HH:mm'),
					personSchedule.Timezone.IanaId,
					currentTimezone);

				if (!moment(newStartInAgentTimezone).isSame(currentDate, 'day')) {
					invalidPeople.push(personSchedule);
					continue;
				}

				var shifts = personSchedule.Shifts.filter(function(shift) {
					return shift.Date === currentDate;
				});

				if (shifts.length === 0) {
					invalidPeople.push(personSchedule);
					continue;
				};

				var shiftForCurrentDay = shifts[0];
				if (!shiftForCurrentDay.ProjectionTimeRange) {
					invalidPeople.push(personSchedule);
					continue;
				}

				var shiftLength = moment(shiftForCurrentDay.ProjectionTimeRange.End)
					.diff(moment(shiftForCurrentDay.ProjectionTimeRange.Start), 'minute');
				var newEndMoment = newStartMoment.clone().add(shiftLength, 'minute');

				var hasConflict = personSchedule.Shifts.concat(personSchedule.ExtraShifts).some(function(shift) {
					if (currentDateMoment.isSame(shift.Date, 'day') || !shift.ProjectionTimeRange) return false;

					return (currentDateMoment.isAfter(shift.Date, 'day') &&  newStartMoment.isSameOrBefore(moment(shift.ProjectionTimeRange.End), 'minute')) ||
						(currentDateMoment.isBefore(shift.Date, 'day') && newEndMoment.isSameOrAfter(moment(shift.ProjectionTimeRange.Start), 'minute'));
				});

				if (hasConflict) {
					invalidPeople.push(personSchedule);
				}
			}

			return invalidPeople.length === 0;
		}

		function validateMoveToTimeForScheduleInDifferentTimezone(newStartMoment, currentTimezone) {
			invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();
			for (var i = 0; i < selectedPersonIds.length; i++) {
				var personId = selectedPersonIds[i];
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(personId);

				var shiftDate = getShiftDate(personSchedule);

				var newStartInAgentTimezone = $filter('timezone')(newStartMoment.format('YYYY-MM-DD HH:mm'),
					personSchedule.Timezone.IanaId,
					currentTimezone);
				if (moment(newStartInAgentTimezone).isBefore(shiftDate, 'day')) {
					invalidPeople.push(personSchedule);
					continue;
				}
				var newShiftStart = getNewScheduleStartMoment(shiftDate, personSchedule, newStartMoment);
				var newShiftStartInAgentTimezone = $filter('timezone')(newShiftStart.format('YYYY-MM-DD HH:mm'),
					personSchedule.Timezone.IanaId,
					currentTimezone);
				var newShiftEnd = getLatestScheduleEndMoment(shiftDate, personSchedule, newStartMoment);
				var scheduleLength = newShiftEnd ? newShiftEnd.diff(newShiftStart, 'minutes') : 0;
				if (moment(newShiftStartInAgentTimezone).format('YYYY-MM-DD') != shiftDate || scheduleLength > MAX_SCHEDULE_LENGTH_IN_MINUTES)
					invalidPeople.push(personSchedule);
			}

			return invalidPeople.length === 0;
		}

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
				if (earliestM === null || m.isBefore(earliestM)) earliestM = m;
			});
			return earliestM;
		}

		function findLatestMoment(moments) {
			if (!moments || moments.length === 0) return null;
			var latestM = null;
			moments.forEach(function(m) {
				if (latestM === null || m.isAfter(latestM)) latestM = m;
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

				return moment(p.Start).clone().add(p.Minutes, 'minutes');
			});
			ends.push(newEndMoment);
			return findLatestMoment(ends);
		}

		this.getInvalidPeople = function () {
			return invalidPeople;
		};

		this.getInvalidPeopleNameList = function() {
			return invalidPeople.map(function(p) { return p.Name });
		}

	}


})()