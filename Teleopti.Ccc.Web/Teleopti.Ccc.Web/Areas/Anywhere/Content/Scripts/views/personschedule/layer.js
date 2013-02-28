define([
		'knockout'
	], function (ko) {

		return function (timeline, start, length, color) {

			var self = this;

			this.StartMinutes = ko.observable(start);
			this.LengthMinutes = ko.observable(length);
			this.Color = ko.observable(color);

			this.EndMinutes = ko.computed(function () {
				return self.StartMinutes() + self.LengthMinutes();
			});

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
