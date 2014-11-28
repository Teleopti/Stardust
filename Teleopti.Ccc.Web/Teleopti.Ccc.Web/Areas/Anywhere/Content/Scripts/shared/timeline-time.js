define([
	'knockout',
	'moment',
	'momentTimezoneData',
	'shared/timezone-current',
	'resources'
], function(
	ko,
	moment,
	momentTimeZoneData,
	timezoneCurrent,
	resources
) {

	return function(timeline, minutes, hideLabel, ianaTimeZoneOther, baseDate) {
		var time = ((baseDate == undefined)
			? moment.tz(timezoneCurrent.IanaTimeZone())
			: moment.tz(baseDate, timezoneCurrent.IanaTimeZone())).startOf('day').add('minutes', minutes);

		var formattedTime = time.format(resources.TimeFormatForMoment);

		this.Minutes = function() {
			return minutes;
		};

		var getTimeForOtherTimeZone = function() {
			if (timezoneCurrent.IanaTimeZone() && ianaTimeZoneOther) {
				var otherTime = time.clone().tz(ianaTimeZoneOther);
				return otherTime.format(resources.TimeFormatForMoment);
			}
			return "";
		};

		this.Time = hideLabel ? "" : formattedTime;
		this.TimeOtherTimeZone = hideLabel ? "" : getTimeForOtherTimeZone();

		this.Pixel = ko.computed(function() {
			var startMinutes = minutes - timeline.StartMinutes();
			var pixels = startMinutes * timeline.PixelsPerMinute();
			return Math.round(pixels);
		}).extend({ throttle: 10 });
	};
});
