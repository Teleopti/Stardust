
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
		GotoPersonSchedule: function (buid, groupid, personid, date) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date));
		},

		GotoPersonScheduleWithoutHistory: function (buid, groupid, personid, date) {
			window.locationReplace('#personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date));
		},

		GotoPersonScheduleAddFullDayAbsenceForm: function (buid, groupid, personid, date) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/addfulldayabsence');
		},

		GotoPersonScheduleAddActivityForm: function (buid, groupid, personid, date) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/addactivity');
		},

		GotoPersonScheduleMoveActivityForm: function (buid, groupid, personid, date, startTime) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/moveactivity/' + startTime);
		},

		GotoPersonScheduleAddIntradayAbsenceForm: function (buid, groupid, personid, date) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/addintradayabsence');
		},

		GotoPersonScheduleWithAction: function (buid, groupid, personid, date, action) {
			window.setLocationHash('personschedule/' + buid + '/' + groupid + '/' + personid + '/' + toDateString(date) + '/' + action);
		},

		GoToTeamSchedule: function (buid, id, date, skill) {
			window.setLocationHash('teamschedule/' + buid + '/' + id + '/' + toDateString(date) + ((skill) ? '/' + skill.Id : ''));
		},

		GoToTeamScheduleOriginal: function (buid) {
			window.setLocationHash('teamschedule/' + buid);
		},

		GoToTeamScheduleForDate: function (buid, date) {
			window.setLocationHash('teamschedule/' + buid + '/' + toDateString(date));
		},

		GoToTeamScheduleWithSelectedPerson: function (buid, id, personId, date) {
			window.openInNewTab('teamschedule/' + buid + '/' + id + ((personId) ? '/' + personId : '') + '/' + toDateString(date));
		},

		GotoRealTimeAdherenceTeams: function (buid, siteId) {
			window.setLocationHash('realtimeadherenceteams/' + buid + '/' + siteId);
		},

		GotoRealTimeAdherenceTeamDetails: function (buid, teamId) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + teamId);
		},
		GotoRealTimeAdherenceMultipleTeamDetails: function (buid) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleTeams');
		},
		GotoRealTimeAdherenceMultipleSiteDetails: function(buid) {
			window.setLocationHash('realtimeadherenceagents/' + buid + '/' + 'MultipleSites');
		},
		GotoRealTimeAdherenceViewOriginal: function (buid) {
			window.setLocationHash('realtimeadherencesites/' + buid);
		}
	};
});
