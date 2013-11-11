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

	        var time = moment().startOf('day').add('minutes', minutes);
	        var formattedTime = time.format(resources.TimeFormatForMoment);

            this.Minutes = function() {
	            return minutes;
            };

            this.Time = formattedTime;

            this.Pixel = ko.computed(function() {
                var startMinutes = minutes - timeline.StartMinutes();
                var pixels = startMinutes * timeline.PixelsPerMinute();
                return Math.round(pixels);
            }).extend({throttle: 10});
        };
    });
