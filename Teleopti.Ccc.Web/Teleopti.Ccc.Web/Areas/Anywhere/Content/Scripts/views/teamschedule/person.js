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

			this.DayOffs = ko.observableArray();
			
			this.IsShift = ko.computed(function() {
				return self.Layers().length > 0;
			});

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
				self.WorkTimeMinutes(0);
				self.ContractTimeMinutes(0);
			};
			
			this.AddData = function (data, timeline, date) {
				var layers = data.Projection;
				var newItems = ko.utils.arrayMap(layers, function (p) {
					p.Date = date;
					p.IsFullDayAbsence = data.IsFullDayAbsence;
				    console.log(p);
					return new layer(timeline, p);
				});
				self.Layers.push.apply(self.Layers, newItems);
				
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
				
				var visibleLayers = function() {
					return ko.utils.arrayFilter(self.Layers(), function(l) {
						return l.OverlapsTimeLine();
					});
				};

				var visibleFullDayAbsences = function() {
					return ko.utils.arrayFilter(visibleLayers(), function(l) {
						return l.IsFullDayAbsence;
					});
				};
				

				var visibleShiftLayers = function () {
					return ko.utils.arrayFilter(visibleLayers(), function (l) {
						return !l.IsFullDayAbsence;
					});
				};

				var visibleDayOffs = function() {
					return ko.utils.arrayFilter(self.DayOffs(), function (l) {
						return l.OverlapsTimeLine();
					});
				};
				
				var earliestMinute = function(layers) {
					var start = undefined;
					ko.utils.arrayForEach(layers, function (l) {
						var startMinutes = l.StartMinutes();
						if (start === undefined)
							start = startMinutes;
						if (startMinutes < start)
							start = startMinutes;
					});
					return start;
				};

				//var sortAsShift = sortera på tiden på alla lager som inte är full day absence som syns
				var layers = visibleShiftLayers();
				if (layers.length > 0)
					return earliestMinute(layers) || 0;
				
				//var sortedAsFullDayAbsence = sortera på tiden på alla lager som är full day absence som syns
				layers = visibleFullDayAbsences();
				if (layers.length > 0)
					return 5000 + (earliestMinute(layers) || 0);
				
				// var sortedAsDayoff = sortera på tiden på alla day offs som syns om det inte finns lager
				// include only visible day offs?
				var dayOffs = visibleDayOffs();
				if (dayOffs.length > 0)
					return 10000;

				// var sortedAsNothing = inga synliga lager eller synliga day offs
				return 20000;
			};
		};
	});
