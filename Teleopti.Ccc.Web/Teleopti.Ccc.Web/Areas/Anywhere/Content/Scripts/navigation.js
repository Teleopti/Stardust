
define([
        'moment'
], function (
        moment
    ) {

    var toDateString = function (date) {
        if (moment.isMoment(date))
            date = date.format("YYYYMMDD");
        return date;
    };

    return {
        GotoPersonSchedule: function (groupid, personid, date) {
            window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date);
        },

        GotoPersonScheduleWithoutHistory: function (groupid, personid, date) {
            window.location.replace('#personschedule/' + groupid + '/' + personid + '/' + toDateString(date));
        },

        GotoPersonScheduleAddFullDayAbsenceForm: function (groupid, personid, date) {
            window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addfulldayabsence';
        },

        GotoPersonScheduleAddActivityForm: function (groupid, personid, date) {
            window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity';
        },

        GotoPersonScheduleAddIntradayAbsenceForm: function (groupid, personid, date) {
            window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addintradayabsence';
        },

        GotoPersonScheduleWithAction: function (groupid, personid, date, action) {
            window.location.hash = 'personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/' + action;
        },

        GoToTeamSchedule: function (id, date, skill) {
            window.location.hash = 'teamschedule/' + id + '/' + toDateString(date) + ((skill) ? '/' + skill.Id : '');
        }
    };
});
