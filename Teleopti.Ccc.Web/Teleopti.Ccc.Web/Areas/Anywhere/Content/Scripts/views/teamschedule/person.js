define([
		'knockout',
		'moment',
		'lazy',
		'views/teamschedule/shift',
		'views/teamschedule/layer',
		'views/teamschedule/dayoff'
	], function(
		ko,
		moment,
		lazy,
		shift,
		layer,
		dayOff
	) {

		return function(data) {
			var self = this;

			this.Id = data.Id;
			this.Name = data.FirstName + ' ' + data.LastName;
			
			this.WorkTimeMinutes = ko.observable(0);
			this.ContractTimeMinutes = ko.observable(0);

			this.DayOffs = ko.observableArray();
			this.Shifts = ko.observableArray();

			this.IsShift = ko.computed(function() {
				return self.Shifts().length > 0;
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
				self.Shifts([]);
				self.DayOffs([]);
				self.WorkTimeMinutes(0);
				self.ContractTimeMinutes(0);
			};
			
			this.AddData = function (data, timeline) {
				if (data.Projection.length > 0) {
					var newShift = new shift(timeline);
					newShift.AddLayers(data);
					self.Shifts.push(newShift);
				}

				if (data.DayOff) {
					data.DayOff.Date = data.Date;
					var newDayOff = new dayOff(timeline, data.DayOff);
					self.DayOffs.push(newDayOff);
				}
				
				self.ContractTimeMinutes(self.ContractTimeMinutes() + data.ContractTimeMinutes);
				self.WorkTimeMinutes(self.WorkTimeMinutes() + data.WorkTimeMinutes);
			};

		    var layersSeq = function() {
		        return lazy(self.Shifts())
		            .map(function(x) { return x.Layers(); })
		            .flatten();
		    };

			this.TimeLineAffectingStartMinute = ko.computed(function() {
			    var start = undefined;
			    layersSeq().each(function (l) {
					var startMinutes = l.TimeLineAffectingStartMinute();
			        if (start === undefined)
			            start = startMinutes;
			        if (startMinutes < start)
			            start = startMinutes;
			    });
				return start;
			});

			this.TimeLineAffectingEndMinute = ko.computed(function () {
				var end = undefined;
				layersSeq().each(function (l) {
					var endMinutes = l.TimeLineAffectingEndMinute();
					if (end === undefined)
						end = endMinutes;
					if (endMinutes > end)
						end = endMinutes;
				});
				return end;
			});

			this.OrderBy = function () {

			    var visibleLayers = function () {
			        return layersSeq()
			            .select(function (x) { return x.OverlapsTimeLine(); })
			            .toArray();
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
