
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
            GotoPersonScheduleAddFullDayAbsenceForm: function(id, date) {
                window.location.hash = 'personschedule/' + id + '/' + toDateString(date) + "/addfulldayabsence";
            },
            GoToTeamSchedule: function(id, date) {
                window.location.hash = 'teamschedule/' + id + '/' + toDateString(date);
            }
        };
    });
