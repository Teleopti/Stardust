define([
	'resources'
],
    function (resources
		) {
    	return function () {

    		var that = {};

    		that.fill = function (data) {
    			that.Name = data.Name;
    			that.State = data.State;
    			that.Activity = data.Activity;
    			that.NextActivity = data.NextActivity;
    			that.NextActivityStartTime = moment(data.NextActivityStartTime).format(resources.DateTimeFormatForMoment);
    			that.Alarm = data.Alarm;
    			that.AlarmTime = moment(data.AlarmTime).format(resources.DateTimeFormatForMoment);
    			that.TimeInState = moment((Date.now()-new Date(data.AlarmTime))).minute();
		    };

    		return that;
    	};
    }
);