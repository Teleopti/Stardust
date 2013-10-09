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
			var layerStartMinutes = startTime.diff(data.Date, 'minutes');

			this.InternalStartMinutes = layerStartMinutes;
			this.LengthMinutes = data.Minutes;
			this.Color = data.Color;

			this.StartMinutes = function () {
				if (self.InternalStartMinutes < 0) {
					self.LengthMinutes = data.Minutes + self.InternalStartMinutes;
					return 0;
				}
				return self.InternalStartMinutes;
			};

			this.StartTime = ko.computed(function () {
				var time = moment().startOf('day').add('minutes', self.StartMinutes());
				return time.format(resources.TimeFormatForMoment);
			});

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
