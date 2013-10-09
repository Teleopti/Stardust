define([
		'knockout',
		'moment',
		'resources!r'
], function (
		ko,
		moment,
		resources
	) {

	return function (timeline, data) {

		var startTime = moment(data.Start, resources.FixedDateTimeFormatForMoment);
		var dayOffStartMinutes = startTime.diff(data.Date, 'minutes');

		var startMinutes = ko.observable(dayOffStartMinutes);
		var lengthMinutes = ko.observable(data.Minutes);

		var displayStartMinute = ko.computed(function() {
			if (startMinutes() < timeline.StartMinutes())
				return timeline.StartMinutes();
			return startMinutes();
		});

		var displayLength = ko.computed(function () {
			if (timeline.EndMinutes() < startMinutes() + lengthMinutes())
				return timeline.EndMinutes() - displayStartMinute();
			return lengthMinutes();
		});

		this.StartPixels = ko.computed(function () {
			var start = displayStartMinute() - timeline.StartMinutes();
			var pixels = start * timeline.PixelsPerMinute();
			return Math.round(pixels);
		});

		this.LengthPixels = ko.computed(function () {
			var pixels = displayLength() * timeline.PixelsPerMinute();
			return Math.round(pixels);
		});

		this.InTimeLine = ko.computed(function () {
			return displayLength() > 0;
		});
	};
});
