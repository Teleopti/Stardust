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

        return function(timeline, data, shift) {

            var self = this;
            
            var unit = new unitViewModel(timeline, data);

            this.LengthMinutes = unit.LengthMinutes;
            this.StartMinutes = unit.CutInsideDayStartMinutes;
            this.EndMinutes = unit.EndMinutes;
            this.StartPixels = unit.CutInsideDayStartPixels;
            this.LengthPixels = unit.CutInsideDayLengthPixels;
            this.OverlapsTimeLine = unit.OverlapsTimeLine;

            this.StartTime = unit.StartTime;
            this.EndTime = unit.EndTime;

            this.Color = data.Color;
            this.Description = data.Description;
            this.IsFullDayAbsence = data.IsFullDayAbsence;

            this.Selected = ko.observable(false);

            this.Select = function () {
                if (shift.Selected()) {
                    shift.Selected(false);
                    if (self.Selected()) {
                        self.Selected(false);
                    } else {
                        self.Selected(true);
                    }
                }
                else
                    shift.Selected(true);
            };
            
            this.DeleteLayer = function() {

            };
            
            this.MoveLayer = function () {

            };
            
        };
    });
