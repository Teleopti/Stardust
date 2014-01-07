define([
		'knockout',
		'moment',
		'lazy',
		'views/personschedule/shift',
		'shared/dayoff'
], function (
		ko,
		moment,
		lazy,
		shift,
		dayOff
	) {

	return function (data) {
		var self = this;

		this.Id = data.Id;
		this.Name = ko.observable(data.FirstName ? data.FirstName + " " + data.LastName : "");

		this.WorkTimeMinutes = ko.observable(0);
		this.ContractTimeMinutes = ko.observable(0);

		this.DayOffs = ko.observableArray();
		this.Shifts = ko.observableArray();

		this.HasShift = ko.computed(function () {
			return self.Shifts().length > 0;
		});

		this.ContractTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
			return time.format("H:mm");
		});

		this.WorkTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
			return time.format("H:mm");
		});

		this.ClearData = function() {
			self.Shifts([]);
			self.DayOffs([]);
			self.ContractTimeMinutes(0);
			self.WorkTimeMinutes(0);
		};
		
		this.AddData = function (data, timeline) {
			
			if (data.Name)
				self.Name(data.Name);

			if (data.Projection && data.Projection.length > 0) {
				var newShift = new shift(data, timeline);
				newShift.AddLayers(data);
				// refact : this might be a wrong assumption
				if (newShift.Layers()[0].StartMinutes() > 0)
					self.Shifts.push(newShift);
			}
			
			if (data.DayOff) {
				data.DayOff.Offset = data.Offset;
				var newDayOff = new dayOff(timeline, data.DayOff);
				self.DayOffs.push(newDayOff);
			}
			
			self.ContractTimeMinutes(self.ContractTimeMinutes() + data.ContractTimeMinutes);
			self.WorkTimeMinutes(self.WorkTimeMinutes() + data.WorkTimeMinutes);
		};

		var layers = function () {
			return lazy(self.Shifts())
		            .map(function (x) { return x.Layers(); })
		            .flatten();
		};

		var visibleLayers = function () {
			return layers()
				.filter(function (x) { return x.OverlapsTimeLine(); });
		};

		var visibleDayOffs = function () {
			return lazy(self.DayOffs())
				.filter(function (x) { return x.OverlapsTimeLine(); });
		};

		this.TimeLineAffectingStartMinute = ko.computed(function () {
			return layers().map(function(x) { return x.TimeLineAffectingStartMinute(); }).min();
		});

		this.TimeLineAffectingEndMinute = ko.computed(function () {
			return layers().map(function(x) { return x.TimeLineAffectingEndMinute(); }).max();
		});

		this.OrderBy = function () {

			var visibleShiftLayers = visibleLayers().filter(function (x) { return !x.IsFullDayAbsence; });
			
			if (visibleShiftLayers.some())
				return visibleShiftLayers.map(function (x) { return x.StartMinutes(); }).min();
			
			var visibleFullDayAbsences = visibleLayers().filter(function (x) { return x.IsFullDayAbsence; });
			if (visibleFullDayAbsences.some())
				return 5000 + visibleFullDayAbsences.map(function (x) { return x.StartMinutes(); }).min();
			
			if (visibleDayOffs().some())
				return 10000;
			
			return 20000;
		};
	};
});
