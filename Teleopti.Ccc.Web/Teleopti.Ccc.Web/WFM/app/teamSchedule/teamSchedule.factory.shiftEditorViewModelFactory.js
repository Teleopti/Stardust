﻿(function () {
	'use strict';

	angular.module('wfm.teamSchedule').factory('ShiftEditorViewModelFactory', ShiftEditorViewModelFactory);
	ShiftEditorViewModelFactory.$inject = ['serviceDateFormatHelper', 'Toggle', 'CurrentUserInfo'];
	function ShiftEditorViewModelFactory(serviceDateFormatHelper, toggleSvc, CurrentUserInfo) {
		var factory = {
			CreateTimeline: function (date, timezone, timeRange) {
				return new TimelineViewModel(date, timezone, timeRange);
			},
			CreateSchedule: function (date, timezone, schedule) {
				return new ScheduleViewModel(date, timezone, schedule);
			}
		};

		function ScheduleViewModel(date, timezone, schedule) {
			if (!schedule) return;
			var currentTimezone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var layers = createShiftLayers(schedule.Projection, date, timezone, currentTimezone);

			var hasUnderlyingSchedules =
				toggleSvc.WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952 &&
				!!schedule.UnderlyingScheduleSummary;
			var underlyingScheduleSummary = hasUnderlyingSchedules
				? getUnderlyingSummarySchedule(schedule.UnderlyingScheduleSummary, date, timezone, currentTimezone)
				: null;

			this.Date = date;
			this.Name = schedule.Name;
			this.Timezone = schedule.Timezone.IanaId;
			this.ProjectionTimeRange = getProjectionTimeRange(layers);
			this.ShiftLayers = layers;
			this.HasUnderlyingSchedules = hasUnderlyingSchedules;
			this.UnderlyingScheduleSummary = underlyingScheduleSummary;
		}

		ScheduleViewModel.prototype.AddLayer = function (layer, startTime, endTime, index) {
			this.ShiftLayers = this.ShiftLayers || [];
			var copyLayer = Object.assign({}, layer);
			copyLayer.Start = startTime;
			copyLayer.End = endTime;

			var newLayer = new ShiftLayerViewModel(copyLayer, this.Date, this.Timezone, this.Timezone);

			this.ShiftLayers.splice(index, 0, newLayer);

			return newLayer;
		}

		ScheduleViewModel.prototype.GetSummaryTimeSpan = function (info) {
			return info.TimeSpan;
		};

		function createShiftLayers(projections, date, timezone, currentTimezone) {
			var layers = [];
			projections &&
				projections.forEach(function (projection) {
					var layer = new ShiftLayerViewModel(projection, date, timezone, currentTimezone);
					layers.push(layer);
				});

			for (var i = 0; i < layers.length; i++) {
				if (i == 0) continue;
				layers[i].ShowDividedLine = layers[i].ActivityId === layers[i - 1].ActivityId;
			}

			return layers;
		}

		function getUnderlyingSummarySchedule(underlyingScheduleSummary, date, timezone, currentTimezone) {
			return {
				PersonalActivities: getUnderlyingSummaryViewModel(
					underlyingScheduleSummary.PersonalActivities,
					date,
					timezone,
					currentTimezone
				),
				PersonPartTimeAbsences: getUnderlyingSummaryViewModel(
					underlyingScheduleSummary.PersonPartTimeAbsences,
					date,
					timezone,
					currentTimezone
				),
				PersonMeetings: getUnderlyingSummaryViewModel(
					underlyingScheduleSummary.PersonMeetings,
					date,
					timezone,
					currentTimezone
				)
			};
		}

		function getUnderlyingSummaryViewModel(items, date, timezone, currentTimezone) {
			var result = [];
			if (items && items.length) {
				result = items.map(function (item) {
					var startInToTimezone = moment
						.tz(item.Start, currentTimezone)
						.clone()
						.tz(timezone);
					var endInToTimezone = moment
						.tz(item.End, currentTimezone)
						.clone()
						.tz(timezone);
					var timeSpan = getTimeSpan(startInToTimezone, endInToTimezone);
					return {
						TimeSpan: timeSpan,
						Description: item.Description
					};
				});
			}
			return result;
		}

		function getProjectionTimeRange(layers) {
			if (!layers.length) {
				return null;
			}
			return {
				Start: layers[0].Start,
				End: layers[layers.length - 1].End
			};
		}

		function ShiftLayerViewModel(layer, date, timezone, fromTimezone) {
			var isSameTimezone = timezone === fromTimezone;
			var startInTimezone = isSameTimezone
				? moment.tz(layer.Start, timezone)
				: moment
					.tz(layer.Start, fromTimezone)
					.clone()
					.tz(timezone);
			var endInTimezone = isSameTimezone
				? moment.tz(layer.End, timezone)
				: startInTimezone.clone().add(layer.Minutes, 'minutes');

			this.Description = layer.Description;
			this.Color = layer.Color;
			this.ActivityId = layer.ActivityId;
			this.Start = serviceDateFormatHelper.getDateTime(startInTimezone);
			this.End = serviceDateFormatHelper.getDateTime(endInTimezone);
			this.Minutes = layer.Minutes;
			this.ShiftLayerIds = layer.ShiftLayerIds;
			this.TopShiftLayerId = layer.TopShiftLayerId;
			this.TimeSpan = getTimeSpan(startInTimezone, endInTimezone);
			this.IsOvertime = layer.IsOvertime;
			this.ShowDividedLine = false;
			this.FloatOnTop = !!layer.FloatOnTop;
		}

		ShiftLayerViewModel.prototype.UseLighterBorder = function (color) {
			return useLighterColor(color || this.Color);
		};

		function TimelineViewModel(date, timezone, timeRange) {
			return {
				Intervals: getIntervals(date, timezone, timeRange),
				TimeRange: timeRange
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

		function getTimeSpan(start, end) {
			return start.format('L LT') + ' - ' + end.format('L LT');
		}

		function useLighterColor(color) {
			var getLumi = function (cstring) {
				var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
				if (!matched) return null;
				return (
					(299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) /
					1000
				);
			};
			var lightColor = '#00ffff';
			var darkColor = '#795548';
			var lumi = getLumi(color);
			if (!lumi) return false;
			return Math.abs(lumi - getLumi(lightColor)) > Math.abs(lumi - getLumi(darkColor));
		}

		function getIntervals(date, timezone, timeRange) {
			var intervals = [];
			var startTime = timeRange.Start.clone();
			var endTime = timeRange.End.clone();
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
