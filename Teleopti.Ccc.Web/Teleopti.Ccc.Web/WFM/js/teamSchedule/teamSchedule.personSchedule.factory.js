(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', PersonSchedule]);

	function PersonSchedule(currentUserInfo) {
		var createShiftProjectionViewModel = function(projection, timeLine, isOverNightShift) {
			if (!projection) projection = {};

			var startTime = moment(projection.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			if ((startTimeMinutes > timeLine.EndMinute) || ((startTimeMinutes + projection.Minutes) <= timeLine.StartMinute)) {
				return undefined;
			}

			var shiftProjectionVm = {
				StartPosition: function() {
					var displayStartTimeMinutes = startTimeMinutes >= 0 ? startTimeMinutes : 0;
					var start = displayStartTimeMinutes - timeLine.StartMinute;
					var position = start * timeLine.LengthPercentPerMinute;
					return position;
				},
				Length: function() {
					var lengthMinutes = startTimeMinutes < 0 ? projection.Minutes - (startTimeMinutes * -1) : projection.Minutes;
					var position = lengthMinutes * timeLine.LengthPercentPerMinute;
					return position;
				},
				IsOvertime: projection.IsOvertime,
				Color: projection.Color,
				Description: projection.Description,
				IsOverNight: isOverNightShift
			};

			return shiftProjectionVm;
		}

		var createDayOffViewModel = function (dayOff, timeLine) {
			if (dayOff == undefined || dayOff == null) {
				return undefined;
			}

			var startTime = moment(dayOff.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			if (startTimeMinutes > timeLine.EndMinute) {
				return undefined;
			}

			var displayStart = startTimeMinutes < timeLine.StartMinute ? timeLine.StartMinute : startTimeMinutes;
			var dayOffVm = {
				DayOffName: dayOff.DayOffName,
				StartPosition: function() {
					var start = displayStart - timeLine.StartMinute;
					var position = start * timeLine.LengthPercentPerMinute;
					return position;
				},
				Length: function() {
					var displayEnd = startTimeMinutes + dayOff.Minutes;
					displayEnd = displayEnd <= timeLine.EndMinute ? displayEnd : timeLine.EndMinute;
					var position = (displayEnd - displayStart) * timeLine.LengthPercentPerMinute;
					return position;
				}
			};

			return dayOffVm;
		}

		var createProjections = function (projections, timeLine, isOverNightShift) {
			if (projections == undefined || projections == null || projections.length === 0) {
				return undefined;
			}

			var projectionVms = [];
			angular.forEach(projections, function (projection) {
				var proj = createShiftProjectionViewModel(projection, timeLine, isOverNightShift);
				if (proj !== undefined) {
					projectionVms.push(proj);
				} else {
					console.log("Projection started from this time will be give up for timeline:", projection.Start);
				}
			});

			return projectionVms;
		}

		var merge = function (otherSchedule, timeLine, isOverNightShift) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine, isOverNightShift);
			if (otherProjections != undefined) {
				this.Shifts.push({ Projections: otherProjections });
			}

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine);

			if (otherDayOffVm != undefined) {
				this.DayOffs.push(otherDayOffVm);
			}
		}
		
		var sortValue = function () {
			if (this.Shifts.length === 0) {
				if (this.DayOffs.length === 0) {
					// Empty schedule at last
					return 20000;
				} else {
					// DayOff after schedule
					return 10000;
				}
			}

			var shiftStart = -24*60;
			angular.forEach(this.Shifts[0].Projections, function(projection) {
				var projStart = projection.StartPosition();
				if (shiftStart === -24 * 60 || projStart < shiftStart) {
					shiftStart = projStart;
				}
			});

			if (this.IsFullDayAbsence) {
				return 5000 + shiftStart;
			}

			return shiftStart;
		};

		var create = function(schedule, timeLine, isOverNightShift) {
			if (!schedule) schedule = {};

			var projectionVms = createProjections(schedule.Projection, timeLine, isOverNightShift);
			var dayOffVm = createDayOffViewModel(schedule.DayOff, timeLine);

			var personSchedule = {
				PersonId: schedule.PersonId,
				Name: schedule.Name,
				Date: moment.tz(schedule.Date, currentUserInfo.DefaultTimeZone),
				Shifts: projectionVms == undefined ? [] : [
					{
						Projections: projectionVms
					}
				],
				IsFullDayAbsence: schedule.IsFullDayAbsence,
				DayOffs: dayOffVm == undefined ? [] : [dayOffVm],
				Merge: merge,
				SortValue: sortValue
			}

			return personSchedule;
		}

		var personScheduleFactory = {
			Create: create
		};
		return personScheduleFactory;
	}
}());
