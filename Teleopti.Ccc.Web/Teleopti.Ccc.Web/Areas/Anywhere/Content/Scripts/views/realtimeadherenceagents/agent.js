define([
],
    function (
		) {
    	return function () {

    		var that = {};

    		that.fill = function (data) {
    			that.Name = data.Name;
    			that.State = data.State;
    			that.Activity = data.Activity;
    			that.NextActivity = data.NextActivity;
    			that.NextActivityStartTime = data.NextActivityStartTime;
    			that.Alarm = data.Alarm;
    			that.AlarmTime = data.AlarmTime;
    		};

    		return that;
    	};
    }
);