(function (angular) {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('ScheduleHelper', ScheduleHelperService);

	function ScheduleHelperService() { }

	ScheduleHelperService.prototype.getEarliestStartMomentOfSelectedSchedules = function (schedules, dateMoment, personIds) {
		checkPersonIds(personIds);

		var earlistStart = null;
		schedules && schedules.forEach(function (schedule) {
			var scheduleStart = schedule.ScheduleStartTimeMoment();
			if (personIds.indexOf(schedule.PersonId) > -1 && (!earlistStart || scheduleStart < earlistStart)) {
				earlistStart = scheduleStart;
			}
		});
		// Set to 08:00 for empty schedule or day off
		return earlistStart || dateMoment.startOf('day').add(8, 'hour');
	};

	ScheduleHelperService.prototype.getEarliestStartOfSelectedSchedules = function (schedules, dateMoment, personIds) {
		return this.getEarliestStartMomentOfSelectedSchedules(schedules, dateMoment, personIds).toDate();
	};


	ScheduleHelperService.prototype.getLatestStartMomentOfSelectedSchedules = function (schedules, dateMoment, personIds) {
		checkPersonIds(personIds);

		var latestStart = null;
		schedules && schedules.forEach(function (schedule) {
			var scheduleStart = schedule.ScheduleStartTimeMoment();
			if (personIds.indexOf(schedule.PersonId) > -1 && (!latestStart || scheduleStart > latestStart)) {
				latestStart = scheduleStart;
			}
		});
		return latestStart;
	};

	ScheduleHelperService.prototype.getLatestStartOfSelectedSchedules = function (schedules, dateMoment, personIds) {
		var latestStart = this.getLatestStartMomentOfSelectedSchedules(schedules, dateMoment, personIds);
		if (latestStart) return latestStart.toDate();
	};

	ScheduleHelperService.prototype.getLatestPreviousDayOvernightShiftEndMoment = function (schedules, dateMoment, personIds) {
		checkPersonIds(personIds);

		var previousDayShifts = [];

		schedules && schedules.forEach(function (schedule) {
			if (personIds.indexOf(schedule.PersonId) > -1) {
				previousDayShifts = previousDayShifts.concat(schedule.Shifts.filter(function (shift) {
					return shift.Projections.length > 0 &&
						dateMoment.isAfter(shift.Date);
				}));
			}
		});

		if (previousDayShifts.length === 0) return null;

		var latestEndTimeMoment = null;
		previousDayShifts.forEach(function (shift) {
			shift.Projections.forEach(function (projection) {
				if (!latestEndTimeMoment || latestEndTimeMoment < projection.EndMoment)
					latestEndTimeMoment = projection.EndMoment;
			});
		});

		return latestEndTimeMoment;
	};

	ScheduleHelperService.prototype.getLatestPreviousDayOvernightShiftEnd = function (schedules, dateMoment, personIds) {
		var latestEndTimeMoment = this.getLatestPreviousDayOvernightShiftEndMoment(schedules, dateMoment, personIds);
		if (latestEndTimeMoment)
			return latestEndTimeMoment.toDate();
	};

	ScheduleHelperService.prototype.getLatestStartTimeMomentOfSelectedProjections = function (schedules, personIds) {
		checkPersonIds(personIds);

		var latestStart = null;
		var projectionShiftLayerIds = [];
		var shifts = [];

		schedules && schedules.forEach(function (schedule) {
			if (personIds.indexOf(schedule.PersonId) > -1) {
				shifts = shifts.concat(schedule.Shifts);
			}
		});

		shifts.forEach(function (shift) {
			if (shift.Projections) {
				shift.Projections.forEach(function (projection) {
					var scheduleStart = projection.StartMoment;
					if (projection.Selected && (latestStart === null || scheduleStart >= latestStart)) {
						var exist = projection.ShiftLayerIds && projection.ShiftLayerIds.some(function (layerId) {
							return projectionShiftLayerIds.indexOf(layerId) > -1;
						});
						if (exist) return;

						latestStart = scheduleStart;
						projectionShiftLayerIds = projectionShiftLayerIds.concat(projection.ShiftLayerIds);
					}
				});
			}
		});
		return latestStart;
	};

	ScheduleHelperService.prototype.getLatestStartTimeOfSelectedSchedulesProjections = function (schedules, dateMoment, personIds) {
		var lastestStartTimeMoment = this.getLatestStartTimeMomentOfSelectedProjections(schedules, personIds);
		if (lastestStartTimeMoment) return lastestStartTimeMoment.toDate();
	};

	function checkPersonIds(personIds) {
		personIds.forEach(function (x) {
			if (!angular.isString(x))
				throw 'Invalid parameter.';
		});
	}

})(angular);
