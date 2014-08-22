define([
        'knockout',
        'moment',
        'resources'
    ], function(
        ko,
        moment,
        resources
    ) {

    	return function (timeline, minutes, hideLabel, ianaTimeZoneLoggedOnUser, ianaTimeZoneOther) {

        	var time = moment().startOf('day').add('minutes', minutes);
	        var formattedTime = time.format(resources.TimeFormatForMoment);

            this.Minutes = function() {
	            return minutes;
            };

		    var getTimeForOtherTimeZone = function() {
		    	if (ianaTimeZoneLoggedOnUser && ianaTimeZoneOther) {
		    		var userTime = time.tz(ianaTimeZoneLoggedOnUser);
				    var otherTime = userTime.tz(ianaTimeZoneOther);
				    return otherTime.format(resources.TimeFormatForMoment);
		    	}
			    return "";
		    };

            this.Time = hideLabel ? "" : formattedTime;
            this.TimeOtherTimeZone = hideLabel ? "" : getTimeForOtherTimeZone();

            this.Pixel = ko.computed(function() {
                var startMinutes = minutes - timeline.StartMinutes();
                var pixels = startMinutes * timeline.PixelsPerMinute();
                return Math.round(pixels);
            }).extend({throttle: 10});
        };
    });
