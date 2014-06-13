define(
    [
        'moment',
        'resources'
    ], function (
        momentX,
        resources
    ) {

    	return {
    		TextColor: {
    			BasedOnBackgroundColor: function (backgroundColor) {
    				backgroundColor = backgroundColor.slice(backgroundColor.indexOf('(') + 1, backgroundColor.indexOf(')'));

    				var backgroundColorArr = backgroundColor.split(',');

    				var brightness = backgroundColorArr[0] * 0.299 + backgroundColorArr[1] * 0.587 + backgroundColorArr[2] * 0.114;

    				return brightness < 100 ? 'rgb(255,255,255)' : 'rgb(0,0,0)';
    			},
    			HexToRgb: function(hex) {
    				var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    				var rgb= result ? {
    					r: parseInt(result[1], 16),
    					g: parseInt(result[2], 16),
    					b: parseInt(result[3], 16)
    				} : null;
    				if (rgb)
    					return "rgb(" + rgb.r + "," + rgb.g + "," + rgb.b + ")";
    				return rgb;
    			}
    		},
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
            },
					timeFormatForUrl: function(time) {
						return moment(time, "HH:mm").format("HHmm").toString();
					}
    	};
    });
