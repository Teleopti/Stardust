define([
    'resources',
    'knockout',
    'helpers',
    'navigation'
],
    function (
        resources,
        ko,
        helpers,
        navigation
        ) {
        return function () {

        	var that = {};

        	that.PersonId = "";
        	that.Name = "";
        	that.TeamId = "";
        	that.TeamName = "";
        	that.SiteId = "";
        	that.SiteName = "";
            that.BusinessUnitId = "";

            that.TimeInState = ko.observable(0);
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

            that.TextWeight = ko.observable();
            that.TextColor = ko.observable();
            that.Selected = ko.observable(false);

            that.HistoricalAdherence = ko.observable(0);
            that.LastAdherenceUpdate = ko.observable();
            that.DisplayAdherencePercentage = ko.observable(false);

            that.SelectedToSendMessage = ko.observable(false);
            that.ScheduleChangeUrl = ko.observable();

	        that.fill = function (data) {
	            that.PersonId = data.PersonId;
	            that.Name = data.Name || that.Name;
            	that.TeamId = data.TeamId || that.TeamId;
            	that.TeamName = data.TeamName || that.TeamName;
            	that.SiteId = data.SiteId;
            	that.SiteName = data.SiteName;
	            that.BusinessUnitId = data.BusinessUnitId;
	            that.State(data.State);
	            that.TimeInState(data.TimeInState);
                that.Activity(data.Activity);
                that.NextActivity(data.NextActivity);
                that.NextActivityStartTime(data.NextActivityStartTime ? that.getDateTimeFormat(moment.utc(data.NextActivityStartTime).add(resources.TimeZoneOffsetMinutes, 'minutes')) : '');
				that.updateAlarmTime();

				that.AlarmStart(data.AlarmStart
	                ? moment.utc(data.AlarmStart)
	                    .add(resources.TimeZoneOffsetMinutes, 'minutes')
	                    .format(resources.FixedDateTimeWithSecondsFormatForMoment)
	                : '');

				if (that.shouldWaitWithUpdatingAlarm(data.AlarmStart)) {
                    that.HaveNewAlarm = true;
                    that.NextAlarm = data.Alarm;
                    that.NextAlarmColor = data.AlarmColor;
                    return;
                }
                that.Alarm(data.Alarm);
                that.refreshColor(data.AlarmColor);

                that.AdherenceDetailsUrl = navigation.UrlForAdherenceDetails(that.BusinessUnitId, that.PersonId);
	        };

	        that.updateAlarmTime = function() {
		        that.ScheduleChangeUrl(navigation.UrlForChangingSchedule(that.BusinessUnitId, that.TeamId, that.PersonId));
		        if (!that.TimeInState()) return;
		        if (that.Alarm() === '') that.AlarmTime('');
		        else {
			        var duration = moment.duration(that.TimeInState(), 'seconds');
			        that.AlarmTime(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));
		        }

		        if (!that.HaveNewAlarm
			        || that.shouldWaitWithUpdatingAlarm()) return;
		        that.Alarm(that.NextAlarm);
		        that.refreshColor(that.NextAlarmColor);
		        that.HaveNewAlarm = false;
	        };

	        that.increaseAlarmTime = function() {
		        if (!that.TimeInState()) return;

		        var duration = moment.duration(that.TimeInState(), 'seconds');
		        that.AlarmTime(Math.floor(duration.asHours()) + moment(duration.asMilliseconds()).format(":mm:ss"));
		        that.TimeInState(that.TimeInState() + 1);
	        };

	        that.shouldWaitWithUpdatingAlarm = function (alarmStartUtc) {
	        	return that.AlarmStart() && moment.utc().isBefore(alarmStartUtc);
            }

            that.refreshColor = function (newColor) {
            	if (newColor === "#000000") newColor = "#FFFFFF";
            	if (newColor !== undefined && that.color !== newColor) {
                    var rgb = that.hexToRgb(newColor);
                    that.AlarmColor('rgba(' + rgb + ', 0.6)');
                    that.TextColor(helpers.TextColor.BasedOnBackgroundColor('(' + rgb + ')'));
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
            	if (time.format("YYYYMMDD") > moment().format("YYYYMMDD")) {
            		return time.format(resources.DateTimeFormatForMoment);
                }
                return time.format(resources.TimeFormatForMoment);
            }

            return that;
        };
    }
);