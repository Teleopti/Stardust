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
    		that.Activity = ko.observable();
    		that.NextActivity = ko.observable();
    		that.NextActivityStartTime = ko.observable();

		    that.HaveNewAlarm = false;
    		that.NextAlarm = '';
		    that.NextAlarmColor = '';

		    that.Alarm = ko.observable();
    		that.AlarmColor = ko.observable();
    		that.AlarmStart = ko.observable();

    		that.EnteredCurrentAlarm = ko.observable();
    		that.TextWeight = ko.observable();

    		that.fill = function (data, name, offset) {
    			that.PersonId = data.PersonId;
				that.Name = name,
    			that.State(data.State);
				that.Activity(data.Activity);
				that.NextActivity(data.NextActivity);
				that.NextActivityStartTime(data.NextActivity && data.NextActivityStartTime ? that.getDateTimeFormat(moment.utc(data.NextActivityStartTime).add(offset, 'minutes')) : '');

				that.EnteredCurrentAlarm(data.StateStart ? moment.utc(data.StateStart).add(offset, 'minutes').format(resources.FixedDateTimeWithSecondsFormatForMoment) : '');
				that.refreshAlarmTime();

				that.AlarmStart(data.AlarmStart ? moment.utc(data.AlarmStart).add(offset, 'minutes').format(resources.FixedDateTimeWithSecondsFormatForMoment) : '');

				if (that.shouldWaitWithUpdatingAlarm()) {
					that.HaveNewAlarm = true;
			    	that.NextAlarm = data.Alarm;
				    that.NextAlarmColor = data.AlarmColor;
				    return;
			    }
    			that.Alarm(data.Alarm);
				that.refreshColor(data.AlarmColor);
    		};
			
    		that.refreshAlarmTime = function() {
			    if (!that.EnteredCurrentAlarm()) return;
			    var duration = moment.duration(((new Date).getTime() - moment(that.EnteredCurrentAlarm(), resources.FixedDateTimeWithSecondsFormatForMoment).toDate()));
			    that.AlarmTime(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));

			    if (!that.HaveNewAlarm
					|| that.shouldWaitWithUpdatingAlarm()) return;
			    that.Alarm(that.NextAlarm);
			    that.refreshColor(that.NextAlarmColor);
			    that.HaveNewAlarm = false;
    		}

    		that.shouldWaitWithUpdatingAlarm = function () {
    			return that.AlarmStart() && moment.utc().isBefore(that.AlarmStart());
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

			that.getDateTimeFormat = function(time) {
				if (time.date() > moment().date()) {
					return time.format(resources.FixedDateTimeWithSecondsFormatForMoment);
				}
				return time.format(resources.TimeFormatForMoment);
			}

    		return that;
    	};
    }
);