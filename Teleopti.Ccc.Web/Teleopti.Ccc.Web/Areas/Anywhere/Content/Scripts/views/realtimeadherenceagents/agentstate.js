define([
	'resources',
	'knockout'
],
    function (resources, ko
		) {
    	return function () {

    		var that = {};
    		that.TimeInState = ko.observable();
    		that.fill = function (data, name, offset) {
    			that.PersonId = data.PersonId;
				that.Name = name,
    			that.State = data.State;
    			that.Activity = data.Activity;
    			that.NextActivity = data.NextActivity;
    			that.NextActivityStartTime = moment.utc(data.NextActivityStartTime).add(offset, 'minutes').format(resources.TimeFormatForMoment);
    			that.Alarm = data.Alarm;
    			that.AlarmColor = 'rgba('+that.hexToRgb(data.AlarmColor) + ', 0.6)';
    			that.AlarmTime = moment.utc(data.AlarmTime).add(offset, 'minutes').format(resources.DateTimeFormatForMoment);
			    that.refreshTimeInState();
		    };

    		that.refreshTimeInState = function () {
    			var duration = moment.duration(((new Date).getTime() - moment(that.AlarmTime, resources.DateTimeFormatForMoment).toDate()));
    			that.TimeInState(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));
    		}

    		that.hexToRgb = function (hex) {
				
			    hex = hex ? hex.substring(1) : 'ffffff';
    			var bigint = parseInt(hex, 16);
    			var r = (bigint >> 16) & 255;
    			var g = (bigint >> 8) & 255;
    			var b = bigint & 255;

    			return r + "," + g + "," + b;
    		}

    		return that;
    	};
    }
);