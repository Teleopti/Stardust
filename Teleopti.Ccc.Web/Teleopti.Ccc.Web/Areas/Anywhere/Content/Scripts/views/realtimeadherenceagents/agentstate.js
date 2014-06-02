define([
	'resources',
	'knockout'
],
    function (resources, ko
		) {
    	return function () {

    		var that = {};
    		that.TimeInState = ko.observable();
    		that.AlarmTime = ko.observable();
    		that.State = ko.observable();
    		that.StateStart = ko.observable();
    		that.Activity = ko.observable();
    		that.NextActivity = ko.observable();
    		that.NextActivityStartTime = ko.observable();
    		that.Alarm = ko.observable();
    		that.AlarmColor = ko.observable();
    		that.AlarmStart = ko.observable();
    		that.TextWeight = ko.observable();
    		that.StateStart = ko.observable();

    		that.fill = function (data, name, offset) {
    			that.PersonId = data.PersonId;
				that.Name = name,
    			that.State(data.State);
				that.StateStart(data.StateStart ? moment.utc(data.StateStart).add(offset, 'minutes').format(resources.DateTimeFormatForMoment): '');
				that.Activity(data.Activity);
				that.NextActivity(data.NextActivity);
    			that.NextActivityStartTime( data.NextActivityStartTime ? (moment.utc(data.NextActivityStartTime).add(offset, 'minutes').format(resources.TimeFormatForMoment)) : '');
    			that.Alarm(data.Alarm);
    			
    			that.refreshColor(data.AlarmColor);
    			that.AlarmStart(data.AlarmStart ? moment.utc(data.AlarmStart).add(offset, 'minutes').format(resources.DateTimeFormatForMoment) : '');
				that.refreshAlarmTime();
				that.refreshTimeInState();
		    };

    		that.refreshTimeInState = function () {
			    if (!that.StateStart()) return;
    			var duration = moment.duration(((new Date).getTime() - moment(that.StateStart(), resources.DateTimeFormatForMoment).toDate()));
    			that.TimeInState(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));
    		}
			
			that.refreshAlarmTime = function () {
				if (!that.AlarmStart()) return;
    			var duration = moment.duration(((new Date).getTime() - moment(that.AlarmStart(), resources.DateTimeFormatForMoment).toDate()));
    			that.AlarmTime(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));
			}

			that.refreshColor = function (newColor) {
				if (that.color !== newColor) {
					that.AlarmColor('rgba(' + that.hexToRgb(newColor) + ', 0.6)');
					that.TextWeight(700);
					setTimeout(function () {
						that.TextWeight(500);
					}, 4000);
					that.color = newColor;
				}
				
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