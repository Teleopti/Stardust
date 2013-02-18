define([
		'knockout',
		'moment'
	], function (ko, moment) {

		return function (timeline, projectionLayer, date) {

			var self = this;

			var localTime = moment(projectionLayer.Start, "YYYY-MM-DD hh:mm:ss Z").local();
			var layerStartMinutes = localTime.diff(date, 'minutes');


			this.InternalStartMinutes = layerStartMinutes;
			this.LengthMinutes = projectionLayer.Minutes;
			this.Color = projectionLayer.Color;

			this.StartMinutes = function () {
				return self.InternalStartMinutes < 0 ? 0 : self.InternalStartMinutes;
			};

			this.EndMinutes = ko.computed(function () {
				return self.InternalStartMinutes + self.LengthMinutes;
			});

			this.StartPixels = ko.computed(function () {
				var startMinutes = self.StartMinutes() - timeline.StartMinutes();
				var pixels = startMinutes * timeline.PixelsPerMinute();
				return Math.round(pixels);
			});

			this.LengthPixels = ko.computed(function () {
				var lengthMinutes = self.LengthMinutes;
				var pixels = lengthMinutes * timeline.PixelsPerMinute();
				return Math.round(pixels);
			});

			this.IsVisible = ko.computed(function () {
				return self.EndMinutes() >= 0;
			});
		};
	});
