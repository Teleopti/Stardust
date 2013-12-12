define([
		'knockout',
		'moment',
		'lazy',
		'views/teamschedule/shift',
		'views/teamschedule/schedule-menu',
		'shared/dayoff'
], function (
		ko,
		moment,
		lazy,
		shift,
		scheduleMenu,
		dayOff
	) {

	return function (data) {
		var self = this;

		this.Id = data.Id;
		this.Name = data.FirstName + ' ' + data.LastName;

		this.WorkTimeMinutes = ko.observable(0);
		this.ContractTimeMinutes = ko.observable(0);

		this.DayOffs = ko.observableArray();
		this.Shifts = ko.observableArray();

		this.IsShift = ko.computed(function () {
			return self.Shifts().length > 0;
		});

		this.ScheduleMenu = new scheduleMenu(data.Id, data.GroupId, data.Date);
		
		this.Selected = ko.observable(false);

		this.IsPersonOrShiftSelected = ko.computed(function() {
			if (self.Selected())
				return true;
			return $(self.Shifts()).is(function(index) {
				return this.IsAnyLayerSelected();
			});
		});
		
		this.ContractTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
			return time.format("H:mm");
		});

		this.WorkTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
			return time.format("H:mm");
		});

		this.ClearData = function () {
			self.Shifts([]);
			self.DayOffs([]);
			self.WorkTimeMinutes(0);
			self.ContractTimeMinutes(0);
		};

		this.AddData = function (shiftData, timeline, selectedGroup) {
			if (shiftData.Projection.length > 0) {
				var newShift = new shift(timeline, selectedGroup, self.Id, shiftData.Date);
				newShift.AddLayers(shiftData);
				// this might be a wrong assumption
				if (newShift.Layers()[0].StartMinutes() < 0) { 
					newShift.ShiftMenu.Date(shiftData.Date.clone().subtract('days', 1));
				}
				self.Shifts.push(newShift);
			}

			if (shiftData.DayOff) {
				shiftData.DayOff.Date = shiftData.Date;
				var newDayOff = new dayOff(timeline, shiftData.DayOff);
				self.DayOffs.push(newDayOff);
			}

			self.ContractTimeMinutes(self.ContractTimeMinutes() + shiftData.ContractTimeMinutes);
			self.WorkTimeMinutes(self.WorkTimeMinutes() + shiftData.WorkTimeMinutes);
		};

		var layers = function () {
			return lazy(self.Shifts())
		            .map(function (x) { return x.Layers(); })
		            .flatten();
		};

		var visibleLayers = function () {
			return layers()
				.select(function(x) { return x.OverlapsTimeLine(); });
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
