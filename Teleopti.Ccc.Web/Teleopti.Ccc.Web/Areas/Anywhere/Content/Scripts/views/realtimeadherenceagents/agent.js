define([
	'resources',
	'knockout'
],
    function (resources, ko
		) {
    	return function () {

    		var that = {};
    		that.TimeInState = ko.observable();
    		that.fill = function (data) {
    			that.Name = data.Name;
    			that.State = data.State;
    			that.Activity = data.Activity;
    			that.NextActivity = data.NextActivity;
    			that.NextActivityStartTime = moment(data.NextActivityStartTime).format(resources.DateTimeFormatForMoment);
    			that.Alarm = data.Alarm;
			    that.AlarmColor = data.AlarmColor;
    			that.AlarmTime = moment(data.AlarmTime).format(resources.DateTimeFormatForMoment);
			    that.refreshTimeInState();
		    };

    		that.refreshTimeInState = function () {
    			var duration = moment.duration(((new Date).getTime() - new Date(that.AlarmTime)));
    			that.TimeInState(Math.floor(duration.asHours()) + moment.utc(duration.asMilliseconds()).format(":mm:ss"));
		    }

    		return that;
    	};
    }
);