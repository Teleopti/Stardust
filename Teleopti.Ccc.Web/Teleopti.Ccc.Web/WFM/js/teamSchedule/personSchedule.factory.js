(function() {
	'use strict';
	angular.module('wfm.teamSchedule').factory('PersonSchedule', ['CurrentUserInfo', 'TimeLineUnit', PersonSchedule]);

	function PersonSchedule(currentUserInfo, timeLineUnit) {

		var personScheduleVm = {};

		var shiftProjectionViewModel = function(projection, baseDate, timeLine) {
			if (!projection) projection = {};

			var startTime = moment.tz(projection.Start, currentUserInfo.DefaultTimeZone);
			var startTimeMinutes = startTime.diff(baseDate, 'minutes');
			var shiftProjectionVm = {
				StartPixels: function() {
					var start = startTimeMinutes - timeLine.StartMinute;
					var pixels = start * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				},
				LengthPixels: function() {
					var lengthMinutes = startTimeMinutes < 0 ? projection.Minutes - (startTimeMinutes * -1) : projection.Minutes;
					var pixels = lengthMinutes * timeLine.PixelsPerMinute;
					return Math.round(pixels);
				},
				Color: projection.Color
			};

			return shiftProjectionVm;
		}

		var dayOffViewModel = function(dayOff, baseDate, timeLine) {
			if (!dayOff) dayOff = {}

			var startTime = moment.tz(dayOff.Start, currentUserInfo.DefaultTimeZone);
			var startTimeMinutes = startTime.diff(baseDate, 'minutes');
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

		//person Schedule ViewModel
		personScheduleVm.Create = function(personSchedule, queryDate, timeLine) {
			if (!personSchedule) personSchedule = {};

			var projectionVms = [];
			angular.forEach(personSchedule.Projection, function(projection) {
				var unit = timeLineUnit;
				unit.init(projection, timeLine);
				projection.Offset = queryDate;
				projection.TimeLineAffectingStartMinute = unit.CutInsideDayStartMinutes();
				projectionVms.push(new shiftProjectionViewModel(projection, queryDate, timeLine));
			});

			var vm = {
				PersonId: personSchedule.PersonId,
				Name: personSchedule.Name,
				Date: moment.tz(personSchedule.Date, currentUserInfo.DefaultTimeZone),
				ShiftProjections: projectionVms,
				IsFullDayAbsence: personSchedule.IsFullDayAbsence,
				DayOff: personSchedule.DayOff == undefined || personSchedule.DayOff == null ? {}
					: new dayOffViewModel(personSchedule.DayOff, queryDate, timeLine)
			}

			return vm;
		}

		return personScheduleVm;
	}
}());
