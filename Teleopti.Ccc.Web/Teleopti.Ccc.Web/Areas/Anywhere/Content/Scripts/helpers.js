define(
    [
        'moment'
    ], function(
        momentX
    ) {

        return {
            Minutes: {
                FromHours: function(hours) {
                    return hours * 60;
                },

                StartOfHour: function(minutes) {
                    return minutes - (minutes % 60);
                },

                EndOfHour: function(minutes) {
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
                        return date.format("YYYY-MM-DD");
                    if (date.getMonth)
                        return date.toString('yyyy-MM-dd');
                    return date;
                    
                    // recreate the date as UTC, then convert it to local clock, and send it to the server which will then convert it correctly
                    return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
                }
            }
        };

    });
