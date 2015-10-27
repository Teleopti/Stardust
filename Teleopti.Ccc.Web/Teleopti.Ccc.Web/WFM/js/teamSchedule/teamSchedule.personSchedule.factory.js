(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', 'TimeLineUnit', PersonSchedule]);

	function PersonSchedule(currentUserInfo, timeLineUnit) {

		var personScheduleVm = {};

		var shiftProjectionViewModel = function(projection, timeLine) {
			if (!projection) projection = {};

			var startTime = moment(projection.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');

			var shiftProjectionVm = {
				StartPixels: function() {
					var displayStartTimeMinutes = startTimeMinutes >= 0 ? startTimeMinutes : 0;
					var start = displayStartTimeMinutes - timeLine.StartMinute;
					var pixels = start * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				},
				LengthPixels: function() {
					var lengthMinutes = startTimeMinutes < 0 ? projection.Minutes - (startTimeMinutes * -1) : projection.Minutes;
					var pixels = lengthMinutes * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				},
				Color: projection.Color,
				Description: projection.Description
			};

			return shiftProjectionVm;
		}

		var dayOffViewModel = function(dayOff, timeLine) {
			if (!dayOff) dayOff = {}

			var startTime = moment(dayOff.Start);
			var startTimeMinutes = startTime.diff(timeLine.Offset, 'minutes');
			var dayOffStartMinutes = startTimeMinutes < timeLine.StartMinute ? timeLine.StartMinute : startTimeMinutes;
			var dayOffVm = {
				DayOffName: dayOff.DayOffName,
				StartPixels: function() {
					var start = dayOffStartMinutes - timeLine.StartMinute;
					var pixels = start * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				},
				LengthPixels: function() {
					var lengthMinutes = dayOff.Minutes;
					if (timeLine.EndMinute < startTimeMinutes + lengthMinutes)
						lengthMinutes = timeLine.EndMinute - dayOffStartMinutes;
					if (startTimeMinutes < timeLine.StartMinute)
						lengthMinutes = startTimeMinutes + lengthMinutes - timeLine.StartMinute;
					var pixels = lengthMinutes * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				}
			};

			return dayOffVm;
		}

		var createProjections = function(projections, timeLine) {
			var projectionVms = [];
			angular.forEach(projections, function(projection) {
				var unit = timeLineUnit;
				unit.init(projection, timeLine);
				projection.Offset = timeLine.Offset;
				projection.TimeLineAffectingStartMinute = unit.CutInsideDayStartMinutes();
				projectionVms.push(new shiftProjectionViewModel(projection, timeLine));
			});

			return projectionVms;
		}

		var merge = function (otherSchedule, timeLine) {
			var otherProjections = createProjections(otherSchedule.Projection, timeLine);
			this.ShiftProjections = this.ShiftProjections.concat(otherProjections);
			if (this.DayOff == undefined && otherSchedule.DayOff != undefined && otherSchedule.DayOff != null){
				this.DayOff = new dayOffViewModel(otherSchedule.DayOff, timeLine);
			}
		}

		personScheduleVm.Create = function(personSchedule, timeLine) {
			if (!personSchedule) personSchedule = {};

			var projectionVms = createProjections(personSchedule.Projection, timeLine);

			var vm = {
				PersonId: personSchedule.PersonId,
				Name: personSchedule.Name,
				Date: moment.tz(personSchedule.Date, currentUserInfo.DefaultTimeZone),
				ShiftProjections: projectionVms,
				IsFullDayAbsence: personSchedule.IsFullDayAbsence,
				DayOff: personSchedule.DayOff == undefined || personSchedule.DayOff == null ? undefined
					: new dayOffViewModel(personSchedule.DayOff, timeLine)
			}

			vm.Merge = merge;

			return vm;
		}

		return personScheduleVm;
	}
}());
