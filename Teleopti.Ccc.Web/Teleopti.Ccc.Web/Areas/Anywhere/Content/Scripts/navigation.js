
define([
        'moment'
    ], function(
        moment
    ) {

        var toDateString = function(date) {
            if (moment.isMoment(date))
                date = date.format("YYYYMMDD");
            return date;
        };

        return {
            GotoPersonSchedule: function (id, date) {
                window.location.hash = 'personschedule/' + id + '/' + toDateString(date);
            },
            GotoPersonScheduleWithoutHistory: function (id, date) {
            	window.location.replace('#personschedule/' + id + '/' + toDateString(date))	;
            },
            
            GotoPersonScheduleAddFullDayAbsenceForm: function (id, date) {
                window.location.hash = 'personschedule/' + id + '/' + toDateString(date) + '/addfulldayabsence';
            },
            GotoPersonScheduleAddFullDayAbsenceFormWithoutHistory: function (id, date) {
                window.location.replace('#personschedule/' + id + '/' + toDateString(date) + '/addfulldayabsence');
            },

            GotoPersonScheduleAddActivityForm: function (id, date) {
            	window.location.hash = 'personschedule/' + id + '/' + toDateString(date) + '/addactivity';
            },
            
            GoToTeamSchedule: function (id, date, skill) {
            	window.location.hash = 'teamschedule/' + id + '/' + toDateString(date) + ((skill) ? '/' + skill.Id : '');
            }
        };
    });
