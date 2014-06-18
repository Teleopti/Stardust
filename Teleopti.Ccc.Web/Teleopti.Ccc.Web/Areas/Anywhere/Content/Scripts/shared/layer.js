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

		var unit = new unitViewModel(timeline, data);

		this.LengthMinutes = unit.LengthMinutes;
		this.StartMinutes = unit.StartMinutes;
		this.EndMinutes = unit.EndMinutes;
		this.StartPixels = unit.CutInsideDayStartPixels;
		this.LengthPixels = unit.CutInsideDayLengthPixels;
		this.OverlapsTimeLine = unit.OverlapsTimeLine;

		this.StartTime = unit.StartTime;
		this.EndTime = unit.EndTime;

		this.Color = data.Color;
		if(this.Color)
			this.TextColor = helpers.TextColor.BasedOnBackgroundColor(helpers.TextColor.HexToRgb(self.Color));
		this.Description = data.Description;
		this.IsFullDayAbsence = data.IsFullDayAbsence;

		this.TimeLineAffecting = function () { return affectTimeLine; }
		this.TimeLineAffectingStartMinute = unit.CutInsideDayStartMinutes;
		this.TimeLineAffectingEndMinute = unit.EndMinutes;

		this.Selected = ko.observable(false);
	};
});
