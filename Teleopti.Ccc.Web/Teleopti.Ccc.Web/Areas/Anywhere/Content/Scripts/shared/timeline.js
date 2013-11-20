define([
		'knockout',
		'helpers',
		'shared/timeline-time'
	], function (ko, helpers, timeViewModel) {

		var minutes = helpers.Minutes;

		return function (timeLineLayers) {
			var self = this;

			this.WidthPixels = ko.observable();

			this.StartMinutes = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(timeLineLayers(), function (l) {
					var startMinutes = l.TimeLineAffectingStartMinute();
					if (start === undefined)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});

				if (start === undefined)
					return minutes.ForHourOfDay(8);
				
				return minutes.StartOfHour(start);
			}).extend({ throttle: 10 });

			this.EndMinutes = ko.computed(function () {
				var end = undefined;
				ko.utils.arrayForEach(timeLineLayers(), function (l) {
					var endMinutes = l.TimeLineAffectingEndMinute();
					if (end === undefined)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});

				if (end === undefined)
					return minutes.ForHourOfDay(16);
				
				return minutes.EndOfHour(end);
			});

			this.Minutes = ko.computed(function () {
				return self.EndMinutes() - self.StartMinutes();
			}).extend({ throttle: 10 });

			this.PixelsPerMinute = ko.computed(function () {
				return self.WidthPixels() / self.Minutes();
			});

			this.Times = ko.computed(function () {
				var times = [];
				var time = self.StartMinutes();
				var end = self.EndMinutes();
				while (time < end + 1) {
					times.push(new timeViewModel(self, time));
					time = minutes.AddHours(time, 1);
				}
				return times;
			}).extend({ throttle: 10 });

			this.StartTime = ko.computed(function () {
				var times = self.Times();
				if (times.length > 0)
					return self.Times()[0].Time;
				return "";
			});

			this.EndTime = ko.computed(function () {
				var times = self.Times();
				if (times.length > 0)
					return times[times.length - 1].Time;
				return "";
			});
		};
	});