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

		    var self = this;
		    
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

			this.ColorIsDark = function () {
			    var hex = self.Color.substr(1, 6);
			    var r = parseInt(hex.substr(0 * 2, 2), 16);
			    var g = parseInt(hex.substr(1 * 2, 2), 16);
			    var b = parseInt(hex.substr(2 * 2, 2), 16);
		        var brightness = r * 0.299 + g * 0.587 + b * 0.114;
		        return brightness < 100;
		    };
		    
		};
	});
