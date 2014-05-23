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
    			that.NextActivityStartTime = moment(data.NextActivityStartTime).add(offset,'minutes').format(resources.DateTimeFormatForMoment);
    			that.Alarm = data.Alarm;
			    that.AlarmColor = data.AlarmColor;
			    that.AlarmTime = moment(data.AlarmTime).add(offset, 'minutes').format(resources.DateTimeFormatForMoment);
			    that.refreshTimeInState();
		    };

    		that.refreshTimeInState = function () {
    			var duration = moment.duration(((new Date).getTime() - moment(that.AlarmTime, resources.DateTimeFormatForMoment).toDate()));
    			that.TimeInState(Math.floor(duration.asHours()) + moment.utc(duration.asMilliseconds()).format(":mm:ss"));
		    }

    		return that;
    	};
    }
);