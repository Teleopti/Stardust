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
			this.Name = ko.observable(data.FirstName + ' ' + data.LastName);

			this.Layers = ko.observableArray();
			this.WorkTimeMinutes = ko.observable(0);
			this.ContractTimeMinutes = ko.observable(0);

			this.DayOff = ko.observable(null);
			
			this.IsDayOff = ko.computed(function() {
				return self.DayOff() != null;
			});

			this.IsShift = ko.computed(function() {
				return !self.IsDayOff();
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
				self.IsFullDayAbsence = false;
				self.DayOff(null);
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
					self.DayOff(new dayOff(timeline, data.DayOff));
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

			this.CompareTo = function(other) {

				var first = self;
				var second = other;
				
				/*
				return a negative value if the first argument is smaller, 
				a positive value is the second is smaller, 
				or zero to treat them as equal.
				*/

				var firstNoShift = first.Layers().length == 0 && !first.IsDayOff();
				var secondNoShift = second.Layers().length == 0 && !second.IsDayOff();
				if (firstNoShift && second.IsDayOff())
					return 1;
				if (secondNoShift && first.IsDayOff())
					return -1;

				if (first.IsDayOff() && second.IsFullDayAbsence)
					return 1;
				if (second.IsDayOff() && first.IsFullDayAbsence)
					return -1;

				if (first.IsFullDayAbsence && !second.IsFullDayAbsence)
					return 1;
				if (second.IsFullDayAbsence && !first.IsFullDayAbsence)
					return -1;

				var firstStartMinutes = first.TimeLineAffectingStartMinute();
				var secondStartMinutes = second.TimeLineAffectingStartMinute();
				if (firstStartMinutes > secondStartMinutes)
					return 1;
				if (firstStartMinutes < secondStartMinutes)
					return -1;

				var firstEndMinutes = first.TimeLineAffectingEndMinute();
				var secondEndMinutes = second.TimeLineAffectingEndMinute();
				if (firstEndMinutes > secondEndMinutes)
					return 1;
				if (firstEndMinutes < secondEndMinutes)
					return -1;

				return 0;
			};
		};
	});
