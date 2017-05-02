(function (angular, moment) {
    'use strict';

    angular.module('wfm.teamSchedule')
        .service('ScheduleHelper', ScheduleHelperService);

    function ScheduleHelperService() {}

    ScheduleHelperService.prototype.getEarliestStartOfSelectedSchedules = function (schedules, dateMoment, personIds) {

        personIds.forEach(function (x) {
            if (!angular.isString(x))
                throw 'Invalid parameter.';
        });

        var startUpdated = false;
        var earlistStart = moment('2099-12-31');

        schedules.forEach(function (schedule) {
            var scheduleStart = moment(schedule.ScheduleStartTime());

            if (personIds.indexOf(schedule.PersonId) > -1 && scheduleStart < earlistStart) {
                startUpdated = true;
                earlistStart = scheduleStart;
            }
        });

        if (!startUpdated) {
            // Set to 08:00 for empty schedule or day off
            earlistStart = dateMoment.startOf('day').add(8, 'hour');
        }

        return earlistStart.toDate();
    };

    ScheduleHelperService.prototype.getLatestStartOfSelectedSchedules = function (schedules, dateMoment, personIds) {
        personIds.forEach(function (x) {
            if (!angular.isString(x))
                throw 'Invalid parameter.';
        });

        var startUpdated = false;
        var latestStart = dateMoment.startOf('day');

        schedules.forEach(function (schedule) {
            var scheduleStart = moment(schedule.ScheduleStartTime());

            if (personIds.indexOf(schedule.PersonId) > -1 && scheduleStart > latestStart) {
                startUpdated = true;
                latestStart = scheduleStart;
            }
        });

        return startUpdated ? latestStart.toDate() : null;
    };

    ScheduleHelperService.prototype.getLatestPreviousDayOvernightShiftEnd = function (schedules, dateMoment, personIds) {
        personIds.forEach(function (x) {
            if (!angular.isString(x))
                throw 'Invalid parameter.';
        });

        var previousDayShifts = [];

        schedules.forEach(function (schedule) {
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
                var projectionEndMoment = moment(projection.Start).add(projection.Minutes, 'minute');
                if (latestEndTimeMoment === null || latestEndTimeMoment < projectionEndMoment)
                    latestEndTimeMoment = projectionEndMoment;
            });
        });

        return latestEndTimeMoment ? latestEndTimeMoment.toDate() : null;
    };

    ScheduleHelperService.prototype.getLatestStartTimeOfSelectedSchedulesProjections = function (schedules, dateMoment, personIds) {

        personIds.forEach(function (x) {
            if (!angular.isString(x))
                throw 'Invalid parameter.';
        });

        var latestStart = null;
        var projectionShiftLayerIds = [];
        var shifts = [];

        schedules.forEach(function (schedule) {
            if (personIds.indexOf(schedule.PersonId) > -1) {
                shifts = shifts.concat(schedule.Shifts);
            }
        });

        shifts.forEach(function (shift) {
            if (shift.Projections) {
                shift.Projections.forEach(function (projection) {
                    var scheduleStart = moment(projection.Start).toDate();
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

})(angular, moment);
