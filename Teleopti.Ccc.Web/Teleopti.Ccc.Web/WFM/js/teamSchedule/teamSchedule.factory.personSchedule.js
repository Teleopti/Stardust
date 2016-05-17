﻿(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', PersonSchedule]);

	function PersonSchedule(currentUserInfo) {

		var personScheduleFactory = {
			Create: create
		};

		var getPersonAbsencesCount = function() {
			var personAbsences = [];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ParentPersonAbsences != null) {
					angular.forEach(projection.ParentPersonAbsences, function(personAbsId) {
						if (personAbsences.indexOf(personAbsId) === -1) {
							personAbsences.push(personAbsId);
						}
					});
				}
			});
			return personAbsences.length;
		};

		var getPersonActivitiesCount = function() {
			var personActivities =[];
			angular.forEach(this.Projections, function (projection) {
				if (projection.ShiftLayerIds != null && !projection.IsOvertime) {
					angular.forEach(projection.ShiftLayerIds, function(shiftLayerId) {
						if (personActivities.indexOf(shiftLayerId) === -1) {
							personActivities.push(shiftLayerId);
						}
					});
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
				Date: moment.tz(schedule.Date, currentUserInfo.CurrentUserInfo().DefaultTimeZone),
				Shifts: projectionVms == undefined ? [] : [
					{
						Date: moment.tz(schedule.Date, currentUserInfo.CurrentUserInfo().DefaultTimeZone),
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
				ContractTime: formatContractTimeMinutes(schedule.ContractTimeMinutes),
				ScheduleStartTime: function () {
					var start = this.Date.format('YYYY-MM-DD hh:mm');
					angular.forEach(this.Shifts, function(shift) {
						if (shift.Date.isSame(start,'day') && shift.Projections.length > 0) {
							start = shift.Projections[0].Start;
						}
					});
					return start;
				},
				ScheduleEndTime: function () {
					var start = this.Date;
					var end = moment(schedule.Date).endOf('day');
					angular.forEach(this.Shifts, function (shift) {
						if (shift.Date.isSame(start,'day') && shift.Projections.length > 0) {
							end = moment(shift.Projections[shift.Projections.length - 1].Start).add(shift.Projections[shift.Projections.length - 1].Minutes,'minutes');
						}
					});
					return end.format("YYYY-MM-DD HH:mm");
				},
				AbsenceCount: function(){
					var shiftsForCurrentDate = this.Shifts.filter(function (shift) {
						return this.Date.isSame(shift.Date, 'day');
					}, this);
					if(shiftsForCurrentDate.length > 0) {
						return shiftsForCurrentDate[0].AbsenceCount();
					}
					return 0;
				},
				ActivityCount: function(){
					var shiftsForCurrentDate = this.Shifts.filter(function (shift) {
						return this.Date.isSame(shift.Date, 'day');
					}, this);
					if(shiftsForCurrentDate.length > 0) {
						return shiftsForCurrentDate[0].ActivityCount();
					}
					return 0;
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

			var borderColorPicker = function (color) {
				var getLumi = function(cstring) {
					var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
					if (!matched) return null;
					return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
				}

				var lightColor = "#00ffff";
				var darkColor = "#795548";

				var lumi = getLumi(color);
				if (!lumi) return false;
				return Math.abs(lumi - getLumi(lightColor)) > Math.abs(lumi - getLumi(darkColor));
			};

			var shiftProjectionVm = {
				ParentPersonAbsences: projection.ParentPersonAbsences,
				ShiftLayerIds: projection.ShiftLayerIds,
				StartPosition: startPosition,
				Length: length,
				IsOvertime: projection.IsOvertime,
				Color: projection.Color,
				Description: projection.Description,
				Start: projection.Start,
				Selected: false,
				ToggleSelection: function () {
					this.Selected = !this.Selected;
				},
				Minutes: projection.Minutes,
				UseLighterBorder: borderColorPicker(projection.Color)
			};

			return shiftProjectionVm;
		}

		function merge(otherSchedule, timeLine) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine);
			if (otherProjections != undefined) {
				this.Shifts.push({
					Date: moment.tz(otherSchedule.Date, currentUserInfo.CurrentUserInfo().DefaultTimeZone),
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

		function formatContractTimeMinutes(minutes) {
			var time = moment().startOf('day').add(minutes, 'minutes');
			return time.format("H:mm");
		}

		return personScheduleFactory;
	}
}());
