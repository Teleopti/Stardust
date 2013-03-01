define([
		'knockout'
	], function (ko) {

		return function (timeline, data) {

			var self = this;

			var localTime = moment(data.Start, "YYYY-MM-DD hh:mm:ss Z").local();
			var localStartMinutes = localTime.diff(data.Date, 'minutes');

			this.StartMinutes = ko.observable(localStartMinutes);
			this.LengthMinutes = ko.observable(data.Minutes);
			this.Color = ko.observable(data.Color);

			this.EndMinutes = ko.computed(function () {
				return self.StartMinutes() + self.LengthMinutes();
			});

			this.TimeLineAffectingStartMinute = this.StartMinutes;
			this.TimeLineAffectingEndMinute = this.EndMinutes;

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
