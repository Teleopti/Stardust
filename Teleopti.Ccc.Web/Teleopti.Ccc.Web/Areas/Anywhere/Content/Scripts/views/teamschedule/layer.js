define([
        'knockout',
        'moment',
        'resources!r',
        'shared/timeline-unit'
    ], function(
        ko,
        moment,
        resources,
        unitViewModel
    ) {

        return function(timeline, data) {

            var unit = new unitViewModel(timeline, data);

            this.LengthMinutes = unit.LengthMinutes;
            this.StartMinutes = unit.StartMinutes;
            this.EndMinutes = unit.EndMinutes;
            this.StartPixels = unit.CutInsideDayStartPixels;
            this.LengthPixels = unit.CutInsideDayLengthPixels;
            this.OverlapsTimeLine = unit.OverlapsTimeLine;

            this.StartTime = unit.StartTime;

            this.Color = data.Color;
            this.Description = data.Description;
            this.IsFullDayAbsence = data.IsFullDayAbsence;

            this.TimeLineAffectingStartMinute = unit.CutInsideDayStartMinutes;
	        this.TimeLineAffectingEndMinute = unit.CutInsideDayEndMinutes;
        };
    });
