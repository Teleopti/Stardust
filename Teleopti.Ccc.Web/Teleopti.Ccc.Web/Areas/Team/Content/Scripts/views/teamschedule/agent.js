define([
		'knockout',
		'moment'
	], function (
		ko,
		moment
		) {

		return function (agentDay) {
			var self = this;
			this.Id = agentDay.Id;

			this.Name = ko.computed(function () { return agentDay.FirstName + ' ' + agentDay.LastName; });
			this.Layers = ko.observableArray(agentDay.Projection);
			this.ContractTime = ko.observable(agentDay.ContractTimeMinutes);
			this.WorkTime = ko.observable(agentDay.ContractTimeMinutes);

			this.FirstStartMinute = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(self.Layers(), function (l) {
					var localTime = moment(l.Start, "YYYY-MM-DD hh:mm:ss Z").local();
					var startMinutes = localTime.minutes() + localTime.hours() * 60;
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
					var localTime = moment(l.End, "YYYY-MM-DD hh:mm:ss Z").local();
					var endMinutes = localTime.minutes() + localTime.hours() * 60;
					if (!end)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return end;
			});
		};
	});
