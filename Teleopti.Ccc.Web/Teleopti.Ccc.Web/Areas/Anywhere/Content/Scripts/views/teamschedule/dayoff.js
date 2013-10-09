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

		var self = this;

		var startTime = moment(data.Start, resources.FixedDateTimeFormatForMoment);
		var dayOffStartMinutes = startTime.diff(data.Date, 'minutes');

		console.log(data.Date);
		console.log(data.Start);
		
		console.log(startTime);
		console.log(dayOffStartMinutes);

		this.StartMinutes = ko.observable(dayOffStartMinutes);
		this.LengthMinutes = ko.observable(data.Minutes);

		this.StartPixels = ko.computed(function () {
			var startMinutes = self.StartMinutes() - timeline.StartMinutes();
			var pixels = startMinutes * timeline.PixelsPerMinute();
			return Math.round(pixels);
		});

		this.LengthPixels = ko.computed(function () {
			var lengthMinutes = self.LengthMinutes();
			var pixels = lengthMinutes * timeline.PixelsPerMinute();
			return Math.round(pixels);
		});

	};
});
