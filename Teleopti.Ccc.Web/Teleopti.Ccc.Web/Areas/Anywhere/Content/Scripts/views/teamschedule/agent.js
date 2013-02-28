define([
		'knockout',
		'moment',
		'views/teamschedule/layer'
	], function (ko, moment, layer) {

		return function (data, events) {
			var self = this;

			this.Id = data.Id;
			this.Name = ko.observable(data.FirstName + ' ' + data.LastName);

			this.Layers = ko.observableArray();
			this.WorkTimeMinutes = ko.observable(0);
			this.ContractTimeMinutes = ko.observable(0);

			this.ContractTime = ko.computed(function () {
				var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
				return time.format("H:mm");
			});

			this.WorkTime = ko.computed(function () {
				var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
				return time.format("H:mm");
			});

			this.ClearLayers = function () {
				self.Layers([]);
			};

			this.AddLayers = function (layers, timeline, date) {
				var newItems = ko.utils.arrayMap(layers, function (p) {
					return new layer(timeline, p, date);
				});
				self.Layers.push.apply(self.Layers, newItems);
			};

			this.AddContractTime = function (minutes) {
				self.ContractTimeMinutes(self.ContractTimeMinutes() + minutes);
			};

			this.AddWorkTime = function (minutes) {
				self.WorkTimeMinutes(self.WorkTimeMinutes() + minutes);
			};

			this.TimeLineAffectingStartMinute = ko.computed(function () {
				var start = undefined;
				ko.utils.arrayForEach(self.Layers(), function (l) {
					var startMinutes = l.StartMinutes();
					if (start === undefined)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});
				return start;
			});

			this.TimeLineAffectingEndMinute = ko.computed(function () {
				var end = undefined;
				ko.utils.arrayForEach(self.Layers(), function (l) {
					var endMinutes = l.EndMinutes();
					if (end === undefined)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return end;
			});

			this.Select = function() {
				events.notifySubscribers(self.Id, "gotoagent");
			};
		};
	});
