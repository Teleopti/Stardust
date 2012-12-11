define([
		'knockout',
		'views/teamschedule/layer'
	], function (ko, layer) {

		return function (timeline, agentDay) {
			var self = this;
			this.Id = agentDay.Id;

			this.Name = ko.computed(function () { return agentDay.FirstName + ' ' + agentDay.LastName; });
			this.Layers = ko.observableArray();
			this.ContractTime = ko.observable(agentDay.ContractTimeMinutes);
			this.WorkTime = ko.observable(agentDay.ContractTimeMinutes);

			ko.utils.arrayForEach(agentDay.Projection, function (p) {
				self.Layers.push(new layer(timeline, p));
			});

			this.FirstStartMinute = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(self.Layers(), function (l) {
					var startMinutes = l.StartMinutes();
					if (!start)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});
				return start;
			});

			this.LastEndMinute = ko.computed(function () {
				var end = undefined;
				ko.utils.arrayForEach(self.Layers(), function (l) {
					var endMinutes = l.EndMinutes();
					if (!end)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return end;
			});
		};
	});
