define([
		'knockout',
		'moment'
	], function (ko, moment) {

		return function (timeline, projectionLayer) {

			var self = this;

			var localTime = moment(projectionLayer.Start, "YYYY-MM-DD hh:mm:ss Z").local();
			var layerStartMinutes = localTime.minutes() + localTime.hours() * 60;

			this.StartMinutes = ko.observable(layerStartMinutes);
			this.LengthMinutes = ko.observable(projectionLayer.Minutes);
			this.Color = ko.observable(projectionLayer.Color);

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
