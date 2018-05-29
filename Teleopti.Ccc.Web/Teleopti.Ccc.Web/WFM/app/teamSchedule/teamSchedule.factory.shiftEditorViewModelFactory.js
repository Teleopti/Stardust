(function () {
	'use strict';

	angular.module('wfm.teamSchedule').factory('ShiftEditorViewModelFactory', ShiftEditorViewModelFactory);
	ShiftEditorViewModelFactory.$inject = ['serviceDateFormatHelper'];
	function ShiftEditorViewModelFactory(serviceDateFormatHelper) {
		var factory = {
			CreateTimeline: function (date, timezone) {
				return new TimelineViewModel(date, timezone);
			},
			CreateSchedule: function (date, timezone, schedule) {
				return new ScheduleViewModel(date, timezone, schedule);
			}
		};

		function ScheduleViewModel(date, timezone, schedule) {
			if (!schedule)
				return;
			var scheduleTimezone = schedule.Timezone.IanaId;
			var layers = [];
			schedule.Projection && schedule.Projection.forEach(function (projection) {
				var layer = new ShiftLayerViewModel(projection, date, timezone, scheduleTimezone);
				layers.push(layer);
			});

			return {
				Date: date,
				Timezone: scheduleTimezone,
				ShiftLayers: layers
			}
		}

		function ShiftLayerViewModel(layer, date, timezone, fromTimezone) {
			var startInToTimezone = moment.tz(layer.Start, fromTimezone).clone().tz(timezone);
			var endInToTimezone = startInToTimezone.clone().add(layer.Minutes, 'minutes')
			return {
				Description: layer.Description,
				Start: serviceDateFormatHelper.getDateTime(startInToTimezone),
				End: serviceDateFormatHelper.getDateTime(endInToTimezone),
				Minutes: layer.Minutes,
				ShiftLayerIds: layer.ShiftLayerIds,
				Width: layer.Minutes,
				Left: getTotalMinutes(date, timezone, layer.Start),
				Color: layer.Color,
				UseLightColor: useLightColor(layer.Color),
				TimeSpan: getProjectionTimeSpan(startInToTimezone, endInToTimezone),
				IsOvertime: layer.IsOvertime
			};
		}

		function TimelineViewModel(date, timezone) {
			return {
				Intervals: getIntervals(date, timezone)
			};
		}

		function IntervalViewModel(datetime, isTheEnd) {
			return {
				Label: datetime.format('LT'),
				Time: datetime,
				Ticks: getTicks(datetime, isTheEnd)
			};
		}

		function TickViewModel(datetime, isHalfHour, isHour) {
			return {
				Time: datetime.clone(),
				IsHalfHour: isHalfHour,
				IsHour: isHour
			};
		}

		function getProjectionTimeSpan(start, end) {
			if (!start.isSame(end, 'day')) {
				return start.format('YYYY-MM-DD LT') + ' - ' + end.format('YYYY-MM-DD LT');
			}
			return start.format('LT') + ' - ' + end.format('LT');
		}

		function getTotalMinutes(date, timezone, dateTime) {
			var hours = 0;
			var currentDate = moment.tz(date, timezone);
			var dateTimeMoment = moment.tz(dateTime, timezone);
			while (dateTimeMoment.diff(currentDate, 'hours') > 0) {
				hours += 1;
				currentDate = currentDate.add(1, 'hours');
			}
			return hours * 60 + moment(dateTime).minutes();
		}

		function useLightColor(color) {
			var getLumi = function (cstring) {
				var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
				if (!matched) return null;
				return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
			};
			var lightColor = '#00ffff';
			var darkColor = '#795548';
			var lumi = getLumi(color);
			if (!lumi) return false;
			return Math.abs(lumi - getLumi(lightColor)) > Math.abs(lumi - getLumi(darkColor));
		}

		function getIntervals(date, timezone) {
			var intervals = [];
			var startTime = moment.tz(date, timezone);
			var endTime = moment.tz(date, timezone).add(1, 'days').hours(12);
			while (startTime <= endTime) {
				intervals.push(new IntervalViewModel(startTime.clone(), startTime.isSame(endTime)));
				startTime = startTime.add(1, 'hours');
			}
			return intervals;
		}

		function getTicks(datetime, isTheEnd) {
			var startTime = datetime.clone();
			var endTime = datetime.clone().add(1, 'hours');

			var ticks = [];
			if (isTheEnd) {
				ticks.push(new TickViewModel(startTime, false, true));
				return ticks;
			}

			while (startTime < endTime) {
				var minutes = startTime.minutes();
				var isHalfHour = minutes !== 0 && minutes % 30 === 0;
				var isHour = minutes === 0;
				ticks.push(new TickViewModel(startTime.clone(), isHalfHour, isHour));
				startTime.add(5, 'minutes');
			}
			return ticks;
		}

		return factory;
	}

})();