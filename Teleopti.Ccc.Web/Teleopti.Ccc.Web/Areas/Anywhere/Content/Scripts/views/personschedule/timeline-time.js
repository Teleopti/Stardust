define([
		'knockout',
		'moment'
	], function(ko, moment) {

		return function(timeline, minutes, shortTimePattern) {

			var self = this;

			this.Minutes = ko.observable(minutes);

			this.Time = ko.computed(function() {
				var time = moment().startOf('day').add('minutes', self.Minutes());
				return time.format(shortTimePattern);
			});

			this.Pixel = ko.computed(function() {
				var startMinutes = self.Minutes() - timeline.StartMinutes();
				var pixels = startMinutes * timeline.PixelsPerMinute();
				return Math.round(pixels);
			});

		};

	});
