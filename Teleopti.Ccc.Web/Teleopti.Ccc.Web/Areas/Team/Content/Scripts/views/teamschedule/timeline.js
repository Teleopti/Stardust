define([
		'knockout',
		'helpers',
		'views/teamschedule/timeline-time'
	], function (ko, helpers, timeViewModel) {

		var minutes = helpers.Minutes;

		return function (shortTimePattern) {

			var self = this;

			this.WidthPixels = ko.observable();
			this.Agents = ko.observableArray();
			
			this.AddAgents = function (agents) {
				self.Agents.push.apply(self.Agents, agents);
			};

			this.StartMinutes = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(self.Agents(), function (l) {
					var startMinutes = l.FirstStartMinute();
					if (!start)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});
				return minutes.StartOfHour(start);
			});

			this.EndMinutes = ko.computed(function () {
				var end = undefined;
				ko.utils.arrayForEach(self.Agents(), function (l) {
					var endMinutes = l.LastEndMinute();
					if (!end)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return minutes.EndOfHour(end);
			});

			this.Times = ko.computed(function () {
				var times = new Array();
				var time = self.StartMinutes();
				var end = self.EndMinutes();
				while (time < end + 1) {
					times.push(new timeViewModel(self, time, shortTimePattern));
					time = minutes.AddHours(time, 1);
				}
				return times;
			});

			this.Minutes = ko.computed(function () {
				return self.EndMinutes() - self.StartMinutes();
			});

			this.PixelsPerMinute = ko.computed(function () {
				return self.WidthPixels() / self.Minutes();
			});
		};
	});