define([
		'knockout',
		'moment',
		'lazy',
		'resources',
		'shared/timeline-unit'
], function (
		ko,
		moment,
		lazy,
		resources,
		unitViewModel
	) {

	return function (timeline, data) {

		var self = this;
		
		var unit = new unitViewModel(timeline, data);

		this.StartMinutes = unit.CutInsideDayStartMinutes;
		this.EndMinutes = unit.EndMinutes;
		this.StartPixels = unit.CutInsideTimeLineStartPixels;
		this.LengthPixels = unit.CutInsideTimeLineLengthPixels;

		this.OverlapsTimeLine = unit.OverlapsTimeLine;

		this.DayOffName = data.DayOffName;

		this.HasOverlapingLayers = function (person) {
			var layers = lazy(person.Shifts())
				.map(function (x) { return x.Layers(); })
				.flatten();
			return layers.some(function(l) {
				return self.StartMinutes() < l.TimeLineAffectingEndMinute() && self.EndMinutes() > l.TimeLineAffectingStartMinute();
			});
		};
		
	};
});