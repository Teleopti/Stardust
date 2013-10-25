define([
		'knockout',
		'moment',
		'views/teamschedule/layer',
		'views/teamschedule/dayoff',
		'resources!r'
	], function(
		ko,
		moment,
		layer,
		dayOff,
		resources
	) {

		return function(data, events) {
			var self = this;

			this.Id = data.Id;
			this.Name = data.FirstName + ' ' + data.LastName;
			
			this.Layers = ko.observableArray();
			this.WorkTimeMinutes = ko.observable(0);
			this.ContractTimeMinutes = ko.observable(0);

			this.DayOffs = ko.observableArray();
			
			this.IsShift = ko.computed(function() {
				return self.Layers().length > 0;
			});

			this.IsFullDayAbsence = false;
			
			this.ContractTime = ko.computed(function() {
				var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
				return time.format("H:mm");
			});

			this.WorkTime = ko.computed(function() {
				var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
				return time.format("H:mm");
			});

			this.ClearData = function() {
				self.Layers([]);
				self.DayOffs([]);
				self.IsFullDayAbsence = false;
				self.WorkTimeMinutes(0);
				self.ContractTimeMinutes(0);
			};
			
			this.AddData = function (data, timeline, date) {
				var layers = data.Projection;
				var newItems = ko.utils.arrayMap(layers, function (p) {
					p.Date = date;
					return new layer(timeline, p);
				});
				self.Layers.push.apply(self.Layers, newItems);

				self.IsFullDayAbsence = data.IsFullDayAbsence;
				
				if (data.DayOff) {
					data.DayOff.Date = date;
					self.DayOffs.push.apply(self.DayOffs, [new dayOff(timeline, data.DayOff)]);
				}
				
				self.ContractTimeMinutes(self.ContractTimeMinutes() + data.ContractTimeMinutes);
				self.WorkTimeMinutes(self.WorkTimeMinutes() + data.WorkTimeMinutes);
			};
			
			this.TimeLineAffectingStartMinute = ko.computed(function() {
				var start = undefined;
				ko.utils.arrayForEach(self.Layers(), function(l) {
					var startMinutes = l.StartMinutes();
					if (start === undefined)
						start = startMinutes;
					if (startMinutes < start)
						start = startMinutes;
				});
				return start;
			});

			this.TimeLineAffectingEndMinute = ko.computed(function() {
				var end = undefined;
				ko.utils.arrayForEach(self.Layers(), function(l) {
					var endMinutes = l.EndMinutes();
					if (end === undefined)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return end;
			});

			this.Select = function() {
				events.notifySubscribers(self.Id, "gotoperson");
			};

			this.OrderBy = function () {
				var value = 0;
				value += self.TimeLineAffectingStartMinute() || 0;
				value += self.IsFullDayAbsence ? 5000 : 0;
				value += self.Layers().length == 0 && self.DayOffs().length > 0 ? 10000 : 0;
				var noShift = self.Layers().length == 0 && self.DayOffs().length == 0;
				value += noShift ? 20000 : 0;
				return value;
			};
		};
	});
