(function () {
	'use strict';

	angular.module("wfm.teamSchedule").service('ActivityValidator', ['$filter', 'PersonSelection', 'belongsToDateDecider', 'serviceDateFormatHelper', validator]);

	function validator($filter, PersonSelectionSvc, belongsToDateDecider, serviceDateFormatHelper) {
		var MAX_SCHEDULE_LENGTH_IN_MINUTES = 36 * 60; // 36 hours
		var invalidPeople = [];
		this.validateMoveToTime = validateMoveToTimeForScheduleInDifferentTimezone;
		this.validateMoveToTimeForShift = validateShiftsToMove;
		this.validateInputForOvertime = validateInputForOvertime;

		function validateInputForOvertime(ScheduleMgmt, timeRange, selectedDefinitionSetId, currentTimezone) {
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
				if (!belongsToDate) {
					invalidPeople.push({ PersonId: personId, Name: personSchedule.Name });
					continue;
				}
				var shiftsForBelongsToDate = personSchedule.Shifts.filter(function (shift) {
					return shift.Date === belongsToDate;
				});

				var start, end, durationInMinutes;
				if (shiftsForBelongsToDate.length > 0 && shiftsForBelongsToDate[0].ProjectionTimeRange) {
					start = moment(shiftsForBelongsToDate[0].ProjectionTimeRange.Start);
					end = moment(shiftsForBelongsToDate[0].ProjectionTimeRange.End);
					if (timeRange.startTime.isBefore(start)) {
						start = timeRange.startTime;
					}
					if (timeRange.endTime.isAfter(end)) {
						end = timeRange.endTime;
					}
					durationInMinutes = end.diff(start, 'minutes');

					if (moment($filter('timezone')(serviceDateFormatHelper.getDateTime(timeRange.startTime), personSchedule.Timezone.IanaId, currentTimezone))
						.isBefore(belongsToDate, 'day')) {
						invalidPeople.push({ PersonId: personId, Name: personSchedule.Name });
						continue;
					}
				} else {
					durationInMinutes = timeRange.endTime.diff(timeRange.startTime, 'minute');
				}

				if (durationInMinutes > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
					invalidPeople.push({ PersonId: personId, Name: personSchedule.Name });
				}
			}

			return invalidPeople;
		}

		function getShiftDate(personSchedule) {
			var selectedShift = personSchedule.Shifts.filter(function (shift) {
				var selectedProjections = shift.Projections.filter(function (layer) {
					return layer.Selected;
				});
				return selectedProjections.length > 0;
			})[0];
			if (selectedShift) return selectedShift.Date;
			else return personSchedule.Date;
		}

		function validateShiftsToMove(ScheduleMgmt, newStartMoment) {
			invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();
			for (var i = 0; i < selectedPersonIds.length; i++) {
				var personId = selectedPersonIds[i];
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(personId);
				var currentDate = personSchedule.Date;

				if (personSchedule.IsFullDayAbsence) {
					invalidPeople.push(personSchedule);
					continue;
				}

				var newStartInAgentTimezone = newStartMoment.tz(personSchedule.Timezone.IanaId);

				if (serviceDateFormatHelper.getDateOnly(newStartInAgentTimezone) !== currentDate) {
					invalidPeople.push(personSchedule);
					continue;
				}

				var shifts = personSchedule.Shifts.filter(function (shift) {
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

				var shiftLength = shiftForCurrentDay.ProjectionTimeRange.EndMoment
					.diff(shiftForCurrentDay.ProjectionTimeRange.StartMoment, 'minute');
				var newEndMoment = newStartMoment.clone().add(shiftLength, 'minute');

				var hasConflict = personSchedule.Shifts.concat(personSchedule.ExtraShifts)
					.some(function (shift) {
						if (shift.Date === currentDate || !shift.ProjectionTimeRange) return false;
						return (currentDate > shift.Date && newStartMoment.isSameOrBefore(shift.ProjectionTimeRange.EndMoment, 'minute')) ||
							(currentDate < shift.Date && newEndMoment.isSameOrAfter(shift.ProjectionTimeRange.StartMoment, 'minute'));
					});

				if (hasConflict) {
					invalidPeople.push(personSchedule);
				}
			}

			return invalidPeople.length === 0;
		}

		function validateMoveToTimeForScheduleInDifferentTimezone(ScheduleMgmt, newStartMoment) {
			invalidPeople = [];
			var selectedPersonIds = PersonSelectionSvc.getSelectedPersonIdList();
			for (var i = 0; i < selectedPersonIds.length; i++) {
				var personId = selectedPersonIds[i];
				var personSchedule = ScheduleMgmt.findPersonScheduleVmForPersonId(personId);
				var shiftDate = getShiftDate(personSchedule);

				var newStartInAgentTimezone = newStartMoment.tz(personSchedule.Timezone.IanaId);
				if (newStartInAgentTimezone.isBefore(shiftDate, 'day')) {
					invalidPeople.push(personSchedule);
					continue;
				}
				var newShiftStartMoment = getNewScheduleStartMoment(shiftDate, personSchedule, newStartMoment);
				var newShiftStartInAgentTimezone = newShiftStartMoment.tz(personSchedule.Timezone.IanaId);

				var newShiftEndMoment = getLatestScheduleEndMoment(shiftDate, personSchedule, newStartMoment);
				var scheduleLength = newShiftEndMoment ? newShiftEndMoment.diff(newShiftStartMoment, 'minutes') : 0;
				if (serviceDateFormatHelper.getDateOnly(newShiftStartInAgentTimezone) != shiftDate || scheduleLength > MAX_SCHEDULE_LENGTH_IN_MINUTES) {
					invalidPeople.push(personSchedule);
				}
			}

			return invalidPeople.length === 0;
		}

		function calculateMaximumProjectionLengthInMinute(projections) {
			if (!projections || projections.length === 0) return null;

			return projections[projections.length - 1].EndMoment.diff(projections[0].StartMoment, 'minutes');
		}

		function getProjectionsBySelectStatus(scheduleDate, personSchedule, isSelected) {
			var shiftsOnCurrentDate = personSchedule.Shifts.filter(function (shift) {
				return shift.Date === scheduleDate;
			});

			if (shiftsOnCurrentDate[0] && shiftsOnCurrentDate[0].Projections && !!shiftsOnCurrentDate[0].Projections.length)
				return shiftsOnCurrentDate[0].Projections.filter(function (p) { return p.Selected == !!isSelected; });
		}

		function sortMoments(moments, sortMethod) {
			if (!moments || moments.length === 0) return null;
			var firstM = null;
			moments.forEach(function (m) {
				if (firstM === null || m[sortMethod](firstM)) firstM = m;
			});
			return firstM;
		}


		function getNewScheduleStartMoment(scheduleDate, personSchedule, newStartMoment) {
			var unselected = getProjectionsBySelectStatus(scheduleDate, personSchedule, false);
			if (!unselected || unselected.length === 0) return newStartMoment;

			var starts = unselected.map(function (p) { return p.StartMoment.clone(); });
			starts.push(newStartMoment);
			return sortMoments(starts, 'isBefore');
		}

		function getLatestScheduleEndMoment(scheduleDate, personSchedule, newStartMoment) {
			var selected = getProjectionsBySelectStatus(scheduleDate, personSchedule, true);

			var pl = calculateMaximumProjectionLengthInMinute(selected);
			if (pl === null) return null;

			var newEndMoment = newStartMoment.clone().add(pl, 'minutes');

			var unselected = getProjectionsBySelectStatus(scheduleDate, personSchedule, false);
			if (!unselected || unselected.length === 0) return newEndMoment;

			var ends = unselected.map(function (p) {
				return p.EndMoment.clone();
			});
			ends.push(newEndMoment);
			return sortMoments(ends, 'isAfter');
		}

		this.getInvalidPeople = function () {
			return invalidPeople;
		};

		this.getInvalidPeopleNameList = function () {
			return invalidPeople.map(function (p) { return p.Name });
		}

	}


})();