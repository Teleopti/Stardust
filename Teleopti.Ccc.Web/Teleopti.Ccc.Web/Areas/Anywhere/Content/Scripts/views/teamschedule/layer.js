define([
        'knockout',
        'moment',
        'lazy',
        'resources!r',
        'shared/timeline-unit'
    ], function(
        ko,
        moment,
        lazy,
        resources,
        unitViewModel
    ) {

        return function(timeline, data, shift) {

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
            this.Description = data.Description;
            this.IsFullDayAbsence = data.IsFullDayAbsence;
            
            this.TimeLineAffectingStartMinute = unit.CutInsideDayStartMinutes;
            this.TimeLineAffectingEndMinute = unit.CutInsideDayEndMinutes;

            this.DisplayDrop = ko.computed(function () {
                if (self.LengthPixels() > 30)
                    return false;
                return shift.AnyLayerSelected();
            });
            
            this.Selected = ko.observable(false);

        };
    });
