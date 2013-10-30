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

			this.LengthMinutes = unit.LengthMinutes;
			this.StartMinutes = unit.StartMinutes;
			this.EndMinutes = unit.EndMinutes;
			this.StartPixels = unit.StartPixels;
			this.LengthPixels = unit.LengthPixels;
			this.OverlapsTimeLine = unit.OverlapsTimeLine;

			this.TimeLineAffectingStartMinute = unit.StartMinutes;
			this.TimeLineAffectingEndMinute = unit.EndMinutes;

			this.StartTime = unit.StartTime;

			this.Color = function () { return data.Color; };
			this.Description = data.Description;
			this.IsFullDayAbsence = data.IsFullDayAbsence;
		};
	});
