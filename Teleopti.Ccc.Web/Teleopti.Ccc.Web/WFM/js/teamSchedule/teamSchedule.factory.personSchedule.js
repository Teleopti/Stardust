﻿(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', PersonSchedule]);

	function PersonSchedule(currentUserInfo) {

		var personScheduleFactory = {
			Create: create
		};

		function create(schedule, timeLine, isOverNightShift) {
			if (!schedule) schedule = {};

			var isOvernightShift = false;
			if (schedule.Projection != undefined && schedule.Projection != null && schedule.Projection.length > 0) {
				var shiftStartTime = moment(schedule.Projection[0].Start);
				var lastProjection = schedule.Projection[schedule.Projection.length - 1];
				var shiftEndTime = moment(lastProjection.Start);
				shiftEndTime = shiftEndTime.add(lastProjection.Minutes, 'minutes');
				isOvernightShift = shiftStartTime.startOf('day').format("YYYY-MM-DD HH:mm:ss")
					!== shiftEndTime.startOf('day').format("YYYY-MM-DD HH:mm:ss");
			}

			var projectionVms = createProjections(schedule.Projection, timeLine, isOverNightShift);
			var dayOffVm = createDayOffViewModel(schedule.DayOff, timeLine);
			
			var personSchedule = {
				PersonId: schedule.PersonId,
			
				Name: schedule.Name,
				Date: moment.tz(schedule.Date, currentUserInfo.DefaultTimeZone),
				Shifts: projectionVms == undefined ? [] : [{Projections: projectionVms}],
				IsFullDayAbsence: schedule.IsFullDayAbsence,
				DayOffs: dayOffVm == undefined ? [] : [dayOffVm],
				Merge: merge,
				IsSelected: false,
				IsOvernightShift: isOvernightShift,
				AllowSwap: function () {
					return !this.IsFullDayAbsence && (this.Shifts.length === 0 || (this.Shifts.length === 1 && !this.IsOvernightShift));
				}
			}

			return personSchedule;
		};

		function createProjections(projections, timeLine, isOverNightShift) {
			if (projections == undefined || projections == null || projections.length === 0) {
				return undefined;
			}
			
			var projectionVms = [];
			projections.forEach(function (projection) {
				var proj = createShiftProjectionViewModel(projection, timeLine, isOverNightShift);
				if (proj !== undefined) {
					projectionVms.push(proj);
				}
			});

			return projectionVms;
		}

		function createDayOffViewModel(dayOff, timeLine) {
			if (dayOff == undefined || dayOff == null) {
				return undefined;
			}

			var startTime = moment(dayOff.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			if (startTimeMinutes > timeLine.EndMinute) {
				return undefined;
			}

			var displayStart = startTimeMinutes < timeLine.StartMinute ? timeLine.StartMinute : startTimeMinutes;
			var start = displayStart - timeLine.StartMinute;
			var startPosition = start * timeLine.LengthPercentPerMinute;

			var displayEnd = startTimeMinutes + dayOff.Minutes;
			displayEnd = displayEnd <= timeLine.EndMinute ? displayEnd : timeLine.EndMinute;
			var length = (displayEnd - displayStart) * timeLine.LengthPercentPerMinute;

			var dayOffVm = {
				DayOffName: dayOff.DayOffName,
				StartPosition: startPosition,
				Length: length
			};

			return dayOffVm;
		};

		function createShiftProjectionViewModel(projection, timeLine, isOverNightShift) {
			if (!projection) projection = {};

			var startTime = moment(projection.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			if ((startTimeMinutes > timeLine.EndMinute) || ((startTimeMinutes + projection.Minutes) <= timeLine.StartMinute)) {
				return undefined;
			}

			var displayStartTimeMinutes = startTimeMinutes >= 0 ? startTimeMinutes : 0;
			var start = displayStartTimeMinutes - timeLine.StartMinute;
			var startPosition = start * timeLine.LengthPercentPerMinute;

			var lengthMinutes = startTimeMinutes < 0 ? projection.Minutes - (startTimeMinutes * -1) : projection.Minutes;
			var length = lengthMinutes * timeLine.LengthPercentPerMinute;

			var shiftProjectionVm = {
				StartPosition: startPosition,
				Length: length,
				IsOvertime: projection.IsOvertime,
				Color: projection.Color,
				Description: projection.Description,
				IsOverNight: isOverNightShift,
				Start: projection.Start
			};

			return shiftProjectionVm;
		}

		function merge(otherSchedule, timeLine, isOverNightShift) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine, isOverNightShift);
			if (otherProjections != undefined) {
				this.Shifts.push({ Projections: otherProjections });
			}

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine);

			if (otherDayOffVm != undefined) {
				this.DayOffs.push(otherDayOffVm);
			}

			this.IsOvernightShift = true;
		}

		return personScheduleFactory;
	}
}());
