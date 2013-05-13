define([
        'knockout',
        'moment',
        'resources!r'
    ], function(
        ko,
        moment,
        resources
    ) {

        return function(timeline, minutes) {

            var self = this;

            this.Minutes = ko.observable(minutes);

            this.Time = ko.computed(function() {
                var time = moment().startOf('day').add('minutes', self.Minutes());
                return time.format(resources.TimeFormatForMoment);
            });

            this.Pixel = ko.computed(function() {
                var startMinutes = self.Minutes() - timeline.StartMinutes();
                var pixels = startMinutes * timeline.PixelsPerMinute();
                return Math.round(pixels);
            });

        };

    });
