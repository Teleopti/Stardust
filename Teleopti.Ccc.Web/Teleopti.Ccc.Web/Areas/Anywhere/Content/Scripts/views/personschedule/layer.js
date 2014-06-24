define([
	'knockout',
	'moment',
	'lazy',
	'resources',
	'shared/timeline-unit',
	'helpers'
], function (
	ko,
	moment,
	lazy,
	resources,
		unitViewModel,
		helpers
	) {

	return function (timeline, data, affectTimeLine) {
		var self = this;

		this.unit = new unitViewModel(timeline, data);
		this.LengthMinutes;
		this.StartMinutes;
		this.EndMinutes;
		this.StartPixels = ko.observable();
		this.LengthPixels = ko.observable();
		this.OverlapsTimeLine;

		this.StartTime;
		this.EndTime;
		this.Data = data;

		this.UpdateUnit = function (someUpdatedData) {
			Object.keys(someUpdatedData).forEach(function(key) {
				self.Data[key] = someUpdatedData[key];
			});
			
			self.unit = new unitViewModel(timeline, self.Data);
			self.LengthMinutes = self.unit.LengthMinutes;
			self.StartMinutes = self.unit.StartMinutes;
			self.EndMinutes = self.unit.EndMinutes;
			self.StartPixels(self.unit.CutInsideDayStartPixels());
			console.log(self.StartPixels());
			self.LengthPixels(self.unit.CutInsideDayLengthPixels());
			self.OverlapsTimeLine = self.unit.OverlapsTimeLine;

			self.StartTime = self.unit.StartTime;
			self.EndTime = self.unit.EndTime;
			self.TimeLineAffectingStartMinute = self.unit.CutInsideDayStartMinutes;
			self.TimeLineAffectingEndMinute = self.unit.EndMinutes;
		};

		this.UpdateUnit(data);

		this.Selected = ko.observable();
		this.Active = ko.computed(function () {
			return self.Selected() ? 'active' : '';
		});

		this.Color = data.Color;
		if(this.Color)
			this.TextColor = helpers.TextColor.BasedOnBackgroundColor(helpers.TextColor.HexToRgb(self.Color));
		this.Description = data.Description;
		this.IsFullDayAbsence = data.IsFullDayAbsence;

		this.TimeLineAffecting = function () { return affectTimeLine; }
		this.TimeLineAffectingStartMinute;
		this.TimeLineAffectingEndMinute;

		this.ActivityId = data.ActivityId;
	};
});
