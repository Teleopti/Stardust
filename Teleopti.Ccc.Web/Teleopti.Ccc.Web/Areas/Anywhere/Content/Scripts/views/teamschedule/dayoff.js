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
		
		this.StartPixels = unit.CutInsideTimeLineStartPixels;
		this.LengthPixels = unit.CutInsideTimeLineLengthPixels;

		this.OverlapsTimeLine = unit.OverlapsTimeLine;

		this.DayOffName = ko.computed(function () {
		    return "Day off";
		});
	    
	};
});
