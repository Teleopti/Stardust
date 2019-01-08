(function() {
	'use strict';
	angular
		.module('wfm.requests')
		.service('requestScheduleService', ['$locale', '$filter', 'colorUtils', requestScheduleService]);

	function requestScheduleService($locale, $filter, colorUtils) {
		var svc = this;

		svc.formatTime = function(dateTime, currenDateTimesTimeZone, targetTimeZone) {
			var currentDateTime = moment.tz(dateTime, currenDateTimesTimeZone).toDate();

			targetTimeZone = moment
				.tz(dateTime, targetTimeZone)
				.format('Z')
				.replace(':', '');

			return $filter('date')(currentDateTime, $locale.DATETIME_FORMATS.shortTime, targetTimeZone);
		};

		svc.buildScheduleContainerStyle = function(shiftsLength, $event) {
			var containerWidth = 755;
			var headerHeight = 40 + 12;
			var shiftRowHeight = 60;
			var containerHeight = headerHeight + shiftsLength * shiftRowHeight;
			var containerMaxHeight = 242;
			var iconWidth = 20;

			return {
				width: containerWidth + 'px',
				height: containerHeight + 'px',
				'max-height': containerMaxHeight + 'px',
				top:
					document.body.clientHeight - ($event.pageY + $event.offsetY) <
					(containerHeight < containerMaxHeight ? containerHeight : containerMaxHeight)
						? document.body.clientHeight -
						  (containerHeight < containerMaxHeight ? containerHeight : containerMaxHeight) +
						  'px'
						: $event.pageY + ($event.target.offsetHeight - $event.offsetY) + 'px',
				left:
					document.body.clientWidth - ($event.pageX + iconWidth) < containerWidth
						? document.body.clientWidth - containerWidth + 'px'
						: $event.pageX + iconWidth + 'px'
			};
		};

		svc.buildShiftData = function(shift, currentTimeZone, targetTimeZone) {
			return {
				Name: shift.Name,
				Date: getShiftDate(shift.BelongsToDate, currentTimeZone, targetTimeZone),
				Periods: buildPeriods(shift.Periods, currentTimeZone, targetTimeZone),
				IsDayOff: shift.IsDayOff,
				DayOffName: shift.DayOffName,
				IsNotScheduled: shift.IsNotScheduled,
				ShiftCategory: buildShiftCategoryVisualData(shift.ShiftCategory),
				ShiftStartTime: getShiftStartTime(shift.Periods, currentTimeZone, targetTimeZone),
				ShiftEndTime: getShiftEndTime(shift.Periods, currentTimeZone, targetTimeZone)
			};
		};

		function getShiftDate(belongsToDate, currentTimeZone, targetTimeZone) {
			return $filter('date')(
				moment
					.tz(belongsToDate, currentTimeZone)
					.tz(targetTimeZone)
					.format('YYYY-MM-DDTHH:mm:ss'),
				$locale.DATETIME_FORMATS.shortDate
			);
		}

		function getShiftStartTime(periods, currentTimeZone, targetTimeZone) {
			if (!periods || periods.length == 0) return '';
			return svc.formatTime(periods[0].StartTime, currentTimeZone, targetTimeZone);
		}

		function getShiftEndTime(periods, currentTimeZone, targetTimeZone) {
			if (!periods || periods.length == 0) return '';

			var shiftEndTime = svc.formatTime(periods[periods.length - 1].EndTime, currentTimeZone, targetTimeZone);

			var shiftStartDateTime = moment.tz(periods[0].StartTime, currentTimeZone).tz(targetTimeZone);
			var shiftEndDateTime = moment.tz(periods[periods.length - 1].EndTime, currentTimeZone).tz(targetTimeZone);

			if (moment.duration(shiftEndDateTime.startOf('day') - shiftStartDateTime.startOf('day')).days() == 1) {
				shiftEndTime += '+1';
			}

			return shiftEndTime;
		}

		function buildPeriods(periods, currentTimeZone, targetTimeZone) {
			var result = [];

			periods.forEach(function(p) {
				result.push({
					Title: p.Title,
					TimeSpan: buildTimespan(p.StartTime, p.EndTime, currentTimeZone, targetTimeZone),
					Color: colorUtils.colorToRGB(p.Color),
					TextColor: colorUtils.getTextColorBasedOnBackgroundColor(p.Color),
					StartTime: moment
						.tz(p.StartTime, currentTimeZone)
						.tz(targetTimeZone)
						.format('YYYY-MM-DDTHH:mm:ss'),
					EndTime: moment
						.tz(p.EndTime, currentTimeZone)
						.tz(targetTimeZone)
						.format('YYYY-MM-DDTHH:mm:ss'),
					IsOvertime: p.IsOvertime,
					StartPositionPercentage: p.StartPositionPercentage,
					EndPositionPercentage: p.EndPositionPercentage,
					Meeting: p.Meeting,
					showMeetingIcon: p.ShowMeetingIcon
				});
			});

			return result;
		}

		function buildTimespan(startTime, endTime, currentTimeZone, targetTimeZone) {
			var timespan =
				svc.formatTime(startTime, currentTimeZone, targetTimeZone) +
				' - ' +
				svc.formatTime(endTime, currentTimeZone, targetTimeZone);

			return timespan;
		}

		function buildShiftCategoryVisualData(category) {
			if (category) {
				return {
					Id: category.Id,
					ShortName: category.ShortName,
					Name: category.Name,
					DisplayColor: category.DisplayColor,
					TextColor: colorUtils.getTextColorBasedOnBackgroundColor(category.DisplayColor)
				};
			}

			return null;
		}
	}
})();
