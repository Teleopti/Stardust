(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', PersonSchedule]);

	function PersonSchedule(currentUserInfo) {
		var personScheduleVm = {};

		var shiftProjectionViewModel = function(projection, timeLine) {
			if (!projection) projection = {};

			var startTime = moment(projection.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

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
				Color: projection.Color,
				Description: projection.Description
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

		var createProjections = function (projections, timeLine) {
			if (projections == undefined || projections == null || projections.length === 0) {
				return undefined;
			}

			var projectionVms = [];
			angular.forEach(projections, function(projection) {
				projectionVms.push(new shiftProjectionViewModel(projection, timeLine));
			});

			return projectionVms;
		}

		var merge = function(otherSchedule, timeLine) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine);
			this.Shifts.push({ Projections: otherProjections });

			var otherDayOffVm = createDayOffViewModel(otherSchedule.DayOff, timeLine);

			if (otherDayOffVm != undefined) {
				this.DayOffs.push(otherDayOffVm);
			}
		}

		personScheduleVm.Create = function(personSchedule, timeLine) {
			if (!personSchedule) personSchedule = {};

			var projectionVms = createProjections(personSchedule.Projection, timeLine);
			var dayOffVm  = createDayOffViewModel(personSchedule.DayOff, timeLine);

			var vm = {
				PersonId: personSchedule.PersonId,
				Name: personSchedule.Name,
				Date: moment.tz(personSchedule.Date, currentUserInfo.DefaultTimeZone),
				Shifts: projectionVms == undefined ? [] : [
					{
						Projections: projectionVms
					}
				],
				IsFullDayAbsence: personSchedule.IsFullDayAbsence,
				DayOffs: dayOffVm == undefined ? [] : [dayOffVm],
				Merge: merge
			}

			return vm;
		}

		return personScheduleVm;
	}
}());
