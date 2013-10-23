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
			this.StartMinutes = unit.CutInsideDayStartMinutes;
			this.EndMinutes = unit.EndMinutes;
			this.StartPixels = unit.CutInsideDayStartPixels;
			this.LengthPixels = unit.CutInsideDayLengthPixels;
			this.OverlapsTimeLine = unit.OverlapsTimeLine;
			
			this.StartTime = unit.StartTime;

			this.Color = data.Color;
			this.IsFullDayAbsence = data.IsFullDayAbsence;
			
			this.FullDayAbsenceName = ko.computed(function() {
				if (data.Color == "#FFB6C1")
					return "Vacation";
				if (data.Color == "#000000")
					return "Illness";
			});
		};
	});
