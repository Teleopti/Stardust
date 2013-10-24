define(
    [
        'moment',
        'resources!r'
    ], function (
        momentX,
        resources
    ) {

        return {
            Minutes: {
            	FromHours: function (hours) {
                    return hours * 60;
                },

                ForHourOfDay: function (hour) {
                	return hour * 60;
                },

                StartOfHour: function(minutes) {
                    return minutes - (minutes % 60);
                },

                EndOfHour: function (minutes) {
	                var reminder = (minutes % 60);
	                if (reminder == 0)
                        return minutes;
                    return minutes + (60 - reminder);
                },

                AddHours: function(minutes, hours) {
                    return minutes + 60 * hours;
                }
            },
            Async: {
                RunAndSetInterval: function(code, interval) {
                    code();
                    return setInterval(code, interval);
                }
            },
            Date: {
                ToServer: function (date) {
                    if (moment.isMoment(date))
                        return date.format(resources.FixedDateFormatForMoment);
                    if (date.getMonth)
                        return date.toString(resources.FixedDateFormat);
                    return date;
                },
                ToMoment: function (date) {
                    if (moment.isMoment(date))
                        return date;
                    // "D" is added by the message broker
                    if (date.substring(0, 1) == "D")
                        return moment(date.substring(1));
                    return moment(date);
                }
            }
        };

    });
