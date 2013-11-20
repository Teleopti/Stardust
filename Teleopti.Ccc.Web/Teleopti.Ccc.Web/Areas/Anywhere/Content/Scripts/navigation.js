
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
        	GotoPersonSchedule: function (groupid, personid, date) {
        		window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date);
            },
            GotoPersonScheduleWithoutHistory: function (personid, date) {
            	window.location.replace('#personschedule/' + personid + '/' + toDateString(date));
            },
            
            GotoPersonScheduleAddFullDayAbsenceForm: function (groupid, personid, date) {
            	window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addfulldayabsence';
            },
        	
            GotoPersonScheduleAddFullDayAbsenceFormWithoutHistory: function (personid, date) {
            	window.location.replace('#personschedule/' + personid + '/' + toDateString(date) + '/addfulldayabsence');
            },

            GotoPersonScheduleAddActivityForm: function (groupid, personid, date) {
            	window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity';
            },
            
            GotoPersonScheduleAddAbsenceForm: function (groupid, personid, date) {
            	window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addabsence';
            },
        	
            GotoPersonScheduleWithAction: function (groupid, personid, date, action) {
            	window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/' + action;
            },
            
            GoToTeamSchedule: function (id, date, skill) {
            	window.location.hash = 'teamschedule/' + id + '/' + toDateString(date) + ((skill) ? '/' + skill.Id : '');
            }
        };
    });
