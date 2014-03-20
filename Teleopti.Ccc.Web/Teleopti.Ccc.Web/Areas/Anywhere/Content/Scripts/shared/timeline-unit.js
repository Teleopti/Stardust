define([
		'knockout',
		'moment',
		'resources'
], function (
	ko,
	moment,
	resources
	) {

	return function (timeline, data) {

		var self = this;

		var startTime = moment(data.Start, resources.FixedDateTimeFormatForMoment);
		var startTimeMinutes = startTime.diff(data.Offset, 'minutes');
		var lengthMinutes = data.Minutes;
		
		var startMinutesToPixels = function (minutes) {
			var start = minutes - timeline.StartMinutes();
			var pixels = start * timeline.PixelsPerMinute();
			return Math.round(pixels);
		};

		var lengthMinutesToPixels = function (minutes) {
			var pixels = minutes * timeline.PixelsPerMinute();
			return Math.round(pixels);
		};

		this.StartMinutes = function() {
			return startTimeMinutes;
		};

		this.LengthMinutes = function() {
			return lengthMinutes;
		};

		this.EndMinutes = function () {
			return startTimeMinutes + lengthMinutes;
		};

		this.StartPixels = ko.computed(function () {
			return startMinutesToPixels(self.StartMinutes());
		});

		this.LengthPixels = ko.computed(function () {
			return lengthMinutesToPixels(self.LengthMinutes());
		});



		this.CutInsideTimeLineStartMinutes = ko.computed(function () {
			if (self.StartMinutes() < timeline.StartMinutes())
				return timeline.StartMinutes();
			return self.StartMinutes();
		});

		this.CutInsideTimeLineLengthMinutes = ko.computed(function () {
			if (timeline.EndMinutes() < self.StartMinutes() + self.LengthMinutes())
				return timeline.EndMinutes() - self.CutInsideTimeLineStartMinutes();
			if (self.StartMinutes() < timeline.StartMinutes())
				return self.EndMinutes() - timeline.StartMinutes();
			return self.LengthMinutes();
		});

		this.CutInsideTimeLineStartPixels = ko.computed(function () {
			return startMinutesToPixels(self.CutInsideTimeLineStartMinutes());
		});

		this.CutInsideTimeLineLengthPixels = ko.computed(function () {
			return lengthMinutesToPixels(self.CutInsideTimeLineLengthMinutes());
		});



		this.CutInsideDayStartMinutes = ko.computed(function () {
			if (self.StartMinutes() < 0)
				return 0;
			return self.StartMinutes();
		});

		this.CutInsideDayLengthMinutes = ko.computed(function () {
			if (self.StartMinutes() < 0)
				return self.LengthMinutes() - (self.StartMinutes() * -1);
			return self.LengthMinutes();
		});

		this.CutInsideDayStartPixels = ko.computed(function () {
			return startMinutesToPixels(self.CutInsideDayStartMinutes());
		});

		this.CutInsideDayLengthPixels = ko.computed(function () {
			return lengthMinutesToPixels(self.CutInsideDayLengthMinutes());
		});



		this.OverlapsTimeLine = ko.computed(function () {
			return self.CutInsideTimeLineLengthMinutes() > 0;
		});
		


		this.StartTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.StartMinutes());
			return time.format(resources.TimeFormatForMoment);
		});

		this.EndTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.EndMinutes());
			return time.format(resources.TimeFormatForMoment);
		});
	};
});
