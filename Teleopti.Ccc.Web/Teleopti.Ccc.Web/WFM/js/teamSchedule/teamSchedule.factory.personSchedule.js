(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', PersonSchedule]);

	function PersonSchedule(currentUserInfo) {

		var personScheduleFactory = {
			Create: create
		};

		var getPersonAbsencesCount = function() {
			var personAbsences = [];
			angular.forEach(this.Projections, function(projection) {
				if (projection.ParentPersonAbsence != null && personAbsences.indexOf(projection.ParentPersonAbsence) === -1) {
					personAbsences.push(projection.ParentPersonAbsence);
				}
			});
			return personAbsences.length;
		};

		var getPersonActivitiesCount = function() {
			var personActivities =[];
			angular.forEach(this.Projections, function(projection) {
			    if (projection.ShiftLayerId != null && personActivities.indexOf(projection.ParentPersonAbsence) === -1) {
			        personActivities.push(projection.ShiftLayerId);
				}
			});
			return personActivities.length;
		};

		function create(schedule, timeLine) {
			if (!schedule) schedule = {};
			var projectionVms = createProjections(schedule.Projection, timeLine);
			var dayOffVm = createDayOffViewModel(schedule.DayOff, timeLine);
			
			var personSchedule = {
				PersonId: schedule.PersonId,

				Name: schedule.Name,
				Date: moment.tz(schedule.Date, currentUserInfo.DefaultTimeZone),
				Shifts: projectionVms == undefined ? [] : [
					{
						Date: schedule.Date,
						Projections: projectionVms,
						AbsenceCount: getPersonAbsencesCount,
						ActivityCount: getPersonActivitiesCount
					}
				],
				IsFullDayAbsence: schedule.IsFullDayAbsence,
				DayOffs: dayOffVm == undefined ? [] : [dayOffVm],
				Merge: merge,
				IsSelected: false,
				AllowSwap: function() {
					return !this.IsFullDayAbsence;
				},
				ScheduleStartTime: function () {
					var start = schedule.Date;
					angular.forEach(this.Shifts, function(shift) {
						if (start === shift.Date && shift.Projections.length > 0) {
							start = shift.Projections[0].Start;
						}
					});
					return start;
				},
				ScheduleEndTime: function () {
					var start = schedule.Date;
					var end = moment(schedule.Date).endOf('day');
					angular.forEach(this.Shifts, function (shift) {
						if (start === shift.Date && shift.Projections.length > 0) {
							end = moment(shift.Projections[shift.Projections.length - 1].Start).add(shift.Projections[shift.Projections.length - 1].Minutes,'minutes');
						}
					});
					return end.format("YYYY-MM-DD HH:mm");
				}
			}

			return personSchedule;
		};

		function createProjections(projections, timeLine) {
			if (projections == undefined || projections == null || projections.length === 0) {
				return undefined;
			}
			
			var projectionVms = [];
			projections.forEach(function (projection) {
				var proj = createShiftProjectionViewModel(projection, timeLine);
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

		function createShiftProjectionViewModel(projection, timeLine) {
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
				ParentPersonAbsence: projection.ParentPersonAbsence,
				ShiftLayerId: projection.ShiftLayerId,
				StartPosition: startPosition,
				Length: length,
				IsOvertime: projection.IsOvertime,
				Color: projection.Color,
				Description: projection.Description,
				Start: projection.Start,
				Selected: false,
				ToggleSelection: function () {
					// Select person absence and activity
					this.Selected = !this.Selected;
				},
				Minutes: projection.Minutes
			};

			return shiftProjectionVm;
		}

		function merge(otherSchedule, timeLine) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine);
			if (otherProjections != undefined) {
				this.Shifts.push({
					Date: otherSchedule.Date,
					Projections: otherProjections,
					AbsenceCount: getPersonAbsencesCount,
					ActivityCount: getPersonActivitiesCount
				});
			}

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine);

			if (otherDayOffVm != undefined) {
				this.DayOffs.push(otherDayOffVm);
			} else {
				this.DayOffs = [];
			}
		}

		return personScheduleFactory;
	}
}());
