define([
    'resources',
    'knockout',
    'helpers'
],
    function (resources, ko, helpers
        ) {
        return function () {

        	var that = {};

        	that.PersonId = ko.observable();
        	that.Name = ko.observable();
        	that.TeamName = ko.observable();

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
            that.TextColor = ko.observable();
            that.Selected = ko.observable(false);

            that.HistoricalAdherence = ko.observable(0);
            that.LastAdherenceUpdate = ko.observable();
            that.DisplayAdherencePercentage = ko.observable(false);

            that.SelectedToSendMessage = ko.observable(false);

	        that.fill = function (data) {
            	that.PersonId(data.PersonId);
            	that.Name(data.Name);
            	that.TeamName(data.TeamName);
                that.State(data.State);
                that.Activity(data.Activity);
                that.NextActivity(data.NextActivity);
                that.NextActivityStartTime(data.NextActivityStartTime ? that.getDateTimeFormat(moment.utc(data.NextActivityStartTime).add(resources.TimeZoneOffsetMinutes, 'minutes')) : '');
				that.EnteredCurrentAlarm(data.StateStart);
                that.refreshAlarmTime();

                that.AlarmStart(data.AlarmStart ? moment.utc(data.AlarmStart).add(resources.TimeZoneOffsetMinutes, 'minutes').format(resources.FixedDateTimeWithSecondsFormatForMoment) : '');

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
                var duration = moment.duration(((new Date).getTime() - moment.utc(that.EnteredCurrentAlarm())));
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
            	if (newColor !== undefined && newColor !== "#000000" && that.color !== newColor) {
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