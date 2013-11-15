define([
		'knockout',
		'moment',
		'resources!r',
		'shared/timeline-unit'
], function (
		ko,
		moment,
		resources,
		unitViewModel
	) {

	return function (timeline, data) {

		var unit = new unitViewModel(timeline, data);
				
		this.StartMinutes = unit.CutInsideDayStartMinutes;
		this.StartPixels = unit.CutInsideTimeLineStartPixels;
		this.LengthPixels = unit.CutInsideTimeLineLengthPixels;

		this.OverlapsTimeLine = unit.OverlapsTimeLine;

		this.DayOffName = data.DayOffName;
	};
});