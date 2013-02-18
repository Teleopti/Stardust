define([
		'knockout',
		'helpers',
		'views/teamschedule/timeline-time'
	], function (ko, helpers, timeViewModel) {

		var minutes = helpers.Minutes;

		return function (agents, shortTimePattern) {

			var self = this;

			this.Agents = agents;
			this.WidthPixels = ko.observable();
			this.Times = ko.observableArray();

			this.StartMinutes = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(self.Agents.Agents(), function (l) {
					var startMinutes = l.FirstStartMinute();
					if (start === undefined)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});
				return minutes.StartOfHour(start);
			});

			this.EndMinutes = ko.computed(function () {
				var end = undefined;
				ko.utils.arrayForEach(self.Agents.Agents(), function (l) {
					var endMinutes = l.LastEndMinute();
					if (end === undefined)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return minutes.EndOfHour(end);
			});

			this.CalculateTimes = function () {
				self.Times([]);
				var times = self.Times();
				var time = self.StartMinutes();
				var end = self.EndMinutes();
				while (time < end + 1) {
					times.push(new timeViewModel(self, time, shortTimePattern));
					time = minutes.AddHours(time, 1);
				}
				self.Times.valueHasMutated();
			};

			this.Minutes = ko.computed(function () {
				return self.EndMinutes() - self.StartMinutes();
			});

			this.PixelsPerMinute = ko.computed(function () {
				return self.WidthPixels() / self.Minutes();
			});
		};
	});