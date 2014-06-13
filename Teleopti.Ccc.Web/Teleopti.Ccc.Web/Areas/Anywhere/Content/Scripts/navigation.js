
define([
        'moment',
        'window'
], function (
        moment,
		window
    ) {

	var toDateString = function (date) {
		if (moment.isMoment(date))
			date = date.format("YYYYMMDD");
		return date;
	};

	return {
		GotoPersonSchedule: function (groupid, personid, date) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date));
		},

		GotoPersonScheduleWithoutHistory: function (groupid, personid, date) {
			window.locationReplace('#personschedule/' + groupid + '/' + personid + '/' + toDateString(date));
		},

		GotoPersonScheduleAddFullDayAbsenceForm: function (groupid, personid, date) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addfulldayabsence');
		},

		GotoPersonScheduleAddActivityForm: function (groupid, personid, date) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity');
		},

		GotoPersonScheduleMoveActivityForm: function (groupid, personid, date, startTime) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/moveActivity/' + startTime);
		},

		GotoPersonScheduleAddIntradayAbsenceForm: function (groupid, personid, date) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/addintradayabsence');
		},

		GotoPersonScheduleWithAction: function (groupid, personid, date, action) {
			window.setLocationHash('personschedule/' + groupid + '/' + personid + '/' + toDateString(date) + '/' + action);
		},

		GoToTeamSchedule: function (id, date, skill) {
			window.setLocationHash('teamschedule/' + id + '/' + toDateString(date) + ((skill) ? '/' + skill.Id : ''));
		},

		GotoRealTimeAdherenceTeams: function (siteId) {
			window.setLocationHash('realtimeadherenceteams/' + siteId);
		},

		GotoRealTimeAdherenceTeamDetails: function (teamId) {
			window.setLocationHash('realtimeadherenceagents/' + teamId);
		}
	};
});
